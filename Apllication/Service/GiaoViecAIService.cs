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

            // 1. Lấy danh sách quy tắc AI
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            double skillWeight = GetRuleValue(rules, "SKILL_MATCH_WEIGHT", 0.6);
            double experienceWeight = GetRuleValue(rules, "EXPERIENCE_WEIGHT", 0.4);
            double workloadPenalty = GetRuleValue(rules, "WORKLOAD_PENALTY", 0.1);
            double minScore = GetRuleValue(rules, "MIN_MATCH_SCORE", 0.3);

            // 2. Lấy danh sách ứng viên (Chỉ những người tham gia dự án này)
            var projectMembers = await _duAnRepo.GetMembersAsync(task.DuAnId);
            var candidates = projectMembers.Select(m => m.NguoiDung).ToList();

            var recommendations = new List<GoiYGiaoViecDto>();

            foreach (var userDto in candidates)
            {
                var user = await _nguoiDungRepo.LayTheoIdAsync(userDto.Id);
                if (user == null) continue;

                // KHÔNG giao việc cho người có vai trò quản lý
                if (user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN")) continue;

                var matchResult = CalculateMatchScore(task, user, skillWeight, experienceWeight, workloadPenalty, user.KhoiLuongCongViec);
                
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
            var pagedTasks = await _congViecRepo.LayDanhSachCongViecAsync(query);
            var tasks = pagedTasks.Items
                .Where(t => t.AssigneeId == null || t.SprintId == null)
                .OrderBy(t => t.ViTri)
                .ThenByDescending(t => (int)t.DoUuTien)
                .ToList();

            if (!tasks.Any()) return true;

            var currentSprint = await GetOrCreateActiveSprintAsync(duAnId);

            // Dictionary để theo dõi workload gán tạm thời trong phiên chạy này
            var tempWorkload = new Dictionary<int, double>();

            foreach (var tDto in tasks)
            {
                var task = await _congViecRepo.GetByIdAsync(tDto.Id);
                if (task == null) continue;

                // Giao việc cho User (Match Score), tính workload theo Sprint hiện tại
                var suggestions = await GoiYAssigneeWithTempWorkloadAsync(task, tempWorkload, task.SprintId ?? currentSprint.Id, pagedTasks.Items);
                var bestMatch = suggestions.FirstOrDefault();

                if (bestMatch != null)
                {
                    task.AssigneeId = bestMatch.UserId;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.AI;
                    task.AiMatchScore = bestMatch.DiemPhuHop;
                    task.AiReasoning = bestMatch.LyDo;

                    // Cập nhật workload tạm thời để task sau tính toán chính xác (theo Sprint)
                    tempWorkload[bestMatch.UserId] = tempWorkload.GetValueOrDefault(bestMatch.UserId) + task.ThoiGianUocTinh;
                }
                else
                {
                    // Nếu không tìm thấy ai phù hợp (DiemPhuHop < minScore), 
                    // ta gán ngược lại cho người tạo (CreatedBy) để họ tự xử lý/phân bổ lại.
                    task.AssigneeId = task.CreatedBy;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.Manual; // Đánh dấu manual vì AI không tìm được người khớp
                    task.AiMatchScore = 0;
                    task.AiReasoning = "AI không tìm thấy ứng viên đủ điều kiện kỹ năng. Đã gán cho người tạo công việc để xem xét lại.";
                }

                if (task.SprintId == null)
                {
                    task.SprintId = currentSprint.Id;
                }
                var res = await _congViecRepo.UpdateAsync(task);
                
                // Thông báo cho từng người được AI gán việc
                if (res && task.AssigneeId.HasValue)
                {
                    await _notificationService.NotifyPersonal(task.AssigneeId.Value, "AI Giao việc", $"AI vừa tự động giao cho bạn công việc: {task.TieuDe}");
                }
            }

            await LapKeHoachTimelineDuAnAsync(duAnId);

            // Thông báo Realtime: AI đã giao việc/cập nhật timeline
            await _notificationService.NotifyTaskUpdated(duAnId);
            
            return true;
        }

        private async Task LapKeHoachTimelineDuAnAsync(int duAnId)
        {
            var query = new CongViecQueryDto { DuAnId = duAnId, PageSize = 1000 };
            var pagedTasks = await _congViecRepo.LayDanhSachCongViecAsync(query);
            var allTasks = pagedTasks.Items;

            // Đọ́c rules 1 lần duy nhất cho toàn bộ quá trình lên lịch (tránh N+1 query)
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            // Hệ số buffer: mặc định +20% thời gian ước tính để bù đắp họn, fíx bug đột xuất...
            double bufferRate = GetRuleValue(rules, "BUFFER_RATE", 0.2);

            var tasksBySprint = allTasks.Where(t => t.SprintId != null).GroupBy(t => t.SprintId);

            foreach (var sprintGroup in tasksBySprint)
            {
                var sprint = await _sprintRepo.GetByIdAsync(sprintGroup.Key!.Value);
                if (sprint == null) continue;

                var tasksByAssignee = sprintGroup.Where(t => t.AssigneeId != null).GroupBy(t => t.AssigneeId);

                foreach (var userGroup in tasksByAssignee)
                {
                    var sortedTasks = userGroup
                        .OrderBy(t => t.ViTri)
                        .ThenByDescending(t => (int)t.DoUuTien)
                        .ToList();

                    // Lấy điêń xuất phát thực té nhất có thẻ (không đẻ tìm lịch ngày đã qua trong quá khứ)
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
                                if (predecessor != null && predecessor.NgayKetThucDuKien.HasValue)
                                {
                                    DateTime minStart = SkipWeekends(predecessor.NgayKetThucDuKien.Value.AddDays(1));
                                    if (minStart > taskStart) taskStart = minStart;
                                }
                            }
                        }

                        task.NgayBatDauDuKien = SkipWeekends(taskStart);
                        
                        // Áp dụng buffer vào thời gian ước tính trước khi tính ngày kết thúc
                        // bufferRate được đọc 1 lần ở đầu hàm để tối ưu hiệu năng (tránh N+1 DB query)
                        // Ví dụ: ThoiGianUocTinh = 8h, bufferRate = 0.2 → hoursRemaining = 9.6h (làm tròn lên 10h)
                        int hoursRemaining = (int)Math.Ceiling(task.ThoiGianUocTinh * (1 + bufferRate));
                        DateTime endPointer = task.NgayBatDauDuKien.Value;

                        while (hoursRemaining > 0)
                        {
                            if (hoursRemaining <= 8) hoursRemaining = 0;
                            else
                            {
                                hoursRemaining -= 8;
                                endPointer = endPointer.AddDays(1);
                                endPointer = SkipWeekends(endPointer);
                            }
                        }

                        task.NgayKetThucDuKien = endPointer;

                        if (task.NgayKetThucDuKien > sprint.NgayKetThuc)
                            task.NgayKetThucDuKien = sprint.NgayKetThuc;

                        currentPointer = SkipWeekends(task.NgayKetThucDuKien.Value.AddDays(1));
                        await _congViecRepo.UpdateAsync(task);
                    }
                }
            }
        }

        private DateTime SkipWeekends(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            return date;
        }
        public async Task<Sprint> GetOrCreateSprintByModuleNameAsync(int duAnId, string moduleName)
        {
            var sprints = await _sprintRepo.GetByProjectIdAsync(duAnId);
            var existing = sprints.FirstOrDefault(s => s.TenSprint == moduleName);
            if (existing != null) return existing;

            var duAn = await _duAnRepo.GetByIdAsync(duAnId);
            
            // Tìm ngày bắt đầu cho Sprint mới (Nối tiếp sau Sprint cuối cùng)
            DateTime startDate = duAn?.NgayBatDau ?? DateTime.UtcNow;
            if (sprints.Any())
            {
                startDate = sprints.Max(s => s.NgayKetThuc);
            }

            return await _sprintRepo.AddAsync(new Sprint
            {
                DuAnId = duAnId,
                TenSprint = moduleName,
                NgayBatDau = startDate,
                NgayKetThuc = startDate.AddDays(14),
                TrangThai = TrangThaiSprint.New
            });
        }

        private async Task<Sprint> GetOrCreateActiveSprintAsync(int duAnId)
        {
            var Sprints = await _sprintRepo.GetByProjectIdAsync(duAnId);
            var activeSprint = Sprints.FirstOrDefault(s => s.TrangThai == TrangThaiSprint.New || s.TrangThai == TrangThaiSprint.InProgress);
            
            if (activeSprint != null) return activeSprint;

            var duAn = await _duAnRepo.GetByIdAsync(duAnId);
            DateTime startDate = duAn?.NgayBatDau ?? DateTime.UtcNow;

            return await _sprintRepo.AddAsync(new Sprint
            {
                DuAnId = duAnId,
                TenSprint = "Sprint 1",
                NgayBatDau = startDate,
                NgayKetThuc = startDate.AddDays(14),
                TrangThai = TrangThaiSprint.New
            });
        }

        private async Task<Sprint> CreateNextSprintAsync(int duAnId, string lastSprintName, DateTime lastSprintEndDate)
        {
            int nextNumber = 1;
            if (lastSprintName.Contains("Sprint"))
            {
                int.TryParse(lastSprintName.Replace("Sprint", "").Trim(), out nextNumber);
                nextNumber++;
            }

            return await _sprintRepo.AddAsync(new Sprint
            {
                DuAnId = duAnId,
                TenSprint = $"Sprint {nextNumber}",
                NgayBatDau = lastSprintEndDate,
                NgayKetThuc = lastSprintEndDate.AddDays(14),
                TrangThai = TrangThaiSprint.New
            });
        }

        private async Task<IEnumerable<GoiYGiaoViecDto>> GoiYAssigneeWithTempWorkloadAsync(
            CongViec task, 
            Dictionary<int, double> tempWorkload,
            int sprintId,
            IEnumerable<CongViec> allSprintTasks)
        {
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            double skillWeight = GetRuleValue(rules, "SKILL_MATCH_WEIGHT", 0.6);
            double experienceWeight = GetRuleValue(rules, "EXPERIENCE_WEIGHT", 0.4);
            double workloadPenalty = GetRuleValue(rules, "WORKLOAD_PENALTY", 0.1);
            double minScore = GetRuleValue(rules, "MIN_MATCH_SCORE", 0.3);
            double maxWorkload = GetRuleValue(rules, "MAX_WORKLOAD_HOURS", 40.0);

            // 2. Lấy danh sách ứng viên (Chỉ những người tham gia dự án này)
            var projectMembers = await _duAnRepo.GetMembersAsync(task.DuAnId);
            var candidates = projectMembers.Select(m => m.NguoiDung).ToList();

            var recommendations = new List<GoiYGiaoViecDto>();

            foreach (var userDto in candidates)
            {
                var user = await _nguoiDungRepo.LayTheoIdAsync(userDto.Id);
                if (user == null) continue;

                // KHÔNG giao việc cho người có vai trò quản lý
                if (user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN")) continue;

                // Tính workload THEO SPRINT: tổng giờ task cùng Sprint chưa Done
                double sprintWorkload = allSprintTasks
                    .Where(t => t.AssigneeId == user.Id
                             && t.SprintId == sprintId
                             && t.TrangThai != TrangThaiCongViec.Done)
                    .Sum(t => t.ThoiGianUocTinh);

                // Cộng thêm workload tạm thời từ phiên AI này (task vừa giao trong vòng lặp)
                double currentWorkload = sprintWorkload + tempWorkload.GetValueOrDefault(user.Id);

                // Hard Cap: chặn cứng nếu nhân viên đã đạt ngưỡng tải tối đa TRONG SPRINT
                // Ngưỡng mặc định: 40h (1 tuần làm việc). Có thể cấu hình qua rule MAX_WORKLOAD_HOURS
                if (currentWorkload >= maxWorkload) continue; // Bỏ qua hoàn toàn, không tính điểm

                var matchResult = CalculateMatchScore(task, user, skillWeight, experienceWeight, workloadPenalty, currentWorkload);
                
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

        private async Task<double> GetUserWorkloadHoursAsync(int userId)
        {
            var query = new CongViecQueryDto { AssigneeId = userId, PageSize = 1000 };
            var tasks = await _congViecRepo.LayDanhSachCongViecAsync(query);
            // Chỉ tính các task chưa hoàn thành
            return tasks.Items
                .Where(t => t.TrangThai != TrangThaiCongViec.Done)
                .Sum(t => t.ThoiGianUocTinh);
        }

        private (double Score, string Reason, List<string> MatchedSkills) CalculateMatchScore(
            CongViec task, User user, double sWeight, double eWeight, double wPenalty, double currentWorkloadHours)
        {
            double skillScore = 0;
            double expScore = 0;
            var matchedSkills = new List<string>();
            var reason = new StringBuilder();

            var userSkills = user.KyNangNguoiDungs ?? new List<KyNangNguoiDung>();
            var taskRequirements = task.YeuCauCongViecs ?? new List<YeuCauCongViec>();

            if (taskRequirements.Any())
            {
                foreach (var req in taskRequirements)
                {
                    var userSkill = userSkills.FirstOrDefault(us => us.KyNangId == req.KyNangId);
                    if (userSkill != null)
                    {
                        // 1. Diem Ky nang: Level user >= Muc do yeu cau -> 1.0, nguoc lai -> LevelUser / MucDoYeuCau
                        double skillRatio = userSkill.Level >= req.MucDoYeuCau ? 1.0 : (double)userSkill.Level / req.MucDoYeuCau;
                        skillScore += skillRatio;
                        
                        matchedSkills.Add(userSkill.KyNang.TenKyNang);

                        // 2. Diem Kinh nghiem: Tinh tren so nam (Quy uoc 5 nam la diem tuyet doi)
                        expScore += Math.Min(userSkill.SoNamKinhNghiem / 5.0, 1.0);
                    }
                }

                // Trung binh cong theo so luong yeu cau
                skillScore /= taskRequirements.Count;
                expScore /= taskRequirements.Count;
            }
            else
            {
                // Neu task khong co yeu cau thi fallback ve keyword matching
                var taskWords = (task.TieuDe + " " + (task.MoTa ?? "")).Split(new[] { ' ', '-', '_', '.', '/' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var sk in userSkills)
                {
                    var skillName = sk.KyNang.TenKyNang;
                    var skillWords = skillName.Split(new[] { ' ', '-', '_', '.', '/' }, StringSplitOptions.RemoveEmptyEntries);

                    // Neu ten ky nang la mot khoi (SQL, API, C#...) hoac co tu khoa chung voi task
                    bool matches = taskWords.Any(tw => tw.Equals(skillName, StringComparison.OrdinalIgnoreCase)) ||
                                   skillWords.Any(sw => taskWords.Any(tw => tw.Equals(sw, StringComparison.OrdinalIgnoreCase) && sw.Length > 2));

                    if (matches)
                    {
                        skillScore += (sk.Level / 5.0);
                        matchedSkills.Add(sk.KyNang.TenKyNang);
                        expScore += Math.Min(sk.SoNamKinhNghiem / 5.0, 1.0);
                    }
                }
                if (matchedSkills.Any())
                {
                    skillScore /= matchedSkills.Count;
                    expScore /= matchedSkills.Count;
                }
            }

            // Tính điểm tổng hợp (Weighted Average)
            double totalScore = (skillScore * sWeight) + (expScore * eWeight);

            // Phạt quá tải (Workload Penalty)
            // Cứ mỗi 8h làm việc đã có, sẽ trừ một khoảng điểm phạt (wPenalty)
            // Ví dụ: wPenalty = 0.1, User có 40h việc -> trừ 0.5 điểm
            double workloadScorePenalty = (currentWorkloadHours / 8.0) * wPenalty;
            totalScore -= workloadScorePenalty;

            if (currentWorkloadHours >= 40) 
            {
                reason.Append($"Nhân viên đang khá bận ({currentWorkloadHours}h việc). ");
            }

            // Xây dựng lý do
            if (matchedSkills.Any())
            {
                reason.Append($"Phù hợp các kỹ năng: {string.Join(", ", matchedSkills)}. ");
                if (expScore > 0.5) reason.Append("Có kinh nghiệm dày dặn trong lĩnh vực này. ");
            }
            else
            {
                reason.Append("Không tìm thấy kỹ năng tương ứng trực tiếp nhưng có thể đào tạo thêm.");
            }

            return (Math.Max(0, totalScore), reason.ToString().Trim(), matchedSkills);
        }

        private double GetRuleValue(IEnumerable<QuyTacGiaoViecAI> rules, string code, double defaultValue)
        {
            var rule = rules.FirstOrDefault(r => r.MaQuyTac == code);
            if (rule != null && double.TryParse(rule.GiaTri, out double val)) return val;
            return defaultValue;
        }

    }
}
