
namespace DAL.DAOs.Configurations
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Enums.Users;
    using Common.Models.Configurations;
    using DAL.DAOs.Companies;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("CompanyMessages")]
    public class CompanyMessageDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public SendingMethod SendingMethod { get; set; }
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public virtual CompanyConfigurationDAO CompanyConfiguration { get; set; }
        public Language Language { get; set; }

        public CompanyMessageDAO() { }

        public CompanyMessageDAO(CompanyMessage companyMessage)
        {
            Id = companyMessage.Id;
            CompanyId = companyMessage.CompanyId;
            SendingMethod = companyMessage.SendingMethod;
            MessageType = companyMessage.MessageType;
            Content = companyMessage.Content;
            Language = companyMessage?.Language ?? Language.en;
        }
    }
}
