
namespace WeSign.Models.Users
{
    using Common.Enums.Users;

    public class UserConfigurationDTO
    {
        public string SignatureColor { get; set; }
        public bool ShouldSendSignedDocument { get; set; }
        public bool ShouldNotifyWhileSignerSigned { get; set; }
        public bool ShouldNotifyWhileSignerViewed { get; set; }
        public bool ShouldDisplayNameInSignature { get; set; }
        public bool ShouldNotifySignReminder { get; set; }
        public int SignReminderFrequencyInDays { get; set; }    
        public Language Language { get; set; }
    }
}
