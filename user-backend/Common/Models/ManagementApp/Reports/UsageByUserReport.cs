using System;

namespace Common.Models.ManagementApp.Reports
{
    public class UsageByUserReport
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
    }
}
