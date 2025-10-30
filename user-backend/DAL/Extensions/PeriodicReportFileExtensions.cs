using Common.Models.Reports;
using DAL.DAOs.Reports;

namespace DAL.Extensions
{
    public static class PeriodicReportFileExtensions
    {
        public static PeriodicReportFile ToPeriodicReportFile(this PeriodicReportFileDAO periodicReportFileDAO)
        {
            return periodicReportFileDAO == null ? null : new PeriodicReportFile()
            {
                Id = periodicReportFileDAO.Id,
                Token = periodicReportFileDAO.Token,
                CreationTime = periodicReportFileDAO.CreationTIme
            };
        }
    }
}
