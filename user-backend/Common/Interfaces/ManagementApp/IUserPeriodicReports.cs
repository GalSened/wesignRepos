using Common.Models.Configurations;
using Common.Models.Users;
using System.Threading.Tasks;

namespace Common.Interfaces.ManagementApp
{
    public interface IUserPeriodicReports
    {
        Task SendReportToUser(Configuration configuration, UserPeriodicReport report);
    }
}
