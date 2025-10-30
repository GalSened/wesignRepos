using Common.Models.ManagementApp.Reports;
using System;
using System.Collections.Generic;

namespace WeSignManagement.Models.Companies
{
    public class CompanyReportDTO
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

        public CompanyReportDTO(CompanyUtilizationReport companyReport)
        {
            CompanyId = companyReport.CompanyId;
            CompanyName = companyReport.CompanyName;
            ProgramStartDate = companyReport.ProgramStartDate;
            ExpireDate = companyReport.ExpireDate;
            DocumentsUsage = companyReport.DocumentsUsage;
            SmsUsage = companyReport.SmsUsage;
            PeriodicDocumentUsage = companyReport.PeriodicDocumentUsage;
            PeriodicSMSUsage = companyReport.PeriodicSMSUsage;
            LastMonthDocumentUsagePercentage = companyReport.LastMonthDocumentUsagePercentage;
 
        }
    }
    
}
