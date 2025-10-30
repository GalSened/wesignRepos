namespace HistoryIntegratorService.Common.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
    }
}
