using Common.Models.ManagementApp.Reports;
using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class AllFreeTrialUsersReportsDTO
    {
        public IEnumerable<FreeTrialUsersReportDTO> FreeTrialUsersReports { get; set; }
    }
}

