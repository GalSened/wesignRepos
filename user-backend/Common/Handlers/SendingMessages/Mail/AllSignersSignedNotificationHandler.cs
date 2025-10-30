using Common.Enums;
using Common.Extensions;
using Common.Interfaces.Emails;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Emails;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class AllSignersSignedNotificationHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;
        
        private readonly IFilesWrapper _filesWrapper;
        

        public AllSignersSignedNotificationHandler(IEmailProvider emailProvider, IShared shared, 
             IFilesWrapper filesWrapper)
        {
            _emailProvider = emailProvider;
            _shared = shared;
            
            _filesWrapper = filesWrapper;
            
        }
        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var email = new Email() { };
            email.To = messageInfo?.User.Email;
            email.HtmlBody.ClientName = messageInfo?.User.Name;
            _shared.LoadEmailAttachments(messageInfo?.DocumentCollection, email, shouldSendSignedDocument: true);
            LoadSignersAttachments(messageInfo?.DocumentCollection?.Signers, email);
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.AllSignersSignedNotification);
            string documentName = messageInfo?.DocumentCollection?.Name;
            string contactsList = GetContactListFormat(messageInfo?.DocumentCollection?.Signers);
            string notificationText = $"{resource.TheDocument} {documentName} {resource.CompletedByAllParticipants}";
            email.Subject = notificationText;
            _shared.RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection};'>{notificationText} {contactsList}</div>";

            if (email.DocumentCollection == null)
            {
                email.DocumentCollection = messageInfo.DocumentCollection;
            }

            await _emailProvider.Send(email, config);
        }

        private string GetContactListFormat(IEnumerable<Signer> signers)
        {
            StringBuilder result = new StringBuilder();
            var signersList = signers.ToList();
            for (int i = 0; i < signersList.Count; i++)
            {
                result.Append(_shared.GetContactNameFormat(signersList[i]?.Contact));
                if (i < signersList.Count - 1)
                {
                    result.Append(", ");
                }

                result.AppendLine();
            }
            return result.ToString();
        }

        public void LoadSignersAttachments(IEnumerable<Signer> signers, Email email)
        {

            Dictionary<Guid, List<Attachment>> signerAttchments = new Dictionary<Guid, List<Attachment>>();
          
            signers.ForEach(signer => signerAttchments.Add(signer.Id, _filesWrapper.Signers.ReadSignerAttachments(signer).ToList() ));
                

            bool foldersContainsAttachments = false;
            foreach (KeyValuePair<Guid, List<Attachment>> entry in signerAttchments)
            {
                if(entry.Value.Count > 0)
                {
                    foldersContainsAttachments = true;
                    break;
                }
            }
      
            if (foldersContainsAttachments)
            {
                var zipArchive = CreateZipArchive(signerAttchments, signers);
                ContentType contentType = new ContentType
                {
                    MediaType = MediaTypeNames.Application.Zip,
                    Name = "attachments.zip",
                };

                var attachment = new EmailAttachement(contentType.Name,zipArchive);
           
                email.Attachments.Add(attachment);
            }
        }

        private MemoryStream CreateZipArchive(Dictionary<Guid, List<Attachment>> signersAttchments, IEnumerable<Signer> signers)
        {
            var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (KeyValuePair<Guid, List<Attachment>> entry in signersAttchments)
                {
                    foreach(var signerAttachment in entry.Value)
                    {
                        var signer = signers.FirstOrDefault(x => x.Id == entry.Key);
                        string attachmentName = signer?.SignerAttachments?.FirstOrDefault(x => signerAttachment.Name.Contains(x.Id.ToString()))?.Name;
                        string fileName = !string.IsNullOrWhiteSpace(attachmentName) ?                    
                           $"{signer?.Contact?.Name}_{attachmentName}.{MimeTypes.Base64TypeFormatToExtentionType[signerAttachment.ContentType.MediaType]}":
                           signerAttachment.Name;
                        var zipEntry = zip.CreateEntry(fileName);
                        using (var entryStream = zipEntry.Open())
                        {                            
                            signerAttachment.ContentStream.CopyTo(entryStream);
                            signerAttachment.ContentStream.Dispose();
                        }
                    }
                }               
            }
            zipStream.Position = 0;
            return zipStream;
        }
    }
}
