using Common.Models.ManagementApp.Reports;
using System;

namespace WeSignManagement.Models.Companies.Responses
{
    public class UsageByCompanyReportDTO
    {
        public string CompanyName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public int SentDocumentsCount { get; set; }
        public int SignedDocumentsCount { get; set; }
        public int DeclinedDocumentsCount { get; set; }
        public int CanceledDocumentsCount { get; set; }

        public UsageByCompanyReportDTO(UsageByCompanyReport usageByCompanyReport)
        {
            CompanyName = usageByCompanyReport.CompanyName;
            GroupId = usageByCompanyReport.GroupId;
            GroupName = usageByCompanyReport.GroupName;
            SentDocumentsCount = usageByCompanyReport.SentDocumentsCount;
            SignedDocumentsCount = usageByCompanyReport.SignedDocumentsCount;
            DeclinedDocumentsCount = usageByCompanyReport.DeclinedDocumentsCount;
            CanceledDocumentsCount = usageByCompanyReport.CanceledDocumentsCount;
        }
    }
}
