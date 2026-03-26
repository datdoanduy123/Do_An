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

        /// <summary>
        /// Gợi ý danh sách người thực hiện phù hợp nhất cho một công việc cụ thể.
        /// Dùng khi PM muốn xem lại gợi ý thủ công.
        /// Fix #4: Tính workload thực tế từ DB thay vì đọc field KhoiLuongCongViec có thể stale.
        /// </summary>
        public async Task<IEnumerable<GoiYGiaoViecDto>> GoiYAssigneeAsync(int congViecId)
        {
            var task = await _congViecRepo.GetByIdAsync(congViecId);
            if (task == null) return new List<GoiYGiaoViecDto>();

            // Đọc quy tắc AI một lần
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            double skillWeight = GetRuleValue(rules, "SKILL_MATCH_WEIGHT", 0.6);
            double experienceWeight = GetRuleValue(rules, "EXPERIENCE_WEIGHT", 0.4);
            double workloadPenalty = GetRuleValue(rules, "WORKLOAD_PENALTY", 0.1);
            double minScore = GetRuleValue(rules, "MIN_MATCH_SCORE", 0.3);

            // Lấy danh sách thành viên dự án (chỉ những người thuộc dự án này)
            var projectMembers = await _duAnRepo.GetMembersAsync(task.DuAnId);
            var candidates = projectMembers.Select(m => m.NguoiDung).ToList();

            var recommendations = new List<GoiYGiaoViecDto>();

            foreach (var userDto in candidates)
            {
                var user = await _nguoiDungRepo.LayTheoIdAsync(userDto.Id);
                if (user == null) continue;

                // KHÔNG giao việc cho người có vai trò quản lý
                if (user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN")) continue;

                // Fix #4: Tính workload thực tế từ DB (tránh đọc field KhoiLuongCongViec stale)
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

        /// <summary>
        /// Tự động giao toàn bộ công việc chưa có người thực hiện trong một dự án.
        /// Fix #2: Đọc rules một lần duy nhất rồi truyền xuống các hàm con.
        /// Fix #3: Cache danh sách project members và User objects ngoài vòng lặp task.
        /// </summary>
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

            // Fix #2: Đọc rules MỘT LẦN DUY NHẤT cho toàn bộ phiên giao việc (tránh N+1 DB query)
            var rules = await _ruleRepo.GetAllActiveRulesAsync();

            // Fix #3: Cache danh sách thành viên dự án và load tất cả user object một lần
            // Tránh gọi GetMembersAsync + LayTheoIdAsync trong mỗi task × mỗi user
            var projectMembers = await _duAnRepo.GetMembersAsync(duAnId);
            var memberIds = projectMembers.Select(m => m.NguoiDung.Id).ToList();
            var cachedUsers = new Dictionary<int, User>();
            foreach (var memberId in memberIds)
            {
                var u = await _nguoiDungRepo.LayTheoIdAsync(memberId);
                if (u != null) cachedUsers[u.Id] = u;
            }

            // Dictionary theo dõi workload tạm thời gán trong phiên AI này (theo Sprint)
            var tempWorkload = new Dictionary<int, double>();

            foreach (var tDto in tasks)
            {
                // Fix #1 đã xử lý ở CongViecRepository: LayDanhSachCongViecAsync đã load YeuCauCongViecs
                // nên không cần GetByIdAsync ở đây nữa cho mục đích tính điểm.
                // Tuy nhiên vẫn cần entity thật để Update, dùng tDto trực tiếp.
                var task = await _congViecRepo.GetByIdAsync(tDto.Id);
                if (task == null) continue;

                // Giao việc dùng cached users và rules (Fix #2, #3)
                var suggestions = await GoiYAssigneeWithTempWorkloadAsync(
                    task, tempWorkload, task.SprintId ?? currentSprint.Id,
                    pagedTasks.Items, rules, cachedUsers);
                var bestMatch = suggestions.FirstOrDefault();

                if (bestMatch != null)
                {
                    task.AssigneeId = bestMatch.UserId;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.AI;
                    task.AiMatchScore = bestMatch.DiemPhuHop;
                    task.AiReasoning = bestMatch.LyDo;

                    // Cập nhật workload tạm thời cho các task kế tiếp trong phiên này
                    tempWorkload[bestMatch.UserId] = tempWorkload.GetValueOrDefault(bestMatch.UserId) + task.ThoiGianUocTinh;
                }
                else
                {
                    // AI không tìm được ai phù hợp → gán về người tạo để PM xem xét lại
                    task.AssigneeId = task.CreatedBy;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.Manual;
                    task.AiMatchScore = 0;
                    task.AiReasoning = "AI không tìm thấy ứng viên đủ điều kiện kỹ năng. Đã gán cho người tạo công việc để xem xét lại.";
                }

                if (task.SprintId == null)
                {
                    task.SprintId = currentSprint.Id;
                }

                var res = await _congViecRepo.UpdateAsync(task);

                // Thông báo realtime cho người được giao việc
                if (res && task.AssigneeId.HasValue)
                {
                    await _notificationService.NotifyPersonal(task.AssigneeId.Value, "AI Giao việc", $"AI vừa tự động giao cho bạn công việc: {task.TieuDe}");
                }
            }

            await LapKeHoachTimelineDuAnAsync(duAnId);

            // Thông báo Realtime: AI đã giao việc / cập nhật timeline
            await _notificationService.NotifyTaskUpdated(duAnId);

            return true;
        }

        /// <summary>
        /// Lập kế hoạch timeline (ngày bắt đầu / kết thúc dự kiến) cho tất cả task,
        /// nhóm theo Sprint và theo người thực hiện.
        /// Fix #5: Cảnh báo AiReasoning khi task vượt quá thời hạn Sprint.
        /// </summary>
        private async Task LapKeHoachTimelineDuAnAsync(int duAnId)
        {
            var query = new CongViecQueryDto { DuAnId = duAnId, PageSize = 1000 };
            var pagedTasks = await _congViecRepo.LayDanhSachCongViecAsync(query);
            var allTasks = pagedTasks.Items;

            // Đọc rules 1 lần duy nhất cho toàn bộ quá trình lên lịch (tránh N+1 query)
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            // Hệ số buffer: mặc định +20% thời gian ước tính để bù đắp fix bug đột xuất
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

                    // Điểm xuất phát: không cho lịch rơi vào quá khứ
                    DateTime currentPointer = new DateTime[] { sprint.NgayBatDau, DateTime.UtcNow.Date }.Max();
                    currentPointer = SkipWeekends(currentPointer);

                    foreach (var tDto in sortedTasks)
                    {
                        var task = await _congViecRepo.GetByIdAsync(tDto.Id);
                        if (task == null) continue;

                        DateTime taskStart = currentPointer;

                        // Xét dependency: task này chỉ bắt đầu sau khi task tiền đề hoàn thành
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

                        // Áp dụng buffer vào thời gian ước tính (đọc 1 lần ở đầu hàm, tránh N+1 query)
                        // Ví dụ: ThoiGianUocTinh = 8h, bufferRate = 0.2 → hoursRemaining = 10h
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

                        // Fix #5: Thay vì âm thầm cắt ngày, thêm cảnh báo rõ ràng vào AiReasoning
                        // để PM biết Sprint đang bị overload và cần điều chỉnh
                        if (task.NgayKetThucDuKien > sprint.NgayKetThuc)
                        {
                            task.NgayKetThucDuKien = sprint.NgayKetThuc;
                            task.AiReasoning = (task.AiReasoning ?? "") +
                                $" ⚠️ Cảnh báo: Task có thể không hoàn thành trong Sprint (cần đến {endPointer:dd/MM/yyyy}, Sprint kết thúc {sprint.NgayKetThuc:dd/MM/yyyy}).";
                        }

                        currentPointer = SkipWeekends(task.NgayKetThucDuKien.Value.AddDays(1));
                        await _congViecRepo.UpdateAsync(task);
                    }
                }
            }
        }

        /// <summary>
        /// Bỏ qua ngày cuối tuần (Thứ 7, Chủ nhật) khi tính lịch làm việc.
        /// </summary>
        private DateTime SkipWeekends(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            return date;
        }

        /// <summary>
        /// Tìm hoặc tạo Sprint theo tên module (dùng khi import task từ file AI).
        /// </summary>
        public async Task<Sprint> GetOrCreateSprintByModuleNameAsync(int duAnId, string moduleName)
        {
            var sprints = await _sprintRepo.GetByProjectIdAsync(duAnId);
            var existing = sprints.FirstOrDefault(s => s.TenSprint == moduleName);
            if (existing != null) return existing;

            var duAn = await _duAnRepo.GetByIdAsync(duAnId);

            // Sprint mới nối tiếp sau Sprint cuối cùng
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

        /// <summary>
        /// Tìm Sprint đang hoạt động của dự án, hoặc tạo mới nếu chưa có.
        /// Fix #6: Ưu tiên Sprint trạng thái InProgress trước, fallback về New.
        /// Tránh chọn nhầm Sprint New chưa kích hoạt khi đã có Sprint đang chạy.
        /// </summary>
        private async Task<Sprint> GetOrCreateActiveSprintAsync(int duAnId)
        {
            var sprints = await _sprintRepo.GetByProjectIdAsync(duAnId);

            // Fix #6: Ưu tiên InProgress → New (tránh chọn nhầm Sprint New chưa kích hoạt)
            var activeSprint = sprints.FirstOrDefault(s => s.TrangThai == TrangThaiSprint.InProgress)
                            ?? sprints.FirstOrDefault(s => s.TrangThai == TrangThaiSprint.New);

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

        /// <summary>
        /// Tạo Sprint tiếp theo nối tiếp sau Sprint hiện tại.
        /// </summary>
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

        /// <summary>
        /// Gợi ý người thực hiện tốt nhất cho một task trong ngữ cảnh tự động giao việc hàng loạt.
        /// Fix #2: Nhận rules và cachedUsers từ bên ngoài (đã đọc 1 lần), không query DB lặp lại.
        /// Fix #3: Dùng cachedUsers thay vì gọi LayTheoIdAsync trong từng vòng lặp.
        /// </summary>
        private async Task<IEnumerable<GoiYGiaoViecDto>> GoiYAssigneeWithTempWorkloadAsync(
            CongViec task,
            Dictionary<int, double> tempWorkload,
            int sprintId,
            IEnumerable<CongViec> allSprintTasks,
            IEnumerable<QuyTacGiaoViecAI> rules,         // Fix #2: nhận từ caller
            Dictionary<int, User> cachedUsers)             // Fix #3: nhận từ caller
        {
            double skillWeight = GetRuleValue(rules, "SKILL_MATCH_WEIGHT", 0.6);
            double experienceWeight = GetRuleValue(rules, "EXPERIENCE_WEIGHT", 0.4);
            double workloadPenalty = GetRuleValue(rules, "WORKLOAD_PENALTY", 0.1);
            double minScore = GetRuleValue(rules, "MIN_MATCH_SCORE", 0.3);
            double maxWorkload = GetRuleValue(rules, "MAX_WORKLOAD_HOURS", 40.0);

            var recommendations = new List<GoiYGiaoViecDto>();

            // Fix #3: Duyệt qua cachedUsers thay vì gọi GetMembersAsync + LayTheoIdAsync cho từng task
            foreach (var kvp in cachedUsers)
            {
                var user = kvp.Value;

                // KHÔNG giao việc cho người có vai trò quản lý
                if (user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN")) continue;

                // Tính workload THEO SPRINT: tổng giờ task trong Sprint chưa Done
                double sprintWorkload = allSprintTasks
                    .Where(t => t.AssigneeId == user.Id
                             && t.SprintId == sprintId
                             && t.TrangThai != TrangThaiCongViec.Done)
                    .Sum(t => t.ThoiGianUocTinh);

                // Cộng thêm workload tạm thời từ phiên AI này (task vừa giao trong vòng lặp hiện tại)
                double currentWorkload = sprintWorkload + tempWorkload.GetValueOrDefault(user.Id);

                // Hard Cap: bỏ qua hoàn toàn nếu nhân viên đã đạt ngưỡng tải tối đa trong Sprint
                // Ngưỡng mặc định: 40h (1 tuần làm việc). Có thể cấu hình qua rule MAX_WORKLOAD_HOURS
                if (currentWorkload >= maxWorkload) continue;

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

        /// <summary>
        /// Tính tổng số giờ công việc chưa hoàn thành của một nhân viên (workload thực tế từ DB).
        /// </summary>
        private async Task<double> GetUserWorkloadHoursAsync(int userId)
        {
            var query = new CongViecQueryDto { AssigneeId = userId, PageSize = 1000 };
            var tasks = await _congViecRepo.LayDanhSachCongViecAsync(query);
            // Chỉ tính các task chưa hoàn thành
            return tasks.Items
                .Where(t => t.TrangThai != TrangThaiCongViec.Done)
                .Sum(t => t.ThoiGianUocTinh);
        }

        /// <summary>
        /// Tính điểm phù hợp giữa một công việc và một nhân viên.
        /// Gồm: điểm kỹ năng (skill match), điểm kinh nghiệm, và phạt quá tải (workload penalty).
        /// </summary>
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
                // Trường hợp 1: Task có yêu cầu kỹ năng rõ ràng → đối soát trực tiếp
                foreach (var req in taskRequirements)
                {
                    var userSkill = userSkills.FirstOrDefault(us => us.KyNangId == req.KyNangId);
                    if (userSkill != null)
                    {
                        // Nếu level user >= mức yêu cầu → 1.0, ngược lại tính tỉ lệ
                        double skillRatio = userSkill.Level >= req.MucDoYeuCau ? 1.0 : (double)userSkill.Level / req.MucDoYeuCau;
                        skillScore += skillRatio;

                        matchedSkills.Add(userSkill.KyNang.TenKyNang);

                        // Điểm kinh nghiệm: tính theo số năm, tối đa 5 năm = 1.0
                        expScore += Math.Min(userSkill.SoNamKinhNghiem / 5.0, 1.0);
                    }
                }

                // Trung bình cộng theo số lượng yêu cầu
                skillScore /= taskRequirements.Count;
                expScore /= taskRequirements.Count;
            }
            else
            {
                // Trường hợp 2: Task không có yêu cầu kỹ năng → fallback về keyword matching
                // So khớp từ khoá trong tiêu đề/mô tả task với tên kỹ năng của nhân viên
                var taskWords = (task.TieuDe + " " + (task.MoTa ?? "")).Split(new[] { ' ', '-', '_', '.', '/' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var sk in userSkills)
                {
                    var skillName = sk.KyNang.TenKyNang;
                    var skillWords = skillName.Split(new[] { ' ', '-', '_', '.', '/' }, StringSplitOptions.RemoveEmptyEntries);

                    // Khớp nếu tên kỹ năng là một khối (SQL, C#...) hoặc có từ khoá chung với task
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

            // Phạt quá tải: cứ mỗi 8h làm việc đã có → trừ một khoảng điểm phạt
            // Ví dụ: wPenalty=0.1, user có 40h → trừ 0.5 điểm
            double workloadScorePenalty = (currentWorkloadHours / 8.0) * wPenalty;
            totalScore -= workloadScorePenalty;

            if (currentWorkloadHours >= 40)
            {
                reason.Append($"Nhân viên đang khá bận ({currentWorkloadHours}h việc). ");
            }

            // Xây dựng lý do gợi ý
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

        /// <summary>
        /// Lấy giá trị quy tắc AI theo mã, trả về giá trị mặc định nếu không tìm thấy.
        /// </summary>
        private double GetRuleValue(IEnumerable<QuyTacGiaoViecAI> rules, string code, double defaultValue)
        {
            var rule = rules.FirstOrDefault(r => r.MaQuyTac == code);
            if (rule != null && double.TryParse(rule.GiaTri, out double val)) return val;
            return defaultValue;
        }
    }
}
