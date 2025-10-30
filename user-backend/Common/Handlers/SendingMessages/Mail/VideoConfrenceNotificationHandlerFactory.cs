using Common.Interfaces.Emails;
using Common.Interfaces.Files;
using Common.Interfaces;
using Common.Interfaces.MessageSending.Mail;
using Common.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Common.Handlers.SendingMessages.Mail
{
    public class VideoConfrenceNotificationHandlerFactory : IEmailTypeHandlerFactory
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;
        public VideoConfrenceNotificationHandlerFactory(IEmailProvider emailProvider, IShared shared, IConfiguration configuration, 
            IFilesWrapper filesWrapper, IOptions<FolderSettings> folderSettings, IAppendices appendices)
        {
            _emailProvider = emailProvider;
           _shared = shared;
        }
        public IEmailType Create() => new VideoConfrenceNotificationHandler(_emailProvider, _shared);
    }
}
