using Common.Models.Reports;

namespace WeSign.Models.Reports.Response
{
    public class UsageDataReportDTO
    {
        public string GroupName { get; set; }
        public int PendingDocumentsCount { get; set; }
        public int SignedDocumentsCount { get; set; }
        public int DeclinedDocumentsCount { get; set; }
        public int CanceledDocumentsCount { get; set; }
        public int DistributionDocumentsCount { get; set; }

        public UsageDataReportDTO(UsageDataReport usageDataReport)
        {
            GroupName = usageDataReport.GroupName;
            PendingDocumentsCount = usageDataReport.PendingDocumentsCount;
            SignedDocumentsCount = usageDataReport.SignedDocumentsCount;
            DeclinedDocumentsCount = usageDataReport.DeclinedDocumentsCount;
            CanceledDocumentsCount = usageDataReport.CanceledDocumentsCount;
            DistributionDocumentsCount = usageDataReport.DistributionDocumentsCount;
        }
    }
}
