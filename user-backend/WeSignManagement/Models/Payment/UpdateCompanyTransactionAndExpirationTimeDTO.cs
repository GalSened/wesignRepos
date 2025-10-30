using System;

namespace WeSignManagement.Models.Payment
{
    public class UpdateCompanyTransactionAndExpirationTimeDTO
    {
        public Guid CompanyId { get; set; }
        public DateTime NewExpirationTime { get; set; }
        public string TransactionId { get; set; }
    }
}
