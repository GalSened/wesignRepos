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
using System.IO.Abstractions;

namespace Common.Handlers.SendingMessages
{
    public class SMSHandlerFactory : ISendingMessageHandlerFactory
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FolderSettings _folderSettings;
        private readonly IEmailProvider _emailProvider;
        
        

        public SMSHandlerFactory(ILogger logger, IConfiguration configuration, IEmailProvider emailProvider, IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings,
            IServiceScopeFactory scopeFactory, IEmailTypeHandler emailTypeHandler, IOptions<GeneralSettings> generalSettings)
        {
            _logger = logger;
            _emailProvider = emailProvider;
          
            _folderSettings = folderSettings.Value;
            _configuration = configuration;

            _scopeFactory = scopeFactory;
        }
        public IMessageSender Create() => new SmsHandler(_logger, _configuration, _scopeFactory);

    }
}

