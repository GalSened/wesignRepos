using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.MessageSending;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models;
using System;
using System.Threading.Tasks;
using Common.Interfaces.DB;
using Common.Enums.Results;
using Common.Extensions;
using System.Linq;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using System.Collections.Generic;

namespace BL.Handlers
{
    public class SignersHandler : ISigners
    {
        private readonly IUsers _users;
        private readonly IValidator _validator;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IDater _dater;
        private readonly IConfigurationConnector _configurationConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IConfiguration _configuration;
        private readonly IGenerateLinkHandler _generateLinkHandler;
        private readonly IDocumentCollectionOperations _documentCollectionOperations;
        private readonly ISendingMessageHandler _sendingMessageHandler;
        private readonly IProgramConnector _programConnector;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly IContacts _contacts;

        public SignersHandler(IUsers users, IValidator validator, IDocumentCollectionConnector documentCollectionConnector, IDater dater,
            IConfigurationConnector configurationConnector, ICompanyConnector companyConnector, IConfiguration configuration,
            IGenerateLinkHandler generateLinkHandler, IDocumentCollectionOperations documentCollectionOperations, ISendingMessageHandler sendingMessageHandler,
            IProgramConnector programConnector, IProgramUtilizationConnector programUtilizationConnector, IContacts contacts)
        {
            _users = users;
            _validator = validator;
            _documentCollectionConnector = documentCollectionConnector;
            _dater = dater;
            _configurationConnector = configurationConnector;
            _companyConnector = companyConnector;
            _configuration = configuration;
            _generateLinkHandler = generateLinkHandler;
            _documentCollectionOperations = documentCollectionOperations;
            _sendingMessageHandler = sendingMessageHandler;
            _programConnector = programConnector;
            _programUtilizationConnector = programUtilizationConnector;
            _contacts = contacts;
        }

        public async Task ReplaceSigner(Guid id, Guid signerId, string name, string means, Notes notes, OtpMode otpMode, string otpPassword, AuthMode authMode)
        {
            Contact contact = await _contacts.GetOrCreateContact(name, means);
            SendingMethod sendingMethod = ContactsExtenstions.IsValidPhone(means) ? SendingMethod.SMS : SendingMethod.Email;

            var signerAuth = new SignerAuthentication()
            {
                OtpDetails = new OtpDetails() { Mode = otpMode, Identification = otpPassword },
                AuthenticationMode = authMode
            };

            var documentCollection = new DocumentCollection()
            {
                Id = id,
                Signers = new List<Signer>() { new Signer() { SendingMethod = sendingMethod, Contact = contact, Notes = notes, SignerAuthentication = signerAuth } }
            };

            (User user, _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);

            if (dbDocumentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            if (!await IsDocumentCollectionBelongToUserGroup(dbDocumentCollection, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }

            Signer oldSigner = dbDocumentCollection.Signers?.FirstOrDefault(x => x.Id == signerId);

            if (oldSigner == null || oldSigner.Status == SignerStatus.Rejected || oldSigner.Status == SignerStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.InvalidSignerId.GetNumericString());
            }

            if (dbDocumentCollection.DocumentStatus == DocumentStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.DocumentAlreadySignedBySigner.GetNumericString());
            }

            if (signerAuth.AuthenticationMode != AuthMode.None && signerAuth.OtpDetails.Mode != OtpMode.None)
            {
                throw new InvalidOperationException(ResultCode.OtpAndFaceRecognitionSetup.GetNumericString());
            }

            bool needToSendDoc = IsDocNeedToBeSend(dbDocumentCollection, oldSigner);
            Signer newSigner = documentCollection.Signers.First();
            await UpdateFaceRecognition(oldSigner, newSigner, user);
            await UpdateSigner(user, dbDocumentCollection, oldSigner, newSigner, needToSendDoc);
            await _documentCollectionConnector.Update(dbDocumentCollection);
        }

        private async Task<bool> IsDocumentCollectionBelongToUserGroup(DocumentCollection documentCollection, bool readCollection)
        {
            (User user, _) = await _users.GetUser();
            if (readCollection)
            {
                documentCollection = await _documentCollectionConnector.Read(documentCollection);
            }

            return documentCollection != null &&
                  (user.GroupId != Guid.Empty && documentCollection.GroupId == user.GroupId) ||
                  (user.Id != Guid.Empty && documentCollection.UserId == user.Id && documentCollection.Mode == DocumentMode.SelfSign);
        }

        private bool IsDocNeedToBeSend(DocumentCollection dbDocumentCollection, Signer oldSigner)
        {
            if (dbDocumentCollection.Mode == DocumentMode.OrderedGroupSign)
            {
                int currentIndex = dbDocumentCollection.Signers.ToList().IndexOf(oldSigner);
                Signer prevSigner = dbDocumentCollection.Signers.ElementAtOrDefault(currentIndex - 1);
                if (prevSigner != null && prevSigner.Status != SignerStatus.Signed)
                {
                    return false;
                }
            }
            return true;
        }

        private async Task UpdateFaceRecognition(Signer oldSigner, Signer newSigner, User user)
        {
            // Add face recognition
            if (oldSigner.SignerAuthentication?.AuthenticationMode == AuthMode.None &&
                newSigner.SignerAuthentication?.AuthenticationMode != AuthMode.None)
            {
                await ValidateVisualIdentificationCapacity(user);
                await _programUtilizationConnector.AddVisualIdentification(user);
            }
            // Remove face recognition
            if (oldSigner.SignerAuthentication?.AuthenticationMode != AuthMode.None &&
                newSigner.SignerAuthentication?.AuthenticationMode == AuthMode.None)
            {
                await _programUtilizationConnector.RemoveVisualIdentification(user);
            }
        }

        private async Task ValidateVisualIdentificationCapacity(User user)
        {
            if (!await _programConnector.CanAddVisualIdentifications(user))
            {
                throw new InvalidOperationException(ResultCode.VisualIdentificationsExceedLicenseLimit.GetNumericString());
            }
        }

        private async Task<Signer> UpdateSigner(User user, DocumentCollection dbDocumentCollection, Signer oldSigner, Signer newSigner, bool needToSendDoc)
        {
            oldSigner.Contact = newSigner.Contact;

            if (needToSendDoc)
            {
                oldSigner.Status = SignerStatus.Sent;
                oldSigner.TimeSent = _dater.UtcNow();
                oldSigner.TimeLastSent = _dater.UtcNow();
            }

            oldSigner.TimeViewed = newSigner.TimeViewed;
            oldSigner.SendingMethod = newSigner.SendingMethod;

            if(oldSigner.Notes == null)
            {
                oldSigner.Notes = new Notes();
            }

            oldSigner.Notes.UserNote = newSigner.Notes?.UserNote;
            oldSigner.SignerAuthentication = newSigner.SignerAuthentication;
            oldSigner.IdentificationAttempts = 0;

            if(oldSigner.SignerAuthentication?.OtpDetails != null)
            {
                oldSigner.SignerAuthentication.OtpDetails.Attempts = 0;
            }

            if (needToSendDoc)
            {
                await SendDocumentToNewSigner(user, dbDocumentCollection, newSigner, oldSigner);
            }

            return oldSigner;
        }

        private async Task SendDocumentToNewSigner(User user, DocumentCollection documentCollection, Signer newSigner, Signer oldSigner)
        {
            Configuration appConfiguration = await _configurationConnector.Read();
            CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });
            int expirationTime = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);
            SignerLink signerLink = await _generateLinkHandler.GenerateSigningLinkToSingleSigner(documentCollection, true, expirationTime, appConfiguration, oldSigner);
            string messageBefore = _configuration.GetBeforeMessage(user, appConfiguration, companyConfiguration);
            messageBefore = _documentCollectionOperations.UpdateMessage(newSigner.SendingMethod, messageBefore, signerLink.Link, documentCollection.Name);
            IMessageSender messageSender = _sendingMessageHandler.ExecuteCreation(oldSigner.SendingMethod);
            MessageInfo messageInfo = new MessageInfo()
            {
                User = user,
                MessageType = MessageType.BeforeSigning,
                Link = signerLink?.Link,
                Contact = oldSigner.Contact,
                DocumentCollection = documentCollection,
                MessageContent = messageBefore
            };
            await messageSender.Send(appConfiguration, companyConfiguration, messageInfo);
        }
    }
}
