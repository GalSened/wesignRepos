using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class AllUsageByUserReportsDTO
    {
        public IEnumerable<UsageByUserReportDTO> UsageByUsersReports { get; set; }
    }
}
