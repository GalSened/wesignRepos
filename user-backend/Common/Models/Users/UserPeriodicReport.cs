using Common.Enums.Reports;
using System;

namespace Common.Models.Users
{
    public class UserPeriodicReport
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ReportType ReportType { get; set; }
        public DateTime LastTimeSent { get; set; }
        public ReportFrequency ReportFrequency { get; set; }
        public virtual User User { get; set; }

        public DateTime GetReportStartTime(DateTime? endTime = null)
        {
            if (this == null)
            {
                return DateTime.Now;
            }
            var timeToCheckFrom = endTime.HasValue ? endTime.Value : LastTimeSent;
            switch (ReportFrequency)
            {
                case ReportFrequency.None:
                    return timeToCheckFrom;
                case ReportFrequency.Daily:
                    return timeToCheckFrom.AddDays(-1);
                case ReportFrequency.Weekly:
                    return timeToCheckFrom.AddDays(-7);
                case ReportFrequency.Monthly:
                    return timeToCheckFrom.AddMonths(-1);
                default:
                    return timeToCheckFrom;
            }
        }
    }
}

