using Common.Models.ManagementApp.Reports;
using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class AllGroupReportsDTO
    {
        public  IEnumerable<GroupUtilizationReport> groupReports { get; set; }
    }
}
