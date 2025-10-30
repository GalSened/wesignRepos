using Common.Models.Configurations;
using Common.Models.ManagementApp;
using Common.Models.ManagementApp.Reports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.ManagementApp
{
    public interface IManagementPeriodicReports
    {
        Task SendManagementReportToUsers(Configuration configuration, ManagementPeriodicReport report, IEnumerable<ManagementPeriodicReportEmail> reportEmails);
    }
}
