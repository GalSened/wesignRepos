using Common.Enums.Documents;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending;
using Common.Interfaces.MessageSending.Mail;
using Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace Common.Handlers.SendingMessages
{
    public class SendingMessageHandler : ISendingMessageHandler
    {
        private readonly IDictionary<SendingMethod, ISendingMessageHandlerFactory> _factories;

        public SendingMessageHandler(ILogger logger, IConfiguration configuration, IEmailProvider emailProvider, IFilesWrapper filesWrapper, 
            IOptions<FolderSettings> folderSettings,
            IServiceScopeFactory scopeFactory, IEmailTypeHandler emailTypeHandler, IOptions<GeneralSettings> generalSettings)
        {
            _factories = new Dictionary<SendingMethod, ISendingMessageHandlerFactory>();
            foreach (SendingMethod sendingMethod in Enum.GetValues(typeof(SendingMethod)))
            {
                var parameters = new object[8];
                parameters[0] = logger;
                parameters[1] = configuration;
                parameters[2] = emailProvider;             
                parameters[3] = filesWrapper;
                parameters[4] = folderSettings;
                parameters[5] = scopeFactory;
                parameters[6] = emailTypeHandler;
                parameters[7] = generalSettings;
                var type = Type.GetType($"Common.Handlers.SendingMessages.{Enum.GetName(typeof(SendingMethod), sendingMethod)}HandlerFactory");
                var factory = (ISendingMessageHandlerFactory)Activator.CreateInstance(type, parameters);
                _factories.Add(sendingMethod, factory);
            }
        }

        public IMessageSender ExecuteCreation(SendingMethod sendingMethod) => _factories[sendingMethod].Create();
    }
}
