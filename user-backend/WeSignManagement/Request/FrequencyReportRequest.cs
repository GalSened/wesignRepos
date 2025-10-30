using Common.Enums.Reports;
using Common.Models.ManagementApp.Reports;
using System.Collections.Generic;

namespace WeSignManagement.Request
{
    public class FrequencyReportRequest
    {
        public ReportParameters ReportParameters { get; set; }
        public ManagementReportFrequency Frequency { get; set; }
        public ManagementReportType ReportType { get; set; }
        public List<string> EmailsToSend { get; set; }
    }
}
