using Common.Models.ManagementApp;
using DAL.DAOs.Reports;

namespace DAL.Extensions
{
    public static class ManagementPeriodicReportEmailExtensions
    {
        public static ManagementPeriodicReportEmail ToManagementPeriodicReportEmail(this ManagementPeriodicReportEmailDAO managementPeriodicReportEmailDAO)
        {
            if (managementPeriodicReportEmailDAO == null)
            {
                return null;
            }

            var emailReport = new ManagementPeriodicReportEmail()
            {
                Id = managementPeriodicReportEmailDAO.Id,
                Email = managementPeriodicReportEmailDAO.Email,
                PeriodicReportId = managementPeriodicReportEmailDAO.PeriodicReportId,
                Report = managementPeriodicReportEmailDAO.Report.ToManagementPeriodicReport()
            };

            return emailReport;
        }
    }
}
