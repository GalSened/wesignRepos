/*
 * If SMS message (MessageBefore and MessageAfter) contain [LINK] place order, original link will be replace that place order.
 * If not contain link will be set at end of message.
 * If SMS message (MessageBefore) contain [DOCUMENT_NAME] place order, original document name will be replace that place order.
 * If not contain there is no document name will be put in message.
 */

namespace Common.Models.Configurations
{
    using Common.Enums;
    using Common.Enums.Users;

    public class SmsConfiguration
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public ProviderType Provider { get; set; }
        public Language Language { get; set; }
        public bool IsProviderSupportGloballySend { get; set; }
    }
}
