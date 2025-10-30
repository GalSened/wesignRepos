using Common.Interfaces.DB;
using Common.Interfaces;
using Common.Interfaces.MessageSending.Sms;
using Common.Interfaces.MessageSending.SMS;
using Common.Models.Settings;


using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Options;
using System.Net.Http;


namespace Common.Handlers.SendingMessages.SMS
{
    public class SmsNotifyG2HandlerFactory : ISmsProviderHandlerFactory
    {
        private ILogger _logger;
        private IOptions<GeneralSettings> _generalSettings;
        private IEncryptor _encryptor;
        private IServiceScopeFactory _scopeFactory;
        private IHttpClientFactory _httpClientFactory;
        private IOptions<EnvironmentExtraInfo> _environmentExtraInfo;

        public SmsNotifyG2HandlerFactory(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor 
            , IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, IOptions<EnvironmentExtraInfo> environmentExtraInfo)
        {
            _logger = logger;
            _generalSettings = generalSettings;
            _encryptor = encryptor;
    
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _environmentExtraInfo = environmentExtraInfo;
        }
        public ISmsProvider Create() => new SmsNotifyG2Handler(_logger, _generalSettings, _encryptor, _scopeFactory, _httpClientFactory, _environmentExtraInfo);
      
    }

}
