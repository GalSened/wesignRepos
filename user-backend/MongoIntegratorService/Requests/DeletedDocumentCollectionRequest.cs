namespace HistoryIntegratorService.Requests
{
    public class DeletedDocumentCollectionRequest
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CompanyId { get; set; }
        public IEnumerable<Guid>? GroupIds { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }
}