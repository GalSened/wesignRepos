/*
* If SMS message (MessageBefore and MessageAfter) contain [LINK] place order, original link will be replace that place order.
* Otherwise, link will be set at end of message.
* If SMS message (MessageBefore) contain [DOCUMENT_NAME] place order, original document name will be replace that place order.
* Otherwise, there is no document name will be put in message.
*/

namespace Common.Models.Configurations
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Enums.Users;
    using System;

    public class CompanyMessage
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public SendingMethod SendingMethod { get; set; }
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public Language Language { get; set; }
    }
}
