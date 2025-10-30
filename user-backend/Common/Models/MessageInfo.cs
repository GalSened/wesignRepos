/*
* If SMS message (MessageBefore and MessageAfter) contain [LINK] place order, original link will be replace that place order.
* Otherwise, link will be set at end of message.
* If SMS message (MessageBefore) contain [DOCUMENT_NAME] place order, original document name will be replace that place order.
* Otherwise, there is no document name will be put in message.
*/

namespace Common.Models
{
    using Common.Enums;
    public class MessageInfo
    {
        public MessageType MessageType { get; set; }
        public User User { get; set; }
        public Contact Contact { get; set; }
        public DocumentCollection DocumentCollection{ get; set; }
        public string Link{ get; set; }
        public string MessageContent { get; set; }
    }
}
