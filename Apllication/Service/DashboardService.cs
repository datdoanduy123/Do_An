using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Domain.Enums;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Apllication.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly ICongViecRepository _taskRepo;
        private readonly IDuAnRepository _projectRepo;
        private readonly ISprintRepository _sprintRepo;
        private readonly INhatKyCongViecRepository _logRepo;
        private readonly IQuyTacGiaoViecAIRepository _ruleRepo;

        public DashboardService(
            ICongViecRepository taskRepo, 
            IDuAnRepository projectRepo,
            ISprintRepository sprintRepo,
            INhatKyCongViecRepository logRepo,
            IQuyTacGiaoViecAIRepository ruleRepo)
        {
            _taskRepo = taskRepo;
            _projectRepo = projectRepo;
            _sprintRepo = sprintRepo;
            _logRepo = logRepo;
            _ruleRepo = ruleRepo;
        }

        public async Task<object> GetDashboardStatsAsync(int userId, int? projectId = null)
        {
            // Lấy tất cả dự án và task
            var allProjects = await _projectRepo.GetAllAsync();
            
            // Nếu có projectId, ưu tiên lấy dự án đó, nếu không lấy dự án đầu tiên
            var targetProject = projectId.HasValue 
                ? allProjects.FirstOrDefault(p => p.Id == projectId.Value) 
                : allProjects.FirstOrDefault();

            var allTasks = await _taskRepo.GetAllAsync();
            var myTasks = allTasks.Where(t => t.AssigneeId == userId).ToList();
            
            var pendingReviews = allTasks.Count(t => t.TrangThai == TrangThaiCongViec.Review);

            // 1. Phân bổ trạng thái công việc
            var statusDistribution = allTasks
                .GroupBy(t => t.TrangThai)
                .Select(g => new { 
                    Status = g.Key.ToString(), 
                    Count = g.Count(),
                    Color = GetColorForStatus(g.Key)
                });

            // 2. Khối lượng công việc nhân viên
            var teamWorkload = allTasks
                .Where(t => t.AssigneeId.HasValue && t.Assignee != null && t.TrangThai != TrangThaiCongViec.Done)
                .GroupBy(t => t.Assignee!.FullName)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Name = g.Key, TaskCount = g.Count() });

            // 3. Tiến độ dự án
            var projectProgress = allProjects.Select(p => {
                var pTasks = allTasks.Where(t => t.DuAnId == p.Id).ToList();
                var total = pTasks.Count;
                var completed = pTasks.Count(t => t.TrangThai == TrangThaiCongViec.Done);
                return new { ProjectName = p.TenDuAn, Progress = total > 0 ? (int)((double)completed / total * 100) : 0 };
            }).OrderByDescending(p => p.Progress).Take(5);

            // 4. Burndown Chart Data (Dựa trên dự án mục tiêu)
            var activeSprint = (await _sprintRepo.GetByProjectIdAsync(targetProject?.Id ?? 0))
                .FirstOrDefault(s => s.TrangThai == TrangThaiSprint.InProgress);
            
            var burndownData = await CalculateBurndownAsync(activeSprint, allTasks.ToList());

            // 5. Velocity Chart Data (Dựa trên dự án mục tiêu)
            var velocityData = await CalculateVelocityAsync(targetProject?.Id ?? 0);

            // 6. Tính toán tải công việc chi tiết trong Sprint hiện tại (Xử lý yêu cầu xác định quá tải & dư thừa)
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            double focusHours = GetRuleValue(rules, "FOCUS_HOURS_PER_DAY", 6);
            double ceremonyRate = GetRuleValue(rules, "SPRINT_CEREMONY_RATE", 0.15);
            double underLoadThreshold = GetRuleValue(rules, "UNDER_LOAD_THRESHOLD", 60); // 60%
            
            object sprintWorkload = null!;
            if (activeSprint != null)
            {
                var sprintTasks = allTasks.Where(t => t.SprintId == activeSprint.Id && t.TrangThai != TrangThaiCongViec.Cancelled).ToList();
                var sprintDays = (activeSprint.NgayKetThuc - activeSprint.NgayBatDau).Days + 1;
                // Tính số ngày làm việc thực tế (trừ T7, CN)
                int workDays = 0;
                for (int i = 0; i < sprintDays; i++)
                {
                    var day = activeSprint.NgayBatDau.AddDays(i).DayOfWeek;
                    if (day != DayOfWeek.Saturday && day != DayOfWeek.Sunday) workDays++;
                }
                if (workDays == 0) workDays = 1;

                // CÔNG THỨC NÂNG CAO: Capacity = (WorkDays * FocusHours) * (1 - CeremonyRate)
                double capacityPerPerson = Math.Round((workDays * focusHours) * (1 - ceremonyRate), 1);

                sprintWorkload = sprintTasks
                    .Where(t => t.AssigneeId.HasValue)
                    .GroupBy(t => new { t.AssigneeId, t.Assignee?.FullName })
                    .Select(g => {
                        double activeHours = g.Where(t => t.TrangThai != TrangThaiCongViec.Done).Sum(t => t.ThoiGianUocTinh);
                        double loadFactor = capacityPerPerson > 0 ? Math.Round((activeHours / capacityPerPerson) * 100, 1) : 0;
                        
                        string status = "Normal";
                        if (loadFactor > 100) status = "Overloaded";
                        else if (loadFactor > 80) status = "Warning";
                        else if (loadFactor < underLoadThreshold) status = "Under-load";

                        return new {
                            UserId = g.Key.AssigneeId,
                            FullName = g.Key.FullName ?? "Chưa gán",
                            TotalTasks = g.Count(t => t.TrangThai != TrangThaiCongViec.Done),
                            CompletedTasks = g.Count(t => t.TrangThai == TrangThaiCongViec.Done),
                            TotalHours = activeHours, // Trả về số giờ hiện tại đang gánh
                            Capacity = capacityPerPerson,
                            LoadFactor = loadFactor,
                            Status = status
                        };
                    })
                    .OrderByDescending(x => x.LoadFactor)
                    .ToList();
            }

            return new
            {
                TotalProjects = allProjects.Count(),
                CompletedTasks = myTasks.Count(t => t.TrangThai == TrangThaiCongViec.Done),
                InProgressTasks = myTasks.Count(t => t.TrangThai == TrangThaiCongViec.InProgress),
                PendingReviews = pendingReviews,
                TaskStatusDistribution = statusDistribution,
                TeamWorkload = teamWorkload,
                SprintWorkload = sprintWorkload, // Trả về dữ liệu tải công việc Sprint
                ProjectProgress = projectProgress,
                BurndownData = burndownData,
                VelocityData = velocityData,
                RecentProjects = allProjects.OrderByDescending(p => p.Id).Take(3).Select(p => {
                    var pTasks = allTasks.Where(t => t.DuAnId == p.Id).ToList();
                    var total = pTasks.Count;
                    var completed = pTasks.Count(t => t.TrangThai == TrangThaiCongViec.Done);
                    return new { p.Id, p.TenDuAn, p.NgayBatDau, p.NgayKetThuc, p.TrangThai, Progress = total > 0 ? (int)((double)completed / total * 100) : 0 };
                }),
                SelectedProjectName = targetProject?.TenDuAn,
                MyPriorityTasks = myTasks.Where(t => t.TrangThai != TrangThaiCongViec.Done && t.DoUuTien >= DoUuTien.High).Take(5).Select(t => new { t.Id, t.TieuDe, t.DoUuTien, t.ThoiGianUocTinh })
            };
        }

        private async Task<List<object>> CalculateBurndownAsync(Sprint? sprint, List<CongViec> allTasks)
        {
            if (sprint == null) return new List<object>();

            var sprintTasks = allTasks.Where(t => t.SprintId == sprint.Id).ToList();
            double totalEstimate = sprintTasks.Sum(t => t.ThoiGianUocTinh);
            var logs = new List<Domain.Entities.NhatKyCongViec>();
            foreach(var t in sprintTasks) {
                logs.AddRange(await _logRepo.GetByTaskIdAsync(t.Id));
            }

            var days = (sprint.NgayKetThuc - sprint.NgayBatDau).Days + 1;
            if (days <= 0) days = 14; // Default to 2 weeks if date error

            var data = new List<object>();
            for (int i = 0; i < days; i++)
            {
                var date = sprint.NgayBatDau.AddDays(i);
                double ideal = Math.Max(0, totalEstimate - (totalEstimate / (days - 1)) * i);
                
                // Actual remaining = Total Estimate - Hours worked until this date
                double workedUntilNow = logs.Where(l => l.NgayCapNhat.Date <= date.Date).Sum(l => l.SoGioLamViec);
                double actual = Math.Max(0, totalEstimate - workedUntilNow);

                data.Add(new { 
                    Day = date.ToString("dd/MM"), 
                    Ideal = Math.Round(ideal, 1), 
                    Actual = date.Date <= DateTime.UtcNow.Date ? Math.Round(actual, 1) : (double?)null 
                });
            }
            return data;
        }

        private async Task<List<object>> CalculateVelocityAsync(int projectId)
        {
            var finishedSprints = (await _sprintRepo.GetByProjectIdAsync(projectId))
                .Where(s => s.TrangThai == TrangThaiSprint.Finished)
                .OrderByDescending(s => s.NgayKetThuc)
                .Take(5)
                .OrderBy(s => s.NgayKetThuc)
                .ToList();

            var allTasks = await _taskRepo.GetAllAsync();
            
            return finishedSprints.Select(s => new {
                SprintName = s.TenSprint,
                CompletedPoints = allTasks.Where(t => t.SprintId == s.Id && t.TrangThai == TrangThaiCongViec.Done).Sum(t => t.ThoiGianUocTinh)
            }).Cast<object>().ToList();
        }

        private double GetRuleValue(IEnumerable<QuyTacGiaoViecAI> rules, string code, double defaultValue)
        {
            var rule = rules.FirstOrDefault(r => r.MaQuyTac == code);
            if (rule != null && double.TryParse(rule.GiaTri, out double val)) return val;
            return defaultValue;
        }

        private string GetColorForStatus(TrangThaiCongViec status)
        {
            return status switch
            {
                TrangThaiCongViec.Todo => "#94a3b8",
                TrangThaiCongViec.InProgress => "#6366f1",
                TrangThaiCongViec.Review => "#f59e0b",
                TrangThaiCongViec.Done => "#10b981",
                TrangThaiCongViec.Cancelled => "#ef4444",
                _ => "#cbd5e1"
            };
        }
    }
}
