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
    public class SmsPayCallHandlerFactory : ISmsProviderHandlerFactory
    {
        private readonly ILogger _logger;
        private readonly IOptions<GeneralSettings> _generalSetting;
        private readonly IEncryptor _encryptor;
      
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public SmsPayCallHandlerFactory(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor,  IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, IOptions<EnvironmentExtraInfo> environmentExtraInfo)
        {
            _logger = logger;
            _generalSetting = generalSettings;
            _encryptor = encryptor;
 
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
        }

        public ISmsProvider Create() => new SmsPayCallHandler(_logger, _generalSetting, _encryptor, _scopeFactory, _httpClientFactory);

    }
}
