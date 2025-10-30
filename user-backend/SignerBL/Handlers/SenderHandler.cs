// Ignore Spelling: app

using Common.Enums;
using Common.Enums.Documents;
using Common.Interfaces;
using Common.Interfaces.MessageSending;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

namespace SignerBL.Handlers
{
    public class SenderHandler : ISender
    {
        private readonly string LINK_PLACEHOLDER = "[LINK]";
        private readonly string DOCUMENT_NAME_PLACEHOLDER = "[DOCUMENT_NAME]";
        private readonly string OTP_CODE_PLACEHOLDER = "[OTP_CODE]";

        private readonly ISendingMessageHandler _sendingMessageHandler;
        private readonly IGenerateLinkHandler _generateLinkHandler;
        private readonly IConfiguration _configuration;

        public SenderHandler(IConfiguration configuration, ISendingMessageHandler sendingMessageHandler, IGenerateLinkHandler generateLinkHandler)
        {
            _configuration = configuration;
            _generateLinkHandler = generateLinkHandler;
            _sendingMessageHandler = sendingMessageHandler;
        }

        public async Task SendSigningLinkToNextSigner(DocumentCollection dbDocumentCollection, Configuration appConfiguration, CompanyConfiguration companyConfiguration, Signer nextSigner)
        {
            string messageBefore = _configuration.GetBeforeMessage(dbDocumentCollection?.User, appConfiguration, companyConfiguration);
            var messageSender = _sendingMessageHandler.ExecuteCreation(nextSigner.SendingMethod);
            var links = await _generateLinkHandler.GenerateSigningLink(dbDocumentCollection, dbDocumentCollection?.User, companyConfiguration);
            string link = !string.IsNullOrWhiteSpace(links.FirstOrDefault(x => x.SignerId == nextSigner.Id)?.Link) ? links.FirstOrDefault(x => x.SignerId == nextSigner.Id)?.Link : "";
            messageBefore = UpdateMessage(nextSigner.SendingMethod, messageBefore, link, dbDocumentCollection.Name);


            var messageInfo = new MessageInfo()
            {
                MessageType = MessageType.BeforeSigning,
                Contact = nextSigner?.Contact,
                DocumentCollection = dbDocumentCollection,
                User = dbDocumentCollection.User,
                Link = link,
                MessageContent = messageBefore
            };
            await messageSender.Send(appConfiguration, companyConfiguration, messageInfo);


        }

        public Task SendEmailNotification(MessageType messageType, DocumentCollection dbDocumentCollection, Configuration appConfiguration, Signer dbSigner, CompanyConfiguration companyConfiguration)
        {
            var notifySender = _sendingMessageHandler.ExecuteCreation(SendingMethod.Email);
            MessageInfo notifyInfo;
            if (messageType == MessageType.SignerNoteNotification)
            {
                notifyInfo = new SignerNoteMessageInfo()
                {
                    MessageType = messageType,
                    Contact = dbSigner?.Contact,
                    User = dbDocumentCollection.User,
                    DocumentCollection = dbDocumentCollection,
                    Notes = dbSigner.Notes.SignerNote
                };
            }
            else
            {
                notifyInfo = new MessageInfo()
                {
                    MessageType = messageType,
                    Contact = dbSigner?.Contact,
                    User = dbDocumentCollection.User,
                    DocumentCollection = dbDocumentCollection
                };
            }
            return notifySender.Send(appConfiguration, companyConfiguration, notifyInfo);
        }

        public async Task<string> SendSignedDocument(DocumentCollection documentCollection, Configuration appConfiguration, Signer dbSigner, CompanyConfiguration companyConfiguration)
        {
            bool shouldSendSignedDocument = _configuration.ShouldSendSignedDocument(documentCollection.User, companyConfiguration, documentCollection?.Notifications);
            if (shouldSendSignedDocument)
            {
                string messageAfter = _configuration.GetAfterMessage(documentCollection?.User, appConfiguration, companyConfiguration);
                var messageSender = _sendingMessageHandler.ExecuteCreation(dbSigner.SendingMethod);
                var signerLink = await _generateLinkHandler.GenerateDocumentDownloadLink(documentCollection, dbSigner, documentCollection?.User, companyConfiguration);
                string link = !string.IsNullOrWhiteSpace(signerLink?.Link) ? signerLink.Link : "";
                messageAfter = UpdateMessage(dbSigner.SendingMethod, messageAfter, link, documentCollection.Name);

                var messageInfo = new MessageInfo()
                {
                    MessageType = MessageType.AfterSigning,
                    Contact = dbSigner.Contact,
                    DocumentCollection = documentCollection,
                    User = documentCollection.User,
                    Link = signerLink.Link,
                    MessageContent = messageAfter
                };
                if (dbSigner.SendingMethod != SendingMethod.Tablet)
                {
                    await messageSender.Send(appConfiguration, companyConfiguration, messageInfo);
                }
                return signerLink.Link;
            }

            return string.Empty;
        }



        public async Task<string> SendOtpCode(Configuration appConfiguration, Signer signer, User user, Company companyConfiguration)
        {
            SendingMethod sendingMethod = GetContactMeansForOTP(signer);

            var messageSender = _sendingMessageHandler.ExecuteCreation(sendingMethod);

            string otpMessage = _configuration.GetOtpMessgae(user, appConfiguration, null);

            string otpCode = signer?.SignerAuthentication?.OtpDetails?.Code;
            otpMessage = otpMessage.Replace(OTP_CODE_PLACEHOLDER, otpCode);

            var info = new MessageInfo()
            {
                MessageType = MessageType.OtpCode,
                Contact = signer?.Contact,
                MessageContent = otpMessage,
                User = user,
            };

            await messageSender.Send(appConfiguration, companyConfiguration.CompanyConfiguration, info);
            return sendingMethod == SendingMethod.SMS ? signer?.Contact?.Phone : signer?.Contact?.Email;
        }

        /// <summary>
        /// if contact has an email and a phone number- send the OTP to the second means <br/>
        /// For example - the document was sent to the signer via SMS and the contact has a valid email - send the OTP to the email and vice versa
        /// </summary>
        /// <param name="signer"></param>
        /// <returns></returns>
        private SendingMethod GetContactMeansForOTP(Signer signer)
        {
            SendingMethod sendingMethod = signer.SendingMethod;

            if (signer.Contact != null)
            {
                if (sendingMethod == SendingMethod.Email && !string.IsNullOrWhiteSpace(signer.Contact.Phone))
                {
                    sendingMethod = SendingMethod.SMS;
                }
                else if (!string.IsNullOrWhiteSpace(signer.Contact.Email))
                {
                    sendingMethod = SendingMethod.Email;
                }
            }

            return sendingMethod;
        }

        private string UpdateMessage(SendingMethod sendingMethod, string message, string signerLink, string documentCollectionName)
        {
            message = message.Replace(DOCUMENT_NAME_PLACEHOLDER, documentCollectionName);
            if (sendingMethod == SendingMethod.Email)
            {
                return message.Replace(LINK_PLACEHOLDER, "");
            }
            else if (sendingMethod == SendingMethod.SMS)
            {
                return message.Contains(LINK_PLACEHOLDER) ? message.Replace(LINK_PLACEHOLDER, signerLink) : $"{message} {signerLink}";
            }
            return string.Empty;
        }

        public Task SendDocumentDecline(DocumentCollection dbDcumentCollection, Configuration appConfiguration, Signer signer, User user)
        {

            var messageSender = _sendingMessageHandler.ExecuteCreation(SendingMethod.Email);

            var info = new MessageInfo()
            {
                MessageType = MessageType.Decline,
                Contact = signer?.Contact,
                DocumentCollection = dbDcumentCollection,
                User = dbDcumentCollection.User,
                MessageContent = signer?.Notes?.SignerNote
            };
            return messageSender.Send(appConfiguration, null, info);
        }
    }
}
