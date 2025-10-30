
using Common.Enums;
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

namespace Common.Handlers.SendingMessages.SMS
{
    public class SmsProviderHandler : ISmsProviderHandler
    {
        private readonly IDictionary<ProviderType, ISmsProviderHandlerFactory> _factories;

        public SmsProviderHandler(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor, IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory,
            IOptions<EnvironmentExtraInfo> environmentExtraInfo)
        {
            _factories = new Dictionary<ProviderType, ISmsProviderHandlerFactory>();
            foreach (ProviderType providerType in Enum.GetValues(typeof(ProviderType)))
            {
                if (providerType.ToString().StartsWith("Sms"))
                {
                    var parameters = new object[6];
                    parameters[0] = logger;
                    parameters[1] = generalSettings;
                    parameters[2] = encryptor;                    
                    parameters[3] = scopeFactory;
                    parameters[4] = httpClientFactory;
                    parameters[5] = environmentExtraInfo;
                    var factory = (ISmsProviderHandlerFactory)Activator.CreateInstance(Type.GetType("Common.Handlers.SendingMessages.SMS." + Enum.GetName(typeof(ProviderType), providerType) + "HandlerFactory"), parameters);
                    _factories.Add(providerType, factory);

                }
            }
        }

        public ISmsProvider ExecuteCreation(ProviderType providerType) => _factories[providerType]?.Create();

    }
}
