using Common.Models.ManagementApp.Reports;
using System.Collections.Generic;

namespace Common.Models.ManagementApp
{
    public class ManagementReportMessageInfo : MessageInfo
    {
        public ManagementPeriodicReport Report { get; set; }
        public IEnumerable<object> ReportsToCSV { get; set; }
    }
}
