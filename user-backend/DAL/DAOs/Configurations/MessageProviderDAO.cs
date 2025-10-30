namespace DAL.DAOs.Configurations
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Models.Configurations;
    using DAL.DAOs.Companies;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MessageProviders")]
    public class MessageProviderDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public SendingMethod SendingMethod { get; set; }
        public ProviderType ProviderType { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string From { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public virtual CompanyConfigurationDAO CompanyConfiguration { get; set; }

        public MessageProviderDAO() { }

        public MessageProviderDAO(MessageProvider messageProvider)
        {
            Id = messageProvider.Id;
            CompanyId = messageProvider.CompanyId;
            SendingMethod = messageProvider.SendingMethod;
            ProviderType = messageProvider.ProviderType;
            Server = messageProvider.Server;
            Port = messageProvider.Port;
            From = messageProvider.From;
            User = messageProvider.User;
            Password = messageProvider.Password;
            EnableSsl = messageProvider.EnableSsl;
        }
    }
}
