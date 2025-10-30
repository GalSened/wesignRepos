using System.Collections.Generic;

namespace WeSign.Models.Reports.Response
{
    public class AllUsageDataReportsDTO
    {
        public IEnumerable<UsageDataReportDTO> usageDataReports { get; set; }

    }
}
