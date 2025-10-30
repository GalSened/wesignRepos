using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.MessageSending.Sms;
using Common.Interfaces.MessageSending.SMS;
using Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Common.Handlers.SendingMessages.SMS
{
    class SmsMicropayHandlerFactory : ISmsProviderHandlerFactory
    {
        private readonly ILogger _logger;
        private readonly IOptions<GeneralSettings> _generalSetting;
        private readonly IEncryptor _encryptor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public SmsMicropayHandlerFactory(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor,  IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, IOptions<EnvironmentExtraInfo> environmentExtraInfo)
        {
            _logger = logger;
            _generalSetting = generalSettings;
            _encryptor = encryptor;
    
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
        }
        public ISmsProvider Create() => new SmsMicropayHandler(_logger, _generalSetting, _encryptor, _scopeFactory, _httpClientFactory);
    }
}
