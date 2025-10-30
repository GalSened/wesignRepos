using Common.Models.Users;
using DAL.DAOs.Users;

namespace DAL.Extensions
{
    public static class UserPeriodicReportExtensions
    {
        public static UserPeriodicReport ToUserPeriodicReport(this UserPeriodicReportDAO userPeriodicReportDAO) 
        {
            return userPeriodicReportDAO == null ? null : new UserPeriodicReport()
            {
                Id = userPeriodicReportDAO.Id,
                UserId = userPeriodicReportDAO.UserId,
                ReportType = userPeriodicReportDAO.ReportType,
                LastTimeSent = userPeriodicReportDAO.LastTimeSent,
                ReportFrequency = userPeriodicReportDAO.ReportFrequency,
                User = userPeriodicReportDAO.User.ToUser()
            };
        }
    }
}
