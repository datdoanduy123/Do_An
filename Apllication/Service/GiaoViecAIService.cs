using Apllication.DTOs;
using Apllication.DTOs.CongViec;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class GiaoViecAIService : IGiaoViecAIService
    {
        private readonly ICongViecRepository _congViecRepo;
        private readonly INguoiDungRepository _nguoiDungRepo;
        private readonly IQuyTacGiaoViecAIRepository _ruleRepo;
        private readonly IDuAnRepository _duAnRepo;
        private readonly ISprintRepository _sprintRepo;
        private readonly IKanbanNotificationService _notificationService;

        public GiaoViecAIService(
            ICongViecRepository congViecRepo,
            INguoiDungRepository nguoiDungRepo,
            IQuyTacGiaoViecAIRepository ruleRepo,
            IDuAnRepository duAnRepo,
            ISprintRepository sprintRepo,
            IKanbanNotificationService notificationService)
        {
            _congViecRepo = congViecRepo;
            _nguoiDungRepo = nguoiDungRepo;
            _ruleRepo = ruleRepo;
            _duAnRepo = duAnRepo;
            _sprintRepo = sprintRepo;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<GoiYGiaoViecDto>> GoiYAssigneeAsync(int congViecId)
        {
            var task = await _congViecRepo.GetByIdAsync(congViecId);
            if (task == null) return new List<GoiYGiaoViecDto>();

            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            double skillWeight = GetRuleValue(rules, "SKILL_MATCH_WEIGHT", 0.6);
            double experienceWeight = GetRuleValue(rules, "EXPERIENCE_WEIGHT", 0.4);
            double workloadPenalty = GetRuleValue(rules, "WORKLOAD_PENALTY", 0.1);
            double minScore = GetRuleValue(rules, "MIN_MATCH_SCORE", 0.3);

            var projectMembers = await _duAnRepo.GetMembersAsync(task.DuAnId);
            var candidates = projectMembers.Select(m => m.NguoiDung).ToList();

            var recommendations = new List<GoiYGiaoViecDto>();

            foreach (var userDto in candidates)
            {
                var user = await _nguoiDungRepo.LayTheoIdAsync(userDto.Id);
                if (user == null) continue;

                if (user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN")) continue;

                double workload = await GetUserWorkloadHoursAsync(user.Id);
                var matchResult = CalculateMatchScore(task, user, skillWeight, experienceWeight, workloadPenalty, workload);

                if (matchResult.Score >= minScore)
                {
                    recommendations.Add(new GoiYGiaoViecDto
                    {
                        UserId = user.Id,
                        HoTen = user.FullName,
                        DiemPhuHop = Math.Round(matchResult.Score, 2),
                        LyDo = matchResult.Reason,
                        KyNangPhuHop = matchResult.MatchedSkills
                    });
                }
            }

            return recommendations.OrderByDescending(x => x.DiemPhuHop).Take(5);
        }

        public async Task<bool> TuDongGiaoViecDuAnAsync(int duAnId)
        {
            var query = new CongViecQueryDto { DuAnId = duAnId, PageSize = 1000 };
            var pagedResult = await _congViecRepo.LayDanhSachCongViecAsync(query);
            var allTasksInProject = pagedResult.Items.ToList();

            var sortedTasks = SortTasksTopologically(allTasksInProject)
                .Where(t => t.AssigneeId == null || t.SprintId == null)
                .ToList();

            if (!sortedTasks.Any()) return true;

            var currentSprint = await GetOrCreateActiveSprintAsync(duAnId);
            var rules = await _ruleRepo.GetAllActiveRulesAsync();

            var projectMembers = await _duAnRepo.GetMembersAsync(duAnId);
            var memberIds = projectMembers.Select(m => m.NguoiDung.Id).ToList();
            var cachedUsers = new Dictionary<int, User>();
            foreach (var memberId in memberIds)
            {
                var u = await _nguoiDungRepo.LayTheoIdAsync(memberId);
                if (u != null) cachedUsers[u.Id] = u;
            }

            var tempWorkload = new Dictionary<int, double>();

            foreach (var tDto in sortedTasks)
            {
                var task = await _congViecRepo.GetByIdAsync(tDto.Id);
                if (task == null) continue;

                var suggestions = await GoiYAssigneeWithTempWorkloadAsync(
                    task, tempWorkload, task.SprintId ?? currentSprint.Id,
                    allTasksInProject, rules, cachedUsers);
                
                var bestMatch = suggestions.FirstOrDefault();

                if (bestMatch != null)
                {
                    task.AssigneeId = bestMatch.UserId;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.AI;
                    task.AiMatchScore = bestMatch.DiemPhuHop;
                    task.AiReasoning = bestMatch.LyDo;
                    task.TrangThai = TrangThaiCongViec.Todo;

                    tempWorkload[bestMatch.UserId] = tempWorkload.GetValueOrDefault(bestMatch.UserId) + task.ThoiGianUocTinh;
                }
                else
                {
                    task.AssigneeId = task.CreatedBy;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.Manual;
                    task.AiMatchScore = 0;
                    task.AiReasoning = "AI không tìm thấy ứng viên phù hợp hoặc quá tải. Giao lại PM xử lý.";
                }

                if (task.SprintId == null) task.SprintId = currentSprint.Id;
                await _congViecRepo.UpdateAsync(task);

                if (task.AssigneeId.HasValue)
                {
                    await _notificationService.NotifyPersonal(task.AssigneeId.Value, "AI Giao việc", $"AI vừa giao cho bạn: {task.TieuDe}. Trình tự đã tối ưu theo Dependency.");
                }
            }

            await LapKeHoachTimelineDuAnAsync(duAnId);
            await _notificationService.NotifyTaskUpdated(duAnId);
            return true;
        }

        private IEnumerable<CongViec> SortTasksTopologically(List<CongViec> tasks)
        {
            var taskDict = tasks.ToDictionary(t => t.Id);
            var adj = new Dictionary<int, List<int>>();
            var inDegree = new Dictionary<int, int>();

            foreach (var task in tasks)
            {
                if (!adj.ContainsKey(task.Id)) adj[task.Id] = new List<int>();
                if (!inDegree.ContainsKey(task.Id)) inDegree[task.Id] = 0;
            }

            foreach (var task in tasks)
            {
                foreach (var dep in task.Dependencies)
                {
                    if (taskDict.ContainsKey(dep.DependsOnTaskId))
                    {
                        adj[dep.DependsOnTaskId].Add(task.Id);
                        inDegree[task.Id]++;
                    }
                }
            }

            var queue = new Queue<int>(inDegree.Where(x => x.Value == 0).Select(x => x.Key));
            var sorted = new List<CongViec>();

            while (queue.Any())
            {
                var u = queue.Dequeue();
                if (taskDict.TryGetValue(u, out var t)) sorted.Add(t);

                foreach (var v in adj[u])
                {
                    inDegree[v]--;
                    if (inDegree[v] == 0) queue.Enqueue(v);
                }
            }

            var remaining = tasks.Where(t => !sorted.Any(s => s.Id == t.Id)).OrderBy(t => t.ViTri);
            sorted.AddRange(remaining);

            return sorted;
        }

        private async Task LapKeHoachTimelineDuAnAsync(int duAnId)
        {
            var query = new CongViecQueryDto { DuAnId = duAnId, PageSize = 1000 };
            var pagedTasks = await _congViecRepo.LayDanhSachCongViecAsync(query);
            var allTasks = pagedTasks.Items.ToList();

            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            double bufferRate = GetRuleValue(rules, "BUFFER_RATE", 0.2);

            var tasksBySprint = allTasks.Where(t => t.SprintId != null).GroupBy(t => t.SprintId);

            foreach (var sprintGroup in tasksBySprint)
            {
                var sprint = await _sprintRepo.GetByIdAsync(sprintGroup.Key!.Value);
                if (sprint == null) continue;

                var tasksByAssignee = sprintGroup.Where(t => t.AssigneeId != null).GroupBy(t => t.AssigneeId);

                foreach (var userGroup in tasksByAssignee)
                {
                    var sortedTasks = userGroup.OrderBy(t => t.Dependencies.Any() ? 1 : 0).ThenBy(t => t.ViTri).ToList();
                    DateTime currentPointer = new DateTime[] { sprint.NgayBatDau, DateTime.UtcNow.Date }.Max();
                    currentPointer = SkipWeekends(currentPointer);

                    foreach (var tDto in sortedTasks)
                    {
                        var task = await _congViecRepo.GetByIdAsync(tDto.Id);
                        if (task == null) continue;

                        DateTime taskStart = currentPointer;

                        if (task.Dependencies != null && task.Dependencies.Any())
                        {
                            foreach (var dep in task.Dependencies)
                            {
                                var predecessor = await _congViecRepo.GetByIdAsync(dep.DependsOnTaskId);
                                if (predecessor == null) continue;

                                if (predecessor.TrangThai != TrangThaiCongViec.Done && predecessor.SprintId < task.SprintId)
                                {
                                    var rolloverTask = allTasks.FirstOrDefault(t => 
                                        t.SprintId == task.SprintId && 
                                        t.TieuDe.Equals(predecessor.TieuDe, StringComparison.OrdinalIgnoreCase));
                                    
                                    if (rolloverTask != null && rolloverTask.NgayKetThucDuKien.HasValue) predecessor = rolloverTask;
                                }

                                if (predecessor.NgayKetThucDuKien.HasValue)
                                {
                                    DateTime minStart = SkipWeekends(predecessor.NgayKetThucDuKien.Value.AddDays(1));
                                    if (minStart > taskStart) taskStart = minStart;
                                }
                            }
                        }

                        task.NgayBatDauDuKien = SkipWeekends(taskStart);
                        int hrs = (int)Math.Ceiling(task.ThoiGianUocTinh * (1 + bufferRate));
                        DateTime eP = task.NgayBatDauDuKien.Value;

                        while (hrs > 0)
                        {
                            if (hrs <= 8) hrs = 0;
                            else { hrs -= 8; eP = SkipWeekends(eP.AddDays(1)); }
                        }

                        task.NgayKetThucDuKien = eP;
                        if (task.NgayKetThucDuKien > sprint.NgayKetThuc)
                        {
                            task.NgayKetThucDuKien = sprint.NgayKetThuc;
                            task.AiReasoning += " ⚠️ Out of Sprint deadline.";
                        }

                        currentPointer = SkipWeekends(task.NgayKetThucDuKien.Value.AddDays(1));
                        await _congViecRepo.UpdateAsync(task);
                    }
                }
            }
        }

        private async Task<IEnumerable<GoiYGiaoViecDto>> GoiYAssigneeWithTempWorkloadAsync(
            CongViec task, Dictionary<int, double> tempWorkload, int sprintId, IEnumerable<CongViec> allTasks,
            IEnumerable<QuyTacGiaoViecAI> rules, Dictionary<int, User> cachedUsers)
        {
            double sWeight = GetRuleValue(rules, "SKILL_MATCH_WEIGHT", 0.6);
            double eWeight = GetRuleValue(rules, "EXPERIENCE_WEIGHT", 0.4);
            double wPenalty = GetRuleValue(rules, "WORKLOAD_PENALTY", 0.1);
            double minScore = GetRuleValue(rules, "MIN_MATCH_SCORE", 0.3);
            double maxWorkload = GetRuleValue(rules, "MAX_WORKLOAD_HOURS", 40.0);

            var recs = new List<GoiYGiaoViecDto>();

            foreach (var kvp in cachedUsers)
            {
                var user = kvp.Value;
                if (user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN")) continue;

                double sprintWorkload = allTasks.Where(t => t.AssigneeId == user.Id && t.SprintId == sprintId && t.TrangThai != TrangThaiCongViec.Done).Sum(t => t.ThoiGianUocTinh);
                double currentWL = sprintWorkload + tempWorkload.GetValueOrDefault(user.Id);

                if (currentWL >= maxWorkload) continue;

                double wsBonus = 0;
                if (task.Dependencies != null && task.Dependencies.Any())
                {
                    foreach (var dep in task.Dependencies)
                    {
                        var pred = allTasks.FirstOrDefault(t => t.Id == dep.DependsOnTaskId);
                        if (pred != null && pred.AssigneeId == user.Id) { wsBonus = 0.2; break; }
                    }
                }

                var res = CalculateMatchScore(task, user, sWeight, eWeight, wPenalty, currentWL, wsBonus);
                if (res.Score >= minScore)
                {
                    recs.Add(new GoiYGiaoViecDto { UserId = user.Id, HoTen = user.FullName, DiemPhuHop = Math.Round(res.Score, 2), LyDo = res.Reason + (wsBonus > 0 ? " (Workstream Bonus)" : ""), KyNangPhuHop = res.MatchedSkills });
                }
            }
            return recs.OrderByDescending(x => x.DiemPhuHop).Take(5);
        }

        private (double Score, string Reason, List<string> MatchedSkills) CalculateMatchScore(
            CongViec task, User user, double sWeight, double eWeight, double wPenalty, double currentWL, double wsBonus = 0)
        {
            double skillScore = 0; double expScore = 0;
            var matchedSkills = new List<string>();
            var reason = new StringBuilder();

            var userSkills = user.KyNangNguoiDungs ?? new List<KyNangNguoiDung>();
            var reqs = task.YeuCauCongViecs ?? new List<YeuCauCongViec>();

            if (reqs.Any())
            {
                foreach (var req in reqs)
                {
                    var us = userSkills.FirstOrDefault(s => s.KyNangId == req.KyNangId);
                    if (us != null)
                    {
                        skillScore += us.Level >= req.MucDoYeuCau ? 1.0 : (double)us.Level / req.MucDoYeuCau;
                        matchedSkills.Add(us.KyNang.TenKyNang);
                        expScore += Math.Min(us.SoNamKinhNghiem / 5.0, 1.0);
                    }
                }
                skillScore /= reqs.Count; expScore /= reqs.Count;
            }
            else
            {
                var words = (task.TieuDe + " " + (task.MoTa ?? "")).Split(new[] { ' ', '-', '_', '.', '/' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var sk in userSkills)
                {
                    var skillName = sk.KyNang.TenKyNang;
                    if (words.Any(w => w.Contains(skillName, StringComparison.OrdinalIgnoreCase)))
                    {
                        skillScore += (sk.Level / 5.0);
                        matchedSkills.Add(sk.KyNang.TenKyNang);
                        expScore += Math.Min(sk.SoNamKinhNghiem / 5.0, 1.0);
                    }
                }
                if (matchedSkills.Any()) { skillScore /= matchedSkills.Count; expScore /= matchedSkills.Count; }
            }

            double totalScore = (skillScore * sWeight) + (expScore * eWeight) + wsBonus - ((currentWL / 8.0) * wPenalty);
            if (currentWL >= 40) reason.Append("Khá bận. ");
            reason.Append(matchedSkills.Any() ? $"Khớp: {string.Join(", ", matchedSkills)}. " : "Cần đào tạo thêm.");

            return (Math.Max(0, totalScore), reason.ToString().Trim(), matchedSkills);
        }

        private async Task<double> GetUserWorkloadHoursAsync(int userId)
        {
            var tasks = await _congViecRepo.LayDanhSachCongViecAsync(new CongViecQueryDto { AssigneeId = userId, PageSize = 1000 });
            return tasks.Items.Where(t => t.TrangThai != TrangThaiCongViec.Done).Sum(t => t.ThoiGianUocTinh);
        }

        private DateTime SkipWeekends(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) date = date.AddDays(1);
            return date;
        }

        private double GetRuleValue(IEnumerable<QuyTacGiaoViecAI> rules, string code, double defaultValue)
        {
            var rule = rules.FirstOrDefault(r => r.MaQuyTac == code);
            if (rule != null && double.TryParse(rule.GiaTri, out double val)) return val;
            return defaultValue;
        }

        public async Task<Sprint> GetOrCreateSprintByModuleNameAsync(int duAnId, string moduleName)
        {
            var sprints = await _sprintRepo.GetByProjectIdAsync(duAnId);
            var existing = sprints.FirstOrDefault(s => s.TenSprint == moduleName);
            if (existing != null) return existing;
            var duAn = await _duAnRepo.GetByIdAsync(duAnId);
            DateTime start = sprints.Any() ? sprints.Max(s => s.NgayKetThuc) : (duAn?.NgayBatDau ?? DateTime.UtcNow);
            return await _sprintRepo.AddAsync(new Sprint { DuAnId = duAnId, TenSprint = moduleName, NgayBatDau = start, NgayKetThuc = start.AddDays(14), TrangThai = TrangThaiSprint.New });
        }

        private async Task<Sprint> GetOrCreateActiveSprintAsync(int duAnId)
        {
            var sprints = await _sprintRepo.GetByProjectIdAsync(duAnId);
            var active = sprints.FirstOrDefault(s => s.TrangThai == TrangThaiSprint.InProgress) ?? sprints.FirstOrDefault(s => s.TrangThai == TrangThaiSprint.New);
            if (active != null) return active;
            var duAn = await _duAnRepo.GetByIdAsync(duAnId);
            DateTime start = duAn?.NgayBatDau ?? DateTime.UtcNow;
            return await _sprintRepo.AddAsync(new Sprint { DuAnId = duAnId, TenSprint = "Sprint 1", NgayBatDau = start, NgayKetThuc = start.AddDays(14), TrangThai = TrangThaiSprint.New });
        }
    }
}
