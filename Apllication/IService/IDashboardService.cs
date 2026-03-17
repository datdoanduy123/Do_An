using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface IDashboardService
    {
        Task<object> GetDashboardStatsAsync(int userId);
    }
}
