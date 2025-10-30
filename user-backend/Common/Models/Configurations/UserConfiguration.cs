/*
    ShouldSendSignedDocument = Should user need sent signed document to customer after customer signed them
    ShouldNotifyWhileSignerSigned = Should user need to be notified when someone signs a document on an offline Signing Process
    
 */

namespace Common.Models.Configurations
{
    using Common.Enums.Users;

    public class UserConfiguration
    {
        private const string BLUE_COLOR = "#0000ff";

        public string SignatureColor { get; set; }
        public bool ShouldSendSignedDocument { get; set; }
        public bool ShouldNotifyWhileSignerSigned { get; set; }
        public bool ShouldNotifyWhileSignerViewed { get; set; }
        public bool ShouldDisplayNameInSignature { get; set; }
        public bool ShouldNotifySignReminder { get; set; }
        public int SignReminderFrequencyInDays { get; set; }
        public Language Language { get; set; }
        public UserConfiguration()
        {
            Language = Language.en;
            SignatureColor = BLUE_COLOR;
            ShouldSendSignedDocument = true; 
            ShouldNotifyWhileSignerSigned = true;
        }
    }
}
