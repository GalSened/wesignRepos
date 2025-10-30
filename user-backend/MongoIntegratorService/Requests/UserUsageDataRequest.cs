using HistoryIntegratorService.Common.Enums;

namespace HistoryIntegratorService.Requests
{
    public class UserUsageDataRequest
    {
        public Guid UserId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<DocumentStatus>? DocumentStatuses { get; set; }
        public List<Guid>? GroupIds { get; set; }
        public bool IncludeDistributionDocs { get; set; }
    }
}
