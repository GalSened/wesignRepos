using Common.Models.Configurations;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WeSignManagement.Models.Companies
{
    public class NotificationsDTO
    {
        public bool ShouldSendSignedDocument { get; set; }
        public bool ShouldNotifyWhileSignerSigned { get; set; }
        public bool ShouldEnableSignReminders { get; set; }
        public int SignReminderFrequencyInDays { get; set; }
        public bool CanUserControlReminderSettings { get; set; }
        public int SignerLinkExpirationInHours { get; set; }
        public bool ShouldSendDocumentNotifications { get; set; }
        public string DocumentNotificationsEndpoint { get; set; }


        public IEnumerable<NotificationDTO> NotificationMessages { get; set; }

        public NotificationsDTO(CompanyConfiguration companyConfiguration)
        {
            ShouldSendSignedDocument = companyConfiguration?.ShouldSendSignedDocument ?? false;
            ShouldNotifyWhileSignerSigned = companyConfiguration?.ShouldNotifyWhileSignerSigned ?? false;
            ShouldEnableSignReminders = companyConfiguration?.ShouldEnableSignReminders ?? false;
            SignReminderFrequencyInDays = companyConfiguration?.SignReminderFrequencyInDays ?? 1;
            CanUserControlReminderSettings = companyConfiguration?.CanUserControlReminderSettings ?? true;
            SignerLinkExpirationInHours = companyConfiguration?.SignerLinkExpirationInHours ?? 0;
            ShouldSendDocumentNotifications = companyConfiguration?.ShouldSendDocumentNotifications ?? false;
            DocumentNotificationsEndpoint = companyConfiguration?.DocumentNotificationsEndpoint ?? string.Empty;
        }

    }

    public class NotificationDTO
    {
        public NotificationMessageType MessageType { get; set; }
        public bool ShouldSend { get; set; }
        public int FrequencyInDays { get; set; }
        public DateTime Time { get; set; }
        public string Text { get; set; }
    }

    public enum NotificationMessageType
    {
        MsgToSenderThatNotAllSignersSigned = 1,
        MsgToSignerThatNotSignedYet = 2
    }


}
