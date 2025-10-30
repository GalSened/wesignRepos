namespace HistoryIntegratorService.Requests
{
    public class UsageByCompanyAndGroupsRequest
    {
        public Guid CompanyId { get; set; }
        public IEnumerable<Guid> GroupIds { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
