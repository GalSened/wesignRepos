namespace HistoryIntegratorService.Common.Models.ManagementReports
{
    public class UsageByCompanyReport
    {
        public string CompanyName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public int SentDocumentsCount { get; set; }
        public int SignedDocumentsCount { get; set; }
        public int DeclinedDocumentsCount { get; set; }
        public int CanceledDocumentsCount { get; set; }
    }
}
