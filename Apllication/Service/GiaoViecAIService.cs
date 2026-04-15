using Apllication.DTOs;
using Apllication.DTOs.CongViec;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // Lấy danh sách thành viên dự án
            var projectMembers = await _duAnRepo.GetMembersAsync(task.DuAnId);
            var cachedUsers = new Dictionary<int, User>();
            foreach (var m in projectMembers)
            {
                var u = await _nguoiDungRepo.LayTheoIdAsync(m.NguoiDung.Id);
                if (u != null) cachedUsers[u.Id] = u;
            }

            // Lấy yêu cầu kỹ năng của task
            var reqs = task.YeuCauCongViecs ?? new List<YeuCauCongViec>();

            // Ngưỡng khoảng cách tối đa cho phép (nếu vượt = không phù hợp)
            double maxAllowedDistance = reqs.Any() ? Math.Sqrt(reqs.Count * 25.0) : double.MaxValue;

            // Tính khoảng cách KNN cho từng user – trả về Top 5 để PM tham khảo
            var result = new List<GoiYGiaoViecDto>();

            foreach (var kvp in cachedUsers)
            {
                var user = kvp.Value;
                // Loại trừ QUAN_LY / ADMIN khỏi danh sách ứng viên
                if (user.NguoiDungVaiTros != null &&
                    user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN"))
                    continue;

                var userSkills = user.KyNangNguoiDungs ?? new List<KyNangNguoiDung>();

                if (!reqs.Any())
                {
                    // Task không có yêu cầu kỹ năng → cho vào danh sách với điểm trung bình
                    result.Add(new GoiYGiaoViecDto
                    {
                        UserId = user.Id,
                        HoTen = user.FullName,
                        DiemPhuHop = 0.5,
                        LyDo = "Task không yêu cầu kỹ năng cụ thể.",
                        KyNangPhuHop = new List<string>()
                    });
                    continue;
                }

                // Tính khoảng cách Euclidean giữa vector kỹ năng User và yêu cầu Task
                double sumSquaredDiff = 0;
                var matchedSkills = new List<string>();
                foreach (var req in reqs)
                {
                    double taskReq = req.MucDoYeuCau;
                    var userSkill = userSkills.FirstOrDefault(s => s.KyNangId == req.KyNangId);
                    double userLevel = userSkill?.Level ?? 0;
                    if (userSkill != null) matchedSkills.Add(userSkill.KyNang?.TenKyNang ?? "");

                    // Chỉ phạt khi user thiếu kỹ năng, không phạt khi dư kỹ năng
                    double diff = Math.Max(0, taskReq - userLevel);
                    sumSquaredDiff += diff * diff;
                }

                double distance = Math.Sqrt(sumSquaredDiff);

                // Chuyển khoảng cách thành điểm phù hợp (0-1): distance=0 → score=1, distance=max → score=0
                double score = maxAllowedDistance > 0 ? Math.Max(0, 1 - distance / maxAllowedDistance) : 0;

                result.Add(new GoiYGiaoViecDto
                {
                    UserId = user.Id,
                    HoTen = user.FullName,
                    DiemPhuHop = Math.Round(score, 2),
                    LyDo = matchedSkills.Any()
                        ? $"KNN khớp kỹ năng: {string.Join(", ", matchedSkills.Where(s => !string.IsNullOrEmpty(s)))}."
                        : "Chưa có kỹ năng phù hợp với task này.",
                    KyNangPhuHop = matchedSkills
                });
            }

            // Trả về Top 5 người phù hợp nhất (điểm cao nhất)
            return result.OrderByDescending(x => x.DiemPhuHop).Take(5);
        }

        public async Task<bool> TuDongGiaoViecDuAnAsync(int duAnId)
        {
            var query = new CongViecQueryDto { DuAnId = duAnId, PageSize = 1000 };
            var pagedResult = await _congViecRepo.LayDanhSachCongViecAsync(query);
            var allTasksInProject = pagedResult.Items.ToList();

            // Sắp xếp task theo thứ tự Topological (dependency trước), chỉ lấy task chưa có người làm
            var sortedTasks = SortTasksTopologically(allTasksInProject)
                .Where(t => t.AssigneeId == null || t.SprintId == null)
                .ToList();

            if (!sortedTasks.Any()) return true;

            var rules = await _ruleRepo.GetAllActiveRulesAsync();

            var currentSprint = await GetOrCreateActiveSprintAsync(duAnId, rules);

            // Lấy thông tin dự án để lấy PM (người tạo) cho fallback
            var duAn = await _duAnRepo.GetByIdAsync(duAnId);
            int? pmId = duAn?.CreatedBy;

            // Lấy danh sách thành viên dự án (loại trừ QUAN_LY/ADMIN khi chạy KNN)
            var projectMembers = await _duAnRepo.GetMembersAsync(duAnId);
            var memberIds = projectMembers.Select(m => m.NguoiDung.Id).ToList();
            var cachedUsers = new Dictionary<int, User>();
            foreach (var memberId in memberIds)
            {
                var u = await _nguoiDungRepo.LayTheoIdAsync(memberId);
                if (u != null) cachedUsers[u.Id] = u;
            }

            // Tính số giờ hiện tại của mỗi người để làm gốc (không thể chỉ đếm tạm thời từ 0)
            double defaultEstimate = GetRuleValue(rules, "DEFAULT_TASK_ESTIMATE", 8.0);
            int topK = (int)GetRuleValue(rules, "KNN_TOP_K", 3);

            var userWorkloadHours = new Dictionary<int, double>();
            foreach (var kvp in cachedUsers)
            {
                double currentHours = allTasksInProject
                     .Where(t => t.AssigneeId == kvp.Key && t.TrangThai != TrangThaiCongViec.Done)
                     .Sum(t => t.ThoiGianUocTinh > 0 ? t.ThoiGianUocTinh : defaultEstimate);
                userWorkloadHours[kvp.Key] = currentHours;
            }

            foreach (var tDto in sortedTasks)
            {
                var task = await _congViecRepo.GetByIdAsync(tDto.Id);
                if (task == null) continue;

                // Dùng thuật toán chuẩn 2 Bước: (1) KNN Top K -> (2) Workload Thấp nhất
                var bestUserId = KnnTimNguoiPhuHop(task, cachedUsers, userWorkloadHours, rules);

                if (bestUserId.HasValue)
                {
                    // Cập nhật bộ đếm workload sau khi giao
                    double taskEstimate = task.ThoiGianUocTinh > 0 ? task.ThoiGianUocTinh : defaultEstimate;
                    
                    // Tìm được người phù hợp → Giao cho họ
                    task.AssigneeId = bestUserId.Value;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.AI;
                    task.AiMatchScore = 1.0; // Đánh dấu là AI đã chọn
                    task.AiReasoning = $"Chọn từ Top {topK} ứng viên skill cao nhất, workload hiện tại thấp nhất ({userWorkloadHours[bestUserId.Value]}h)";
                    task.TrangThai = TrangThaiCongViec.Todo;

                    userWorkloadHours[bestUserId.Value] += taskEstimate;
                }
                else
                {
                    // Fallback: Không tìm được ai phù hợp → Giao cho PM để ông ấy tự xử lý
                    task.AssigneeId = pmId;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.Manual;
                    task.AiMatchScore = 0;
                    task.AiReasoning = "KNN không tìm được ứng viên phù hợp. Đã chuyển cho Quản lý dự án xử lý.";
                    task.TrangThai = TrangThaiCongViec.Todo;
                }

                if (task.SprintId == null) task.SprintId = currentSprint.Id;
                await _congViecRepo.UpdateAsync(task);

                // Gửi thông báo riêng cho người được giao việc
                if (task.AssigneeId.HasValue)
                {
                    string noiDungThongBao = task.AssigneeId == pmId
                        ? $"[Cần xử lý] AI không tìm được người phù hợp cho task: '{task.TieuDe}'. Vui lòng phân công thủ công."
                        : $"AI vừa giao cho bạn: '{task.TieuDe}'. Trình tự đã tối ưu theo Dependency.";
                    await _notificationService.NotifyPersonal(task.AssigneeId.Value, "AI Giao việc", noiDungThongBao);
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

                    // Lấy số giờ làm việc mỗi ngày từ quy tắc (mặc định 8)
                    int workingHoursPerDay = (int)GetRuleValue(rules, "WORKING_HOURS_PER_DAY", 8);

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
                            if (hrs <= workingHoursPerDay) hrs = 0;
                            else { hrs -= workingHoursPerDay; eP = SkipWeekends(eP.AddDays(1)); }
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

        /// <summary>
        /// Thuật toán KNN (K-Nearest Neighbors) để tìm người phù hợp nhất cho một Task.
        /// Nguyên lý mới: (1) Lọc KNN lấy Top K người phù hợp kỹ năng nhất. (2) Chọn người ít workload nhất trong Top K.
        /// </summary>
        private int? KnnTimNguoiPhuHop(
            CongViec task,
            Dictionary<int, User> cachedUsers,
            Dictionary<int, double> userWorkloadHours,
            IEnumerable<QuyTacGiaoViecAI> rules)
        {
            double minAcceptableScore = GetRuleValue(rules, "MINIMUM_ACCEPABLE_SCORE", 0.3);

            var reqs = task.YeuCauCongViecs ?? new List<YeuCauCongViec>();
            var allSkillIds = reqs.Select(r => r.KyNangId).Distinct().ToList();

            if (!allSkillIds.Any())
            {
                return TimNguoiItViecNhat(cachedUsers, userWorkloadHours, rules);
            }

            // Bước 1: Lọc KNN thuần túy theo Skill để lấy Top K candidates
            var userDistances = new List<(int UserId, double Score, bool IsPm)>();
            double baseMaxDistance = Math.Sqrt(reqs.Count * 25.0);
            
            foreach (var kvp in cachedUsers)
            {
                var user = kvp.Value;
                bool isPmOrAdmin = user.NguoiDungVaiTros != null &&
                                   user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN");

                var userSkills = user.KyNangNguoiDungs ?? new List<KyNangNguoiDung>();
                double skillDistanceTotal = 0;

                foreach (var req in reqs)
                {
                    double userExperience = 0;
                    var directSkill = userSkills.FirstOrDefault(s => s.KyNangId == req.KyNangId);
                    if (directSkill != null)
                    {
                        userExperience = directSkill.SoNamKinhNghiem;
                    }
                    else if (req.KyNang?.CongNgheId > 0)
                    {
                        var sameTechSkills = userSkills.Where(s => s.KyNang?.CongNgheId == req.KyNang.CongNgheId);
                        if (sameTechSkills.Any())
                            userExperience = sameTechSkills.Max(s => (double)s.SoNamKinhNghiem) * 0.5;
                        else if (req.KyNang.CongNghe?.NhomKyNangId > 0)
                        {
                            var sameGroupSkills = userSkills.Where(s => s.KyNang?.CongNghe?.NhomKyNangId == req.KyNang.CongNghe.NhomKyNangId);
                            if (sameGroupSkills.Any())
                                userExperience = sameGroupSkills.Max(s => (double)s.SoNamKinhNghiem) * 0.2;
                        }
                    }

                    // Điểm trừ khoảng cách
                    double diff = Math.Max(0, 5.0 - userExperience); 
                    skillDistanceTotal += diff * diff;
                }

                double euclideanDistance = Math.Sqrt(skillDistanceTotal);
                double score = baseMaxDistance > 0 ? Math.Max(0, 1 - (euclideanDistance / baseMaxDistance)) : 0;
                
                if (score >= minAcceptableScore)
                {
                    userDistances.Add((user.Id, score, isPmOrAdmin));
                }
            }

            if (!userDistances.Any()) return null;

            // Loại bỏ PM để ưu tiên Developer, trừ khi không còn ai.
            var developerCandidates = userDistances.Where(u => !u.IsPm).OrderByDescending(u => u.Score).ToList();
            if (!developerCandidates.Any()) developerCandidates = userDistances.OrderByDescending(u => u.Score).ToList();

            // Lấy Top K ứng viên có Score cao nhất
            int k = (int)GetRuleValue(rules, "KNN_TOP_K", 3);
            var candidates = developerCandidates.Take(k).ToList();

            // Capacity Limit: Lọc những người đã vượt ngưỡng
            double maxHours = GetRuleValue(rules, "MAX_HOURS_PER_USER", 40.0);
            candidates = candidates
                .Where(u => userWorkloadHours.GetValueOrDefault(u.UserId, 0) < maxHours)
                .ToList();

            if (!candidates.Any()) return null;

            // Bước 2: Trong nhóm Candidates, ưu tiên chọn người đang có ít Workload nhất (tính bằng Hours)
            var minLoad = candidates.Min(u => userWorkloadHours.GetValueOrDefault(u.UserId, 0));

            var selectedUser = candidates
                .Where(u => userWorkloadHours.GetValueOrDefault(u.UserId, 0) == minLoad)
                .OrderBy(x => Guid.NewGuid()) // Tránh bias ngầm luôn chọn người đầu tiên
                .First();

            return selectedUser.UserId;
        }

        /// <summary>
        /// Tìm người ít việc nhất trong nhóm (dùng khi task không có yêu cầu kỹ năng cụ thể).
        /// </summary>
        private int? TimNguoiItViecNhat(
            Dictionary<int, User> cachedUsers,
            Dictionary<int, double> userWorkloadHours,
            IEnumerable<QuyTacGiaoViecAI> rules)
        {
            double pmPenalty = GetRuleValue(rules, "PM_TASK_PENALTY", 10.0);
            
            int? bestUserId = null;
            double minVotedCount = double.MaxValue;

            foreach (var kvp in cachedUsers)
            {
                var user = kvp.Value;
                bool isPmOrAdmin = user.NguoiDungVaiTros != null &&
                                   user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN");

                double hours = userWorkloadHours.GetValueOrDefault(user.Id, 0);
                double votedCount = hours + (isPmOrAdmin ? pmPenalty * 8.0 : 0);

                if (votedCount < minVotedCount)
                {
                    minVotedCount = votedCount;
                    bestUserId = user.Id;
                }
            }
            return bestUserId;
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

        public async Task<Sprint> GetOrCreateSprintByModuleNameAsync(int duAnId, string moduleName, string? description = null)
        {
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            int sprintDays = (int)GetRuleValue(rules, "DEFAULT_SPRINT_DAYS", 14);

            var sprints = await _sprintRepo.GetByProjectIdAsync(duAnId);
            var existing = sprints.FirstOrDefault(s => s.TenSprint == moduleName);
            if (existing != null) 
            {
                if (string.IsNullOrEmpty(existing.MoTa) && !string.IsNullOrEmpty(description))
                {
                    existing.MoTa = description;
                    await _sprintRepo.UpdateAsync(existing);
                }
                return existing;
            }
 
            var duAn = await _duAnRepo.GetByIdAsync(duAnId);
            DateTime start = sprints.Any() ? sprints.Max(s => s.NgayKetThuc) : (duAn?.NgayBatDau ?? DateTime.UtcNow);
            
            return await _sprintRepo.AddAsync(new Sprint 
            { 
                DuAnId = duAnId, 
                TenSprint = moduleName, 
                MoTa = description,
                NgayBatDau = start, 
                NgayKetThuc = start.AddDays(sprintDays), 
                TrangThai = TrangThaiSprint.New 
            });
        }

        /// <summary>
        /// Lấy Sprint đang hoạt động (InProgress) của dự án để gán task vào.
        /// Nếu chưa có sprint InProgress, tự động tìm sprint New đầu tiên và kích hoạt nó.
        /// Nếu không có sprint nào, tạo Sprint 1 và kích hoạt luôn.
        /// → Sprint KHÔNG cần PM bấm nút, tự động bắt đầu khi AI chạy giao việc.
        /// </summary>
        private async Task<Sprint> GetOrCreateActiveSprintAsync(int duAnId, IEnumerable<QuyTacGiaoViecAI> rules)
        {
            int sprintDays = (int)GetRuleValue(rules, "DEFAULT_SPRINT_DAYS", 14);

            var sprints = await _sprintRepo.GetByProjectIdAsync(duAnId);

            // Ưu tiên sprint đang chạy trước
            var inProgress = sprints.FirstOrDefault(s => s.TrangThai == TrangThaiSprint.InProgress);
            if (inProgress != null) return inProgress;

            // Không có sprint InProgress → tìm sprint New đầu tiên (theo NgayBatDau + Id)
            var firstNew = sprints
                .Where(s => s.TrangThai == TrangThaiSprint.New)
                .OrderBy(s => s.NgayBatDau)
                .ThenBy(s => s.Id)
                .FirstOrDefault();

            if (firstNew != null)
            {
                // Tự động kích hoạt sprint đầu tiên, số ngày đọc từ cấu hình
                firstNew.TrangThai = TrangThaiSprint.InProgress;
                firstNew.NgayBatDau = DateTime.UtcNow.Date;
                firstNew.NgayKetThuc = DateTime.UtcNow.Date.AddDays(sprintDays);
                await _sprintRepo.UpdateAsync(firstNew);
                return firstNew;
            }

            // Không có sprint nào → tạo mới Sprint 1 và kích hoạt luôn
            var duAn = await _duAnRepo.GetByIdAsync(duAnId);
            DateTime start = DateTime.UtcNow.Date;
            var newSprint = new Sprint
            {
                DuAnId = duAnId,
                TenSprint = "Sprint 1",
                NgayBatDau = start,
                NgayKetThuc = start.AddDays(sprintDays),
                TrangThai = TrangThaiSprint.InProgress  // Tự động InProgress ngay khi tạo
            };
            return await _sprintRepo.AddAsync(newSprint);
        }
    }
}
