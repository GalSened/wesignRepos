using Common.Models.Dashboard;
using System.Threading.Tasks;

namespace Common.Interfaces.Dashboard
{
    public interface IDashboard
    {
        Task<DashboardView> GetDashboardView();
    }
}
