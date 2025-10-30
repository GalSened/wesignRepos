namespace HistoryIntegratorService.Requests
{
    public class UsageByUserDetailsRequest
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Email { get; set; }
        public Guid CompanyId { get; set; }
        public IEnumerable<Guid>? GroupIds { get; set; }
    }
}
