using Common.Models.ManagementApp.Reports;
using System;

namespace WeSignManagement.Models.Companies.Responses
{
    public class UsageByUserReportDTO
    {
        public string CompanyName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string Email { get; set; }
        public int SentDocumentsCount { get; set; }
        public int SignedDocumentsCount { get; set; }
        public int DeclinedDocumentsCount { get; set; }
        public int CanceledDocumentsCount { get; set; }
        public int DeletedDocumentsCount { get; set; }

        public UsageByUserReportDTO(UsageByUserReport usageByUserReport)
        {
            CompanyName = usageByUserReport.CompanyName;
            GroupId = usageByUserReport.GroupId;
            GroupName = usageByUserReport.GroupName;
            Email = usageByUserReport.Email;
            SentDocumentsCount = usageByUserReport.SentDocumentsCount;
            SignedDocumentsCount = usageByUserReport.SignedDocumentsCount;
            DeclinedDocumentsCount = usageByUserReport.DeclinedDocumentsCount;
            CanceledDocumentsCount = usageByUserReport.CanceledDocumentsCount;
            DeletedDocumentsCount = usageByUserReport.DeletedDocumentsCount;
        }
    }
}
