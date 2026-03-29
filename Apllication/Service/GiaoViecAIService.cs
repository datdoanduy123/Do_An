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

            var currentSprint = await GetOrCreateActiveSprintAsync(duAnId);

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

            // Đếm số task đã gán tạm thời cho mỗi user (để tính penalty chia đều)
            var taskCountPerUser = new Dictionary<int, int>();

            foreach (var tDto in sortedTasks)
            {
                var task = await _congViecRepo.GetByIdAsync(tDto.Id);
                if (task == null) continue;

                // Dùng thuật toán KNN để tìm người phù hợp nhất
                var bestUserId = KnnTimNguoiPhuHop(task, cachedUsers, taskCountPerUser, allTasksInProject);

                if (bestUserId.HasValue)
                {
                    // Tìm được người phù hợp → Giao cho họ
                    task.AssigneeId = bestUserId.Value;
                    task.PhuongThucGiaoViec = PhuongThucGiaoViec.AI;
                    task.AiMatchScore = 1.0; // KNN không trả về score 0-1 trực tiếp, đánh dấu là AI đã chọn
                    task.AiReasoning = $"AI (KNN) đã chọn người phù hợp nhất dựa trên kỹ năng yêu cầu.";
                    task.TrangThai = TrangThaiCongViec.Todo;

                    // Cập nhật bộ đếm task chia đều
                    taskCountPerUser[bestUserId.Value] = taskCountPerUser.GetValueOrDefault(bestUserId.Value) + 1;
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

        /// <summary>
        /// Thuật toán KNN (K-Nearest Neighbors) để tìm người phù hợp nhất cho một Task.
        /// Nguyên lý: Tính khoảng cách Euclidean giữa vector kỹ năng của User và yêu cầu kỹ năng của Task.
        /// Khoảng cách càng nhỏ = User càng phù hợp. Thêm penalty chia đều để không ai làm quá nhiều.
        /// </summary>
        /// <param name="task">Task cần giao việc</param>
        /// <param name="cachedUsers">Danh sách Users đã nạp sẵn</param>
        /// <param name="taskCountPerUser">Bộ đếm số task đã gán tạm thời cho mỗi user (để chia đều)</param>
        /// <param name="allTasksInProject">Toàn bộ task trong dự án (để tính workload thực tế)</param>
        /// <returns>UserId của người phù hợp nhất, hoặc null nếu không tìm thấy</returns>
        private int? KnnTimNguoiPhuHop(
            CongViec task,
            Dictionary<int, User> cachedUsers,
            Dictionary<int, int> taskCountPerUser,
            List<CongViec> allTasksInProject)
        {
            // Lấy danh sách kỹ năng yêu cầu của Task
            var reqs = task.YeuCauCongViecs ?? new List<YeuCauCongViec>();

            // Tất cả Id kỹ năng cần xét (lấy từ yêu cầu task)
            var allSkillIds = reqs.Select(r => r.KyNangId).Distinct().ToList();

            // Nếu task không có yêu cầu kỹ năng cụ thể → Fallback về người ít việc nhất
            if (!allSkillIds.Any())
            {
                return TimNguoiItViecNhat(cachedUsers, taskCountPerUser, allTasksInProject);
            }

            // Tính trung bình số task đã được gán để tính penalty chia đều
            int totalTasksAssigned = taskCountPerUser.Values.Sum();
            double avgTaskPerUser = cachedUsers.Count > 0 ? (double)totalTasksAssigned / cachedUsers.Count : 0;

            double bestDistance = double.MaxValue;
            int? bestUserId = null;

            foreach (var kvp in cachedUsers)
            {
                var user = kvp.Value;

                // Bỏ qua QUAN_LY và ADMIN (không tham gia làm việc trực tiếp)
                if (user.NguoiDungVaiTros != null &&
                    user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN"))
                    continue;

                var userSkills = user.KyNangNguoiDungs ?? new List<KyNangNguoiDung>();

                // ---- Tính khoảng cách Euclidean theo từng chiều kỹ năng ----
                // Vector Task:  T[i] = mức độ yêu cầu kỹ năng i (1-5)
                // Vector User:  U[i] = level kỹ năng i của User (0 nếu user không có kỹ năng đó)
                // distance = sqrt( Σ (T[i] - U[i])^2 )
                double sumSquaredDiff = 0;
                foreach (var req in reqs)
                {
                    double taskRequirement = req.MucDoYeuCau; // Mức yêu cầu (1-5)
                    var userSkill = userSkills.FirstOrDefault(s => s.KyNangId == req.KyNangId);
                    double userLevel = userSkill?.Level ?? 0; // Level của user, 0 nếu không có kỹ năng

                    // Hiệu (T[i] - U[i]): Nếu User vượt yêu cầu (userLevel > taskRequirement)
                    // thì hiệu = 0 (tốt hoàn toàn), không bị phạt vì quá giỏi.
                    double diff = Math.Max(0, taskRequirement - userLevel);
                    sumSquaredDiff += diff * diff;
                }
                double euclideanDistance = Math.Sqrt(sumSquaredDiff);

                // ---- Penalty chia đều: Nếu user đã nhiều task hơn TB → cộng thêm khoảng cách ----
                // Hệ số penalty = 0.5 nghĩa là: mỗi task dư thêm làm khoảng cách tăng thêm 0.5
                int userTaskCount = taskCountPerUser.GetValueOrDefault(user.Id, 0);
                double balancePenalty = Math.Max(0, userTaskCount - avgTaskPerUser) * 0.5;

                double totalDistance = euclideanDistance + balancePenalty;

                // Chọn người có khoảng cách tổng nhỏ nhất
                if (totalDistance < bestDistance)
                {
                    bestDistance = totalDistance;
                    bestUserId = user.Id;
                }
            }

            // Nếu khoảng cách tốt nhất vẫn quá lớn (không ai có kỹ năng phù hợp)
            // ngưỡng: sqrt(reqs.Count * 25) = khi tất cả kỹ năng User đều = 0 và Task yêu cầu = 5
            double maxAllowedDistance = Math.Sqrt(reqs.Count * 25.0);
            if (bestUserId.HasValue && bestDistance >= maxAllowedDistance)
            {
                // Không ai đủ kỹ năng → Fallback PM
                return null;
            }

            return bestUserId;
        }

        /// <summary>
        /// Tìm người ít việc nhất trong nhóm (dùng khi task không có yêu cầu kỹ năng cụ thể).
        /// </summary>
        private int? TimNguoiItViecNhat(
            Dictionary<int, User> cachedUsers,
            Dictionary<int, int> taskCountPerUser,
            List<CongViec> allTasksInProject)
        {
            int? bestUserId = null;
            int minTaskCount = int.MaxValue;

            foreach (var kvp in cachedUsers)
            {
                var user = kvp.Value;
                if (user.NguoiDungVaiTros != null &&
                    user.NguoiDungVaiTros.Any(uv => uv.VaiTro.MaVaiTro == "QUAN_LY" || uv.VaiTro.MaVaiTro == "ADMIN"))
                    continue;

                int count = taskCountPerUser.GetValueOrDefault(user.Id, 0);
                if (count < minTaskCount)
                {
                    minTaskCount = count;
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
