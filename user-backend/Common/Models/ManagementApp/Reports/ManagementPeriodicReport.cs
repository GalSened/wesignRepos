using Common.Enums.Reports;
using System;
using System.Collections.Generic;

namespace Common.Models.ManagementApp.Reports
{
    public class ManagementPeriodicReport
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ManagementReportType ReportType { get; set; }
        public DateTime LastTimeSent { get; set; }
        public ManagementReportFrequency ReportFrequency { get; set; }
        public string ReportParameters { get; set; }
        public virtual ICollection<ManagementPeriodicReportEmail> Emails { get; set; }
        public virtual User User { get; set; }
        public ManagementPeriodicReport()
        {
            Emails = new List<ManagementPeriodicReportEmail>();
        }

        public DateTime GetReportEndTime()
        {
            if (this == null)
            {
                return DateTime.MinValue;
            }
            switch (ReportFrequency)
            {
                case ManagementReportFrequency.Weekly:
                    return LastTimeSent.AddDays(7);
                case ManagementReportFrequency.Monthly:
                    return LastTimeSent.AddMonths(1);
                case ManagementReportFrequency.Yearly:
                    return LastTimeSent.AddYears(1);
                default:
                    return LastTimeSent;
            }
        }
    }
}
