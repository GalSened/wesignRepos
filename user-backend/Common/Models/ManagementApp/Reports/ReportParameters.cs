using Common.Enums.PDF;
using System;
using System.Collections.Generic;

namespace Common.Models.ManagementApp.Reports
{
    public class ReportParameters
    {
        public int Offset { get; set; } // Global
        public int Limit { get; set; } // Global
        public string ProgramUtilizationHistoryKey { get; set; } // 1
        public bool? IsProgramUtilizationExpired { get; set; } // 1
        public int MonthsForAvgUse { get; set; } // 5
        public Guid ProgramId { get; set; } // 2
        public int DocsUsagePercentage { get; set; } // 2
        public Guid CompanyId { get; set; } // 9
        public List<Guid> GroupIds { get; set; } // 5
        public int MinDocs { get; set; } // 1
        public int MinSms { get; set; } // 1
        public bool? IsProgramUsed { get; set; } // 1
        public string UserEmail { get; set; } // 1
    }
}
