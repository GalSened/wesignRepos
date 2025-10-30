namespace DAL.DAOs.Configurations
{
    using Common.Enums.Users;
    using Common.Models.Configurations;
    using DAL.DAOs.Users;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("UserConfigurations")]
    public class UserConfigurationDAO
    {
        [Key]
        public Guid UserId { get; set; }
        public string SignatureColor { get; set; }
        public bool ShouldSendSignedDocument { get; set; }
        public bool ShouldNotifyWhileSignerSigned { get; set; }
        public bool shouldNotifyWhileSignerViewed { get; set; }
        public bool ShouldDisplayNameInSignature { get; set; }
        public bool ShouldNotifySignReminder { get; set; }
        public int SignReminderFrequencyInDays { get; set; }
        public Language Language { get; set; }
        public virtual UserDAO User { get; set; }

        public UserConfigurationDAO() { }
        public UserConfigurationDAO(UserConfiguration userConfiguration)
        {
            SignatureColor = userConfiguration.SignatureColor;
            ShouldSendSignedDocument = userConfiguration.ShouldSendSignedDocument;
            ShouldNotifyWhileSignerSigned = userConfiguration.ShouldNotifyWhileSignerSigned;
            ShouldDisplayNameInSignature = userConfiguration.ShouldDisplayNameInSignature;
            Language = userConfiguration.Language;
        }
    }
}
