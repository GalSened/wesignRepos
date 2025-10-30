using Common.Enums.Documents;

namespace Common.Interfaces.MessageSending
{
    public interface ISendingMessageHandler
    {
        IMessageSender ExecuteCreation(SendingMethod sendingMethod);

    }
}