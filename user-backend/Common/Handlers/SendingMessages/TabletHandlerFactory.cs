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
    public class TabletHandlerFactory : ISendingMessageHandlerFactory
    {
        private readonly GeneralSettings _generalSettings;
        private readonly ILogger _logger;

        public TabletHandlerFactory(ILogger logger, IConfiguration configuration, IEmailProvider emailProvider, IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings,
            IServiceScopeFactory scopeFactory, IEmailTypeHandler emailTypeHandler, IOptions<GeneralSettings> generalSettings)
        {
            _generalSettings = generalSettings.Value;
            _logger = logger;
        }

        public IMessageSender Create() => new TabletHandler(_generalSettings, _logger);
    }
}
