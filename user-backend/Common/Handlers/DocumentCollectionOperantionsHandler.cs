// Ignore Spelling: app

using Common.Interfaces.DB;
using Common.Interfaces;
using Common.Models;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Text;
using Common.Models.Configurations;
using Common.Enums;
using System.Linq;
using Common.Interfaces.MessageSending;
using Common.Enums.Documents;
using System.Threading.Tasks;

namespace Common.Handlers
{
    public class DocumentCollectionOperantionsHandler : IDocumentCollectionOperations
    {
        private readonly string LINK_PLACEHOLDER = "[LINK]";
        private readonly string DOCUMENT_NAME_PLACEHOLDER = "[DOCUMENT_NAME]";
     
        private readonly IConfiguration _configuration;        
        private readonly IGenerateLinkHandler _generateLinkHandler;
        private readonly ISendingMessageHandler _sendingMessageHandler;
        public DocumentCollectionOperantionsHandler( IConfiguration configuration, IGenerateLinkHandler generateLinkHandler, ISendingMessageHandler sendingMessageHandler)
        {
           
            _configuration = configuration;
            _generateLinkHandler = generateLinkHandler;
            _sendingMessageHandler = sendingMessageHandler;
        }

        public async Task<IEnumerable<SignerLink>> SendLinkToSpecificSigner(DocumentCollection documentCollection, Signer signer, User user, CompanyConfiguration companyConfiguration, bool sendDoc, MessageType messageType, bool shouldGenerateNewGuid = true)
        {           
            var appConfiguration = await _configuration.ReadAppConfiguration();
            int expirationTimeInHours = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);
            List<SignerLink> links = new List<SignerLink>
            {
                await _generateLinkHandler.GenerateSigningLinkToSingleSigner(documentCollection, shouldGenerateNewGuid, expirationTimeInHours, appConfiguration, signer)
            };
            if (sendDoc)
            {
                await SendDocumentLinkToSigner(documentCollection, links, user, companyConfiguration, appConfiguration, signer, messageType);
                
            }
            return links;
        }

        public Task SendDocumentLinkToSigner(DocumentCollection documentCollection, IEnumerable<SignerLink> links,
           User user, CompanyConfiguration companyConfiguration, Configuration appConfiguration, Signer signer, MessageType messageType)
        {
            string signerLink = links.FirstOrDefault(x => x.SignerId == signer.Id && x.Link.Contains("signature"))?.Link??"";
            string messageBefore = _configuration.GetBeforeMessage(user, appConfiguration, companyConfiguration);
            messageBefore = UpdateMessage(signer.SendingMethod, messageBefore, signerLink, documentCollection.Name);
            var messageSender = _sendingMessageHandler.ExecuteCreation(signer.SendingMethod);

            if (messageType == Enums.MessageType.SignReminder)
            {
                if (user?.UserConfiguration?.Language == Enums.Users.Language.en && !string.IsNullOrWhiteSpace(messageBefore))
                {
                    messageBefore = $"Reminder: {messageBefore}" ;
                }
                else if (user?.UserConfiguration?.Language == Enums.Users.Language.he && !string.IsNullOrWhiteSpace(messageBefore))
                {
                    messageBefore = $"תזכורת: {messageBefore}";
                }
            }

            var messageInfo = new MessageInfo()
            {
                MessageType = messageType,
                User = user,
                DocumentCollection = documentCollection,
                Contact = signer?.Contact,
                MessageContent = messageBefore,
                Link = signerLink
            };
            return  messageSender.Send(appConfiguration, companyConfiguration, messageInfo);   
        }

        public string UpdateMessage(SendingMethod sendingMethod, string message, string signerLink, string documentCollectionName)
        {
            message = message.Replace(DOCUMENT_NAME_PLACEHOLDER, documentCollectionName);
            if (sendingMethod == SendingMethod.Email)
            {
                return message.Replace(LINK_PLACEHOLDER, "");
            }
            if (sendingMethod == SendingMethod.SMS)
            {
                return message.Contains(LINK_PLACEHOLDER) ? message.Replace(LINK_PLACEHOLDER, signerLink) : $"{message} {signerLink}";
            }

            return string.Empty;
        }


    }

   
}
