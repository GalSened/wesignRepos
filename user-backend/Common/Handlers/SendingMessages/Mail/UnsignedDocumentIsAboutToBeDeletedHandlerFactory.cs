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
    public class UnsignedDocumentIsAboutToBeDeletedHandlerFactory : IEmailTypeHandlerFactory
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;

        public UnsignedDocumentIsAboutToBeDeletedHandlerFactory(IEmailProvider emailProvider, IShared shared, IConfiguration configuration, IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings, IAppendices appendices)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }

        public IEmailType Create() => new UnsignedDocumentIsAboutToBeDeletedHandler(_emailProvider, _shared);
    }
}
