using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class AllUsageByCompanyReportsDTO
    {
        public IEnumerable<UsageByCompanyReportDTO> UsageByCompaniesReports { get; set; }
    }
}
