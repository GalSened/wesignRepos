using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.MessageSending.Sms;
using Common.Interfaces.MessageSending.SMS;
using Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System.Net.Http;

namespace Common.Handlers.SendingMessages.SMS
{
    class SmsInforUMobileHandlerFactory : ISmsProviderHandlerFactory
    {
        private readonly ILogger _logger;
        private readonly IOptions<GeneralSettings> _generalSetting;
        private readonly IEncryptor _encryptor;

        private readonly IServiceScopeFactory _scopeFactory;
        public SmsInforUMobileHandlerFactory(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor,  IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, IOptions<EnvironmentExtraInfo> environmentExtraInfo)
        {
            _logger = logger;
            _generalSetting = generalSettings;
            _encryptor = encryptor;
 
            _scopeFactory = scopeFactory;
        }
        public ISmsProvider Create() => new SmsInforUMobileHandler(_logger, _generalSetting, _encryptor, _scopeFactory);
    }
}
