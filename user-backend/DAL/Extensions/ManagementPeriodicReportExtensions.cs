using Common.Models.ManagementApp.Reports;
using DAL.DAOs.Management;
using System.Linq;

namespace DAL.Extensions
{
    public static class ManagementPeriodicReportExtensions
    {
        public static ManagementPeriodicReport ToManagementPeriodicReport(this ManagementPeriodicReportDAO managementPeriodicReportDAO)
        {
            if (managementPeriodicReportDAO == null)
            {
                return null;
            }
            var report =  new ManagementPeriodicReport()
            {
                Id = managementPeriodicReportDAO.Id,
                UserId = managementPeriodicReportDAO.UserId,
                ReportType = managementPeriodicReportDAO.ReportType,
                LastTimeSent = managementPeriodicReportDAO.LastTimeSent,
                ReportFrequency = managementPeriodicReportDAO.ReportFrequency,
                ReportParameters = managementPeriodicReportDAO.ReportParameters,
                User = managementPeriodicReportDAO.User.ToUser()
            };

            if (managementPeriodicReportDAO.Emails != null)
            {
                report.Emails = managementPeriodicReportDAO.Emails.Select(_ => _.ToManagementPeriodicReportEmail()).ToList();
            }

            return report;
        }
    }
}
