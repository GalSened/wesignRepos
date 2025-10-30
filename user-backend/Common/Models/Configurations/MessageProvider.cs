
namespace Common.Models.Configurations
{
    using Common.Enums;
    using Common.Enums.Documents;
    using System;

    public class MessageProvider
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public SendingMethod SendingMethod { get; set; }
        public ProviderType ProviderType { get; set; }
        public string From { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }

    }
}
