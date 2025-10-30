namespace HistoryIntegratorService.Common.Models.UserReports
{
    public class UsageDataReport
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public int SentDocumentsCount { get; set; }
        public int SignedDocumentsCount { get; set; }
        public int DeclinedDocumentsCount { get; set; }
        public int CanceledDocumentsCount { get; set; }
        public int DistributionDocumentsCount { get; set; }
        public UsageDataReport()
        {

        }
    }
}
