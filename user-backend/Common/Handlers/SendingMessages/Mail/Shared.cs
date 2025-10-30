using Common.Enums;
using Common.Enums.Users;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Emails;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class Shared : IShared
    {
        private const string IMAGE_MIME_TYPE = "data:image/png;base64,";

        private readonly IConfiguration _configuration;
        private readonly IFilesWrapper _filesWrapper;
        public Shared(IConfiguration configuration, IFilesWrapper filesWrapper)
        {
            _configuration = configuration;            
            _filesWrapper = filesWrapper;
        }

        public void RemoveButton(Email email)
        {
            int startIndexHyperlink = email.HtmlBody.TemplateText.IndexOf("<a");
            int endIndexHyperlink = email.HtmlBody.TemplateText.IndexOf("a>");
            string temp = email.HtmlBody.TemplateText.Remove(startIndexHyperlink, endIndexHyperlink + 2 - startIndexHyperlink);

            int tempIndex = temp.IndexOf("class=\"cta-button\" bgcolor=\"");
            temp = temp.Remove(tempIndex + 28, 7);

            email.HtmlBody.TemplateText = temp;
        }
        public string GetContactNameFormat(Contact contact)
        {
            string delimiter = !string.IsNullOrWhiteSpace(contact?.Phone) && !string.IsNullOrWhiteSpace(contact?.Email) ? " / " : "";
            return $"{contact?.Name} ({contact?.Phone}{delimiter}{contact?.Email})";
        }
        /// <summary>
        /// Load Email body and logo.
        /// In addition, Get constant strings from language json files and init common htmlBody parameters.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<Resource> InitEmail(Email email, User user, MessageType messageType)
        {
            var language = await _configuration.GetLanguage(user);
            Resource resource = _filesWrapper.Configurations.ReadEmailsResource(language);            
            email.HtmlBody.TemplateText = _filesWrapper.Configurations.GetEmailTemplate(user, messageType);
            email.HtmlBody.Logo = $"{IMAGE_MIME_TYPE}{_filesWrapper.Configurations.GetLogo(user)}";
            email.HtmlBody.Date = GetDateByLanguage(language);
            var textDirection = language == Language.en ? TextDirection.LTR : TextDirection.RTL;
            email.HtmlBody.TextDirection = textDirection;
            email.HtmlBody.ClientName = $"{email.HtmlBody.ClientName}";
            email.HtmlBody.CopyrightText = resource.Copyright;
            email.HtmlBody.AttachmentDoNotReply = resource.AttachmentDoNotReply;
            email.HtmlBody.DigitalText = resource.Digital;
            email.HtmlBody.VisitText = resource.Visit;
            email.DisplayName = user.Name;

            return resource;
        }
        public void LoadEmailAttachments(DocumentCollection documentCollection, Email email, bool shouldSendSignedDocument)
        {
            if (shouldSendSignedDocument)
            {
                foreach (var document in documentCollection.Documents ?? Enumerable.Empty<Document>())
                {
                    
                    string fileName = GetAttachmentName(documentCollection, document);
                   
                    byte[] attachmentBytes = _filesWrapper.Documents.ReadDocument(DocumentType.Document, document.Id);
                    Stream stream = new MemoryStream(attachmentBytes);
                    email.Attachments.Add(new EmailAttachement(fileName, stream));
                    
                }
            }
        }

        #region Private Funtions

        private string GetAttachmentName(DocumentCollection documentCollection, Document document)
        {
            if (documentCollection.Documents.Count() == 1)
            {
                return documentCollection.Name.ToLower().EndsWith(".pdf") ? documentCollection.Name : $"{documentCollection.Name}.pdf";
            }

            return document.Name.ToLower().EndsWith(".pdf") ? document.Name : $"{document.Name}.pdf";
        }

        private string GetDateByLanguage(Language userLanguage)
        {
            if (userLanguage == Language.he)
            {
                var provider = CultureInfo.CreateSpecificCulture("he-IL");
                return DateTime.Now.ToString(Consts.Consts.DATE_FORMAT, provider);
            }
            return DateTime.Now.ToString(Consts.Consts.DATE_FORMAT);
        }

        #endregion
    }
}
