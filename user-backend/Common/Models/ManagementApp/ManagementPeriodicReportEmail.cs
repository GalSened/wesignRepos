using Common.Models.ManagementApp.Reports;
using System;

namespace Common.Models.ManagementApp
{
    public class ManagementPeriodicReportEmail
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public Guid PeriodicReportId { get; set; }
        public virtual ManagementPeriodicReport Report { get; set; }
    }
}