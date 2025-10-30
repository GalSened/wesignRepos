namespace Common.Models.ManagementApp.Reports
{

    using Common.Models.Programs;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CompanyUtilizationReport
    {

        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }

        public DateTime? ProgramStartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public long DocumentsUsage { get; set; }
        public long SmsUsage { get; set; }
        public long PeriodicDocumentUsage { get; set; }
        public long PeriodicSMSUsage { get; set; }
        public long LastMonthDocumentUsagePercentage { get; set; }

        public CompanyUtilizationReport(List<ProgramUtilizationHistory> programUtilizationHistories, DateTime programStartDate, int monthsForAvgUse)
        {
            CompanyId = programUtilizationHistories[0].CompanyId;
            CompanyName = programUtilizationHistories[0].CompanyName;
            ProgramStartDate = programStartDate;
            ExpireDate = programUtilizationHistories[0].Expired;
            DocumentsUsage = programUtilizationHistories.Sum(u => u.DocumentsUsage);
            SmsUsage = programUtilizationHistories.Sum(u => u.SmsUsage);
            PeriodicDocumentUsage = programUtilizationHistories.Take(monthsForAvgUse).Sum(u => u.DocumentsUsage);
            PeriodicSMSUsage = programUtilizationHistories.Take(monthsForAvgUse).Sum(u => u.SmsUsage);
            LastMonthDocumentUsagePercentage = -1;

        }

        public CompanyUtilizationReport(List<ProgramUtilizationHistory> programUtilizationHistories, ProgramUtilization programUtilization, int monthsForAvgUse)
        {
            CompanyId = programUtilizationHistories[0].CompanyId;
            CompanyName = programUtilizationHistories[0].CompanyName;
            ProgramStartDate = programUtilization.StartDate;
            ExpireDate = programUtilizationHistories[0].Expired;
            DocumentsUsage = programUtilizationHistories.Sum(u => u.DocumentsUsage) + programUtilization.DocumentsUsage;
            SmsUsage = programUtilizationHistories.Sum(u => u.SmsUsage) + programUtilization.SMS;
            PeriodicDocumentUsage = programUtilizationHistories.Take(monthsForAvgUse - 1).Sum(u => u.DocumentsUsage) + programUtilization.DocumentsUsage;
            PeriodicSMSUsage = programUtilizationHistories.Take(monthsForAvgUse - 1).Sum(u => u.SmsUsage) + programUtilization.SMS;
            LastMonthDocumentUsagePercentage = programUtilization.DocumentsLimit==0 ? -1 : (programUtilization.DocumentsUsage / programUtilization.DocumentsLimit) * 100;

        }

        public CompanyUtilizationReport(Company company, ProgramUtilization programUtilization)
        {
            CompanyId = company.Id;
            CompanyName = company.Name;
            ProgramStartDate = programUtilization.StartDate;
            ExpireDate = programUtilization.Expired;
            DocumentsUsage = programUtilization.DocumentsUsage;
            SmsUsage = programUtilization.SMS;
            PeriodicDocumentUsage = programUtilization.DocumentsUsage;
            PeriodicSMSUsage = programUtilization.SMS;
            LastMonthDocumentUsagePercentage = programUtilization.DocumentsLimit == 0 ? -1 : (programUtilization.DocumentsUsage / programUtilization.DocumentsLimit) * 100;
        }
    }
}
