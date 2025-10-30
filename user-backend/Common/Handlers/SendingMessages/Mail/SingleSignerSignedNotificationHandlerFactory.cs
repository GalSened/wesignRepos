using Common.Interfaces;
using Common.Interfaces.Emails;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending.Mail;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace Common.Handlers.SendingMessages.Mail
{
    class SingleSignerSignedNotificationHandlerFactory : IEmailTypeHandlerFactory
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;

        public SingleSignerSignedNotificationHandlerFactory(IEmailProvider emailProvider, IShared shared, IConfiguration configuration, IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings, IAppendices appendices)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }

        public IEmailType Create() => new SingleSignerSignedNotificationHandler(_emailProvider, _shared);
    }
}
