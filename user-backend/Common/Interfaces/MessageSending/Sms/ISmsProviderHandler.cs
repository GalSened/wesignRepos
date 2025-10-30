using Common.Enums;

namespace Common.Interfaces.MessageSending.Sms
{
    public interface ISmsProviderHandler
    {
        ISmsProvider ExecuteCreation(ProviderType providerType);
    }
}
