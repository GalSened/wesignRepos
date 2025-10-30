using Common.Interfaces.DB;
using Common.Interfaces;
using Common.Interfaces.MessageSending.Sms;
using Common.Interfaces.MessageSending.SMS;
using Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Serilog;
using Microsoft.Extensions.Options;

namespace Common.Handlers.SendingMessages.SMS
{
    public class SmsNotifyHandlerFactory : ISmsProviderHandlerFactory
    {
        private readonly ILogger _logger;
        private readonly IOptions<GeneralSettings> _generalSettings;
        private readonly IEncryptor _encryptor;
       
        private readonly IServiceScopeFactory _scopeFactory;

        public SmsNotifyHandlerFactory(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor, IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, IOptions<EnvironmentExtraInfo> environmentExtraInfo)
        {
            _logger = logger;
            _generalSettings = generalSettings;
            _encryptor = encryptor;
       
            _scopeFactory = scopeFactory;
        }
    public ISmsProvider Create() => new SmsNotifyHandler(_logger, _generalSettings, _encryptor  ,_scopeFactory);
    }
}
