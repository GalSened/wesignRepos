using Common.Interfaces;
using Common.Interfaces.Emails;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending.Mail;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace Common.Handlers.SendingMessages.Mail
{
    public class BeforeSigningHandlerFactory : IEmailTypeHandlerFactory
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;
        private readonly IAppendices _appendices;

        public BeforeSigningHandlerFactory(IEmailProvider emailProvider, IShared shared, IConfiguration configuration, IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings, IAppendices appendices)
        {
            _emailProvider = emailProvider;
            _shared = shared;
            _appendices = appendices;
        }

        public IEmailType Create() => new BeforeSigningHandler(_emailProvider, _shared, _appendices);
    }
}
