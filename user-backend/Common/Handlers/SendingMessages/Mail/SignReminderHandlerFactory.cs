using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Interfaces;
using Common.Models.Settings;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using Microsoft.Extensions.Options;
using Common.Interfaces.Files;

namespace Common.Handlers.SendingMessages.Mail
{
    public class SignReminderHandlerFactory : IEmailTypeHandlerFactory
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;
        private readonly IAppendices _appendices;


        public SignReminderHandlerFactory(IEmailProvider emailProvider, IShared shared, IConfiguration configuration, IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings, IAppendices appendices)
        {
            _emailProvider = emailProvider;
            _shared = shared;
            _appendices = appendices;
        }

        public IEmailType Create() => new SignReminderHandler(_emailProvider, _shared, _appendices);
    }
}
