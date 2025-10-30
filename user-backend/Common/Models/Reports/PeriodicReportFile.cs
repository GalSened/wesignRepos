using System;

namespace Common.Models.Reports
{
    public class PeriodicReportFile
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
