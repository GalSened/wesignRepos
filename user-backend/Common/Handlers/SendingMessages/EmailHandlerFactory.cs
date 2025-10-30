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
using System.Text;

namespace Common.Handlers.SendingMessages
{
    public class EmailHandlerFactory : ISendingMessageHandlerFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailTypeHandler _emailTypeHandler;


        public EmailHandlerFactory(ILogger logger, IConfiguration configuration, IEmailProvider emailProvider, IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings,
            IServiceScopeFactory scopeFactory, IEmailTypeHandler emailTypeHandler, IOptions<GeneralSettings> generalSettings)
        {
            _configuration = configuration;
            _emailTypeHandler = emailTypeHandler;
        }

        public IMessageSender Create() => new EmailHandler(_configuration, _emailTypeHandler);
    }
}
