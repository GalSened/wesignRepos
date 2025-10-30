using Common.Consts;
using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers.SendingMessages;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.MessageSending;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Links;

using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace BL.Handlers
{
    public class LinksHandler : ILinks
    {
        
        private readonly IUsers _users;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IGenerateLinkHandler _generateLinkHandler;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IProgramConnector _programConnector;
        private readonly IVideoConfrence _videoConfrence;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly ISendingMessageHandler _sendingMessageHandler;
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IValidator _validator;
        private readonly ITemplateConnector _templateConnector;

        public LinksHandler(IDocumentCollectionConnector documentCollectionConnector, ICompanyConnector companyConnector,
            IUsers users, IGenerateLinkHandler generateLinkHandler,IProgramConnector programConnector, 
            IProgramUtilizationConnector programUtilizationConnector,
            ILogger logger, IConfiguration configuration, IVideoConfrence videoConfrence,
            ISendingMessageHandler sendingMessageHandler, IConfigurationConnector configurationConnector,
            IValidator validator, ITemplateConnector templateConnector)
        {
           
            _users = users;
            _documentCollectionConnector = documentCollectionConnector;
            _companyConnector = companyConnector;
            _generateLinkHandler = generateLinkHandler;
            _configuration = configuration;
            _logger = logger;
            _programConnector = programConnector;
            _videoConfrence = videoConfrence;
            _programUtilizationConnector = programUtilizationConnector;
            _sendingMessageHandler = sendingMessageHandler;
            _configurationConnector = configurationConnector;
            _validator = validator;
            _templateConnector = templateConnector;
        }


        public async Task UpdateCreateSingleLinkInfo(TemplateSingleLink templateSingleLink)
        {
            (User user, _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            var dbTemplate = await ReadAndValidateTempalteGroup(new Template
            {
                Id = templateSingleLink.TemplateId
            });

            await _templateConnector.UpdateCreateSingleLink(templateSingleLink);


        }

        public async Task<TemplateSingleLink> GetSingleLinkInfo(Template template)
        {

            var dbTemplate = await ReadAndValidateTempalteGroup(template);
            TemplateSingleLink templateSingleLink = new TemplateSingleLink
            {
                TemplateId = dbTemplate.Id
            };
            templateSingleLink.SingleLinkAdditionalResources = _templateConnector.ReadSingleLink(dbTemplate).ToList();

            return templateSingleLink;
        }

        public async Task<VideoConferenceResult> CreateVideoConference(CreateVideoConference createVideoConferences)
        {
            (User user, _) = await _users.GetUser();
            if(user.CompanyId == Consts.FREE_ACCOUNTS_COMPANY_ID)
            {
                throw new InvalidOperationException(ResultCode.FreeAccountsCannotCreateVideoConference.GetNumericString()); 
            }
            CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });
            if (!companyConfiguration.ShouldEnableVideoConference)
            {
                throw new InvalidOperationException(ResultCode.VideoConfrenceIsNotEnabled.GetNumericString());
            }
            if (!await _programConnector.CanAddVideoConference(user))
            {
                throw new InvalidOperationException(ResultCode.VideoConfrenceExceedLicenseLimit.GetNumericString());
            }
            ExternalVideoConfrenceResult result = await _videoConfrence.CreateVideoConference();

            await _programUtilizationConnector.AddVideoConfrence(user);

            await SendLinksToParticipants(createVideoConferences, result, user, companyConfiguration);

        
            VideoConferenceResult videoConferenceResult = new VideoConferenceResult()
            {
              ConferenceHostUrl = result.HostURL,
            };

            return videoConferenceResult;

        }

        private async Task<Template> ReadAndValidateTempalteGroup(Template template)
        {
            (User user, _) = await _users.GetUser();
            Template dbTemplate = await _templateConnector.Read(template);
            if (dbTemplate == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
            }
            if (user.GroupId != dbTemplate.GroupId)
            {
                throw new InvalidOperationException(ResultCode.TemplateNotBelongToUserGroup.GetNumericString());
            }

            return dbTemplate;
        }
        private async Task SendLinksToParticipants(CreateVideoConference createVideoConferences, ExternalVideoConfrenceResult externalVideoConfrenceResult, User user, CompanyConfiguration companyConfiguration)
        {

            Configuration appConfiguration = await _configurationConnector.Read();
            IMessageSender messageEmailSender = null;           
            List <MessageInfo> batchSMS = new List<MessageInfo>();
            foreach (var participant in createVideoConferences.VideoConferenceUsers)
            {

                MessageInfo messageInfo = new MessageInfo()
                {
                    User = user,
                    MessageType = MessageType.VideoConfrenceNotification,
                    Link = externalVideoConfrenceResult.ParticipantURL,
                    MessageContent = createVideoConferences.DocumentCollectionName
                };
                if (participant.SendingMethod == Common.Enums.Documents.SendingMethod.Email)
                {
                    if(messageEmailSender == null)
                    {
                        messageEmailSender = _sendingMessageHandler.ExecuteCreation(Common.Enums.Documents.SendingMethod.Email);
                    }
                    messageInfo.Contact = new Contact()
                    {
                        Email = participant.Means,
                        Name = participant.FullName
                    };
                    await messageEmailSender.Send(appConfiguration, companyConfiguration, messageInfo);

                }
                else
                {                    
                    messageInfo.MessageContent = _configuration.GetVideoConfrenceSmsMessage(user.UserConfiguration.Language);
                    messageInfo.MessageContent = messageInfo.MessageContent.Replace("[DOCUMENT_NAME]", createVideoConferences.DocumentCollectionName).
                        Replace("[LINK]", messageInfo.Link);
                    messageInfo.Contact = new Contact()
                    {
                        Phone = participant.Means,
                        Name = participant.FullName,
                        PhoneExtension = participant.PhoneExtension
                    };
                    batchSMS.Add(messageInfo);
                }
            }

            if (batchSMS.Count > 0)
            {
                IMessageSender messageSMSSender = _sendingMessageHandler.ExecuteCreation(Common.Enums.Documents.SendingMethod.SMS);
                await messageSMSSender.SendBatch(appConfiguration, companyConfiguration, batchSMS);
            }
                
        }



 
        public async Task<(IEnumerable<(DocumentCollection DocumentCollection, string SigningLink)>,int)> Read(string key, int offset, int limit)
        {
            
            (User user,  _) = await _users.GetUser();
            CompanyConfiguration companyConfiguration =await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });
            
            List<(DocumentCollection, string)> userDocumentsForSigning = new List<(DocumentCollection, string)>();
            IEnumerable<DocumentCollection> docs = _documentCollectionConnector.ReadBySignerEmail(user.Email, key, offset ,
                limit, out int totalCount);
           
            int expirationTimeInHours = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);
            Configuration appConfiguration =await _configuration.ReadAppConfiguration();
            
            foreach(DocumentCollection documentCollection in docs ?? Enumerable.Empty<DocumentCollection>())            
            {
                

                foreach (Signer signer in documentCollection?.Signers ?? new List<Signer>())
                {
                  
                    if (!string.IsNullOrWhiteSpace(signer?.Contact?.Email) && signer.Contact?.Email?.ToLower() == user.Email.ToLower()
                    && signer.SendingMethod == Common.Enums.Documents.SendingMethod.Email)
                    {
                        SignerLink signingLinks =await _generateLinkHandler.GenerateSigningLinkToSingleSigner(documentCollection, false, expirationTimeInHours, appConfiguration, signer);
                        userDocumentsForSigning.Add((documentCollection, signingLinks.Link));
                    }
                }
            }

            return (userDocumentsForSigning, totalCount);
        }




    }
}
