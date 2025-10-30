using Common.Models.ManagementApp.Reports;
using System.Collections.Generic;

namespace WeSignManagement.Models.Reports
{
    public class AllManagementPeriodicReportsDTO
    {
        public IEnumerable<ManagementPeriodicReport> managementPeriodicReports { get; set; }
    }
}
