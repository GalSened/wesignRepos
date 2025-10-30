using HistoryIntegratorService.Common.Enums;

namespace HistoryIntegratorService.Requests
{
    public class UsageByCompanyAndSignatureTypesRequest
    {
        public Guid CompanyId { get; set; }
        public IEnumerable<SignatureFieldType> SignatureFieldTypes { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
