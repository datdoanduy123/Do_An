using Apllication.IRepositories;
using Apllication.IService;
using Domain.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly ICongViecRepository _taskRepo;
        private readonly IDuAnRepository _projectRepo;

        public DashboardService(ICongViecRepository taskRepo, IDuAnRepository projectRepo)
        {
            _taskRepo = taskRepo;
            _projectRepo = projectRepo;
        }

        public async Task<object> GetDashboardStatsAsync(int userId)
        {
            // Lấy tất cả dự án
            var allProjects = await _projectRepo.GetAllAsync();
            
            // Lấy task cá nhân
            var myTasks = await _taskRepo.GetByAssigneeIdAsync(userId);
            
            // Lấy task chờ duyệt (nếu là quản lý thì có thể lấy toàn bộ nhưng ở đây tính đơn giản lấy những cái Review)
            var allTasks = await _taskRepo.GetAllAsync();
            var pendingReviews = allTasks.Count(t => t.TrangThai == TrangThaiCongViec.Review);

            // 1. Phân bổ trạng thái công việc (Pie Chart) - Dành cho toàn bộ hệ thống hoặc theo quyền
            var statusDistribution = allTasks
                .GroupBy(t => t.TrangThai)
                .Select(g => new { 
                    Status = g.Key.ToString(), 
                    Count = g.Count(),
                    Color = GetColorForStatus(g.Key)
                });

            // 2. Khối lượng công việc của nhân viên (Bar Chart) - Lấy Top 5 nhân viên bận nhất
            var teamWorkload = allTasks
                .Where(t => t.AssigneeId.HasValue && t.TrangThai != TrangThaiCongViec.Done)
                .GroupBy(t => t.Assignee.FullName) // Giả định Assignee có FullName
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new {
                    Name = g.Key,
                    TaskCount = g.Count()
                });

            // 3. Tiến độ dự án (Line/Area Chart)
            var projectProgress = allProjects.Select(p => {
                var projectTasks = allTasks.Where(t => t.DuAnId == p.Id).ToList();
                var total = projectTasks.Count;
                var completed = projectTasks.Count(t => t.TrangThai == TrangThaiCongViec.Done);
                var progress = total > 0 ? (int)((double)completed / total * 100) : 0;
                
                return new {
                    ProjectName = p.TenDuAn,
                    Progress = progress
                };
            }).OrderByDescending(p => p.Progress).Take(5);

            return new
            {
                TotalProjects = allProjects.Count(),
                CompletedTasks = myTasks.Count(t => t.TrangThai == TrangThaiCongViec.Done),
                InProgressTasks = myTasks.Count(t => t.TrangThai == TrangThaiCongViec.InProgress),
                PendingReviews = pendingReviews,
                TaskStatusDistribution = statusDistribution,
                TeamWorkload = teamWorkload,
                ProjectProgress = projectProgress,
                RecentProjects = allProjects.OrderByDescending(p => p.Id).Take(3).Select(p => new {
                    p.Id,
                    p.TenDuAn,
                    p.NgayBatDau,
                    p.NgayKetThuc,
                    p.TrangThai
                }),
                MyPriorityTasks = myTasks.Where(t => t.TrangThai != TrangThaiCongViec.Done && t.DoUuTien >= DoUuTien.High).Take(5).Select(t => new {
                    t.Id,
                    t.TieuDe,
                    t.DoUuTien,
                    t.ThoiGianUocTinh
                })
            };
        }

        private string GetColorForStatus(TrangThaiCongViec status)
        {
            return status switch
            {
                TrangThaiCongViec.Todo => "#94a3b8",      // Slate
                TrangThaiCongViec.InProgress => "#6366f1", // Indigo
                TrangThaiCongViec.Review => "#f59e0b",     // Amber
                TrangThaiCongViec.Done => "#10b981",       // Emerald
                TrangThaiCongViec.Cancelled => "#ef4444", // Red
                _ => "#cbd5e1"
            };
        }
    }
}
