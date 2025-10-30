using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.Emails;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending.Mail;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace Common.Handlers.SendingMessages.Mail
{
    public class EmailTypeHandler : IEmailTypeHandler
    {
        private readonly IDictionary<MessageType, IEmailTypeHandlerFactory> _factories;

        public EmailTypeHandler(IEmailProvider emailProvider, IShared shared, IConfiguration configuration, IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings, IAppendices appendices)
        {
            _factories = new Dictionary<MessageType, IEmailTypeHandlerFactory>();
            foreach (MessageType messageType in Enum.GetValues(typeof(MessageType)))
            {
                var parameters = new object[6];
                parameters[0] = emailProvider;
                parameters[1] = shared;
                parameters[2] = configuration;
                parameters[3] = filesWrapper;
                parameters[4] = folderSettings;
                parameters[5] = appendices;
                var factory = (IEmailTypeHandlerFactory)Activator.CreateInstance(Type.GetType("Common.Handlers.SendingMessages.Mail." + Enum.GetName(typeof(MessageType), messageType) + "HandlerFactory"), parameters);
                _factories.Add(messageType, factory);
            }
        }

        public IEmailType ExecuteCreation(MessageType messageType) => _factories[messageType].Create();
    }
}
