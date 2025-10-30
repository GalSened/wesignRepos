using Common.Interfaces.MessageSending.Sms;

namespace Common.Interfaces.MessageSending.SMS
{
    public interface ISmsProviderHandlerFactory
    {
        ISmsProvider Create();
    }
}
