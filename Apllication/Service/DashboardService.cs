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

        public DashboardService(
            ICongViecRepository taskRepo, 
            IDuAnRepository projectRepo,
            ISprintRepository sprintRepo,
            INhatKyCongViecRepository logRepo)
        {
            _taskRepo = taskRepo;
            _projectRepo = projectRepo;
            _sprintRepo = sprintRepo;
            _logRepo = logRepo;
        }

        public async Task<object> GetDashboardStatsAsync(int userId)
        {
            // Lấy tất cả dự án và task
            var allProjects = await _projectRepo.GetAllAsync();
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

            // 4. Burndown Chart Data (Dựa trên dự án đầu tiên có Sprint đang chạy)
            var activeSprint = (await _sprintRepo.GetByProjectIdAsync(allProjects.FirstOrDefault()?.Id ?? 0))
                .FirstOrDefault(s => s.TrangThai == TrangThaiSprint.InProgress);
            
            var burndownData = await CalculateBurndownAsync(activeSprint, allTasks.ToList());

            // 5. Velocity Chart Data (Dựa trên 5 Sprint gần nhất đã kết thúc)
            var velocityData = await CalculateVelocityAsync(allProjects.FirstOrDefault()?.Id ?? 0);

            return new
            {
                TotalProjects = allProjects.Count(),
                CompletedTasks = myTasks.Count(t => t.TrangThai == TrangThaiCongViec.Done),
                InProgressTasks = myTasks.Count(t => t.TrangThai == TrangThaiCongViec.InProgress),
                PendingReviews = pendingReviews,
                TaskStatusDistribution = statusDistribution,
                TeamWorkload = teamWorkload,
                ProjectProgress = projectProgress,
                BurndownData = burndownData,
                VelocityData = velocityData,
                RecentProjects = allProjects.OrderByDescending(p => p.Id).Take(3).Select(p => {
                    var pTasks = allTasks.Where(t => t.DuAnId == p.Id).ToList();
                    var total = pTasks.Count;
                    var completed = pTasks.Count(t => t.TrangThai == TrangThaiCongViec.Done);
                    return new { p.Id, p.TenDuAn, p.NgayBatDau, p.NgayKetThuc, p.TrangThai, Progress = total > 0 ? (int)((double)completed / total * 100) : 0 };
                }),
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
