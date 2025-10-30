using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Emails
{
    public class DNSSmtpClient: SmtpClient
    {
        public DNSSmtpClient()
        {

        }

        protected override string GetEnvelopeId(MimeMessage message)
        {
            // Since you will want to be able to map whatever identifier you return here to the
            // message, the obvious identifier to use is probably the Message-Id value.
            return message.MessageId;
        }

        protected override DeliveryStatusNotification? GetDeliveryStatusNotifications(MimeMessage message, MailboxAddress mailbox)
        {
            // In this example, we only want to be notified of failures to deliver to a mailbox.
            // If you also want to be notified of delays or successful deliveries, simply bitwise-or
            // whatever combination of flags you want to be notified about.
            return DeliveryStatusNotification.Failure;
        }
    }
}
