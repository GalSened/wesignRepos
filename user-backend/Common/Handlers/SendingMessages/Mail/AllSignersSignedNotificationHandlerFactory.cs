using Common.Handlers.Files;
using Common.Interfaces;
using Common.Interfaces.Emails;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending.Mail;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;

namespace Common.Handlers.SendingMessages.Mail
{
    class AllSignersSignedNotificationHandlerFactory : IEmailTypeHandlerFactory
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;
        private readonly IFilesWrapper _filesWrapper;
        

        public AllSignersSignedNotificationHandlerFactory(IEmailProvider emailProvider, IShared shared, IConfiguration configuration, IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings, IAppendices appendices)
        {
            _emailProvider = emailProvider;
            _shared = shared;
            _filesWrapper = filesWrapper;



        }

        public IEmailType Create() => new AllSignersSignedNotificationHandler(_emailProvider, _shared, _filesWrapper);
    }
}
