using Common.Enums;
using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignerBL.Handlers
{
    public class SingleLinkHandler : ISingleLink
    {
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IUserConnector _userConnector;
        private readonly ITemplateConnector _templateConnector;
        private readonly IContactConnector _contactConnector;
        private readonly IGenerateLinkHandler _generateLinkHandler;
        private readonly IDocumentPdf _documentPdf;
        private readonly ITemplatePdf _templateHandler;
        private readonly IDater _dater;
        private readonly IConfiguration _configuration;

        public SingleLinkHandler(IDocumentCollectionConnector documentCollectionConnector, ICompanyConnector companyConnector, IUserConnector userConnector, 
            ITemplateConnector templateConnector, IContactConnector contactConnector, IGenerateLinkHandler generateLinkHandler, 
            IDocumentPdf documentPdf, IDater dater, ITemplatePdf templateHandler, IConfiguration configuration)
        {
            _documentCollectionConnector = documentCollectionConnector;
            _companyConnector = companyConnector;
            _userConnector = userConnector;
            _templateConnector = templateConnector;
            _contactConnector = contactConnector;
             _generateLinkHandler = generateLinkHandler;
            _documentPdf = documentPdf;
            _templateHandler = templateHandler;
            _dater = dater;
            _configuration = configuration;
        }

        public async Task<SignerLink> Create(SingleLink singleLink)
        {
            SignleLinkGetDataResult getDataResult = await GetData(singleLink);

            Contact tempContact = await GetOrCreateContact(singleLink, getDataResult.Template, getDataResult.IsSmsProviderSupportGloballySend);

            //Create document collection in the database
            var documentCollection = new DocumentCollection
            {
                Mode = DocumentMode.GroupSign,
                Name = getDataResult.Template.Name,
                Documents = GetDocumentFromTemplate(getDataResult.Template),
                Signers = GetSignerForSimpleDoc(getDataResult, tempContact),
                Notifications = new DocumentNotifications() { ShouldNotifyWhileSignerSigned = true, ShouldSendSignedDocument = true },
                UserId = getDataResult.Template.UserId,
                GroupId = getDataResult.Template.GroupId,
                CreationTime = _dater.UtcNow(),
                ShouldEnableMeaningOfSignature = false
            };

            await _documentCollectionConnector.Create(documentCollection);

            if (documentCollection.Id == default)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            //store in the file-system
            foreach (var document in documentCollection.Documents ?? Enumerable.Empty<Document>())
            {
                var bytes = _templateHandler.Download();
                _documentPdf.Create(document.Id, bytes, false);
                _documentPdf.CreateImagesFromExternalList(_templateHandler.Images);
            }

            //TODO: need to lock           
            await _contactConnector.UpdateLastUsed(tempContact);
            await Update(documentCollection);
            var links = await _generateLinkHandler.GenerateSigningLink(documentCollection, new User() { Id = getDataResult.Template.UserId }, null);

            return links.FirstOrDefault();
        }

        public async Task<SignleLinkGetDataResult> GetData(SingleLink singleLink)
        {
            SignleLinkGetDataResult result = new SignleLinkGetDataResult();
            result.Template = await _templateConnector.Read(new Template() { Id = singleLink.TemplateId });
            if (result.Template == null || result.Template.Id == default)
            {
                throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
            }

            var user = await _userConnector.Read(new User { Id = result.Template.UserId });
            if (user == null)
            {
                // the user that created the template deleted
                user = _userConnector.GetAllUsersInGroup(new Group { Id = result.Template.GroupId }).FirstOrDefault();
                if (user != null)
                {
                    result.Template.UserId = user.Id;
                }
            }

            result.SingleLinkAdditionalResources = _templateConnector.ReadSingleLink(result.Template);

            var appConfiguration = await _configuration.ReadAppConfiguration();
            var companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });
            var smsConfiguration = await _configuration.GetSmsConfiguration(user, appConfiguration, companyConfiguration);
            result.Language = user.UserConfiguration.Language;
            result.IsSmsProviderSupportGloballySend = smsConfiguration.IsProviderSupportGloballySend;
            return result;
        }

        #region Private Methods
        private List<SignerField> GetAllFiledsForSimpleDocument(Template template)
        {
            _templateHandler.Load(template.Id);
            var fields = new List<SignerField>();


            List<BaseField> fieldsInTemplate = new List<BaseField>(_templateHandler.SignatureFields);
            fieldsInTemplate.AddRange(_templateHandler.TextFields);
            fieldsInTemplate.AddRange(_templateHandler.CheckBoxFields);
            fieldsInTemplate.AddRange(_templateHandler.ChoiceFields);


            foreach (var signerField in fieldsInTemplate)
            {
                var field = new SignerField()
                {
                    TemplateId = template.Id,

                    FieldName = signerField.Name,
                };

                fields.Add(field);
            }

            foreach (var signerField in _templateHandler.RadioGroupFields)
            {
                signerField.RadioFields?.ForEach(radio =>
                {
                    var field = new SignerField()
                    {
                        TemplateId = template.Id,
                        FieldName = radio.Name,
                    };
                    fields.Add(field);
                });


            }

            return fields;
        }

        private IEnumerable<Signer> GetSignerForSimpleDoc(SignleLinkGetDataResult signleLinkGetDataResult, Contact selectedContact)
        {
            var template = signleLinkGetDataResult.Template;
            var signers = new List<Signer>();
            var fields = GetAllFiledsForSimpleDocument(template);
            var signer = new Signer()
            {
                Contact = selectedContact,
                SendingMethod = selectedContact.DefaultSendingMethod,
                SignerFields = fields,
                SignerAuthentication = new SignerAuthentication
                {
                    OtpDetails = new OtpDetails
                    {
                        Mode = OtpMode.CodeRequired
                    }
                }
            };
            AddSignerAttchments(signleLinkGetDataResult, signer);
            signers.Add(signer);
            return signers;
        }

        private IEnumerable<Document> GetDocumentFromTemplate(Template template)
        {
            var documents = new List<Document>
            {
                new Document() { TemplateId = template.Id }
            };

            return documents;
        }

        private async Task<Contact> GetOrCreateContact(SingleLink singleLink, Template template, bool isSmsProviderSupportGloballySend)
        {
            Contact tempContact = new Contact
            {
                UserId = template.UserId,
                GroupId = template.GroupId,
                Email = ContactsExtenstions.IsValidEmail(singleLink.Contact) ? singleLink.Contact : "",
                Phone = ContactsExtenstions.IsValidPhone(singleLink.Contact) ? singleLink.Contact : "",
                PhoneExtension = string.IsNullOrWhiteSpace(singleLink.PhoneExtension) ? "+972" : ContactsExtenstions.IsValidPhoneExtension(singleLink.PhoneExtension) ? singleLink.PhoneExtension : "",
                Name = singleLink.Fullname,
                LastUsedTime = _dater.UtcNow(),
                DefaultSendingMethod = ContactsExtenstions.IsValidEmail(singleLink.Contact) ? SendingMethod.Email : SendingMethod.SMS
            };

            if (!isSmsProviderSupportGloballySend && tempContact.DefaultSendingMethod == SendingMethod.SMS && tempContact.PhoneExtension != "+972")
            {
                throw new InvalidOperationException(ResultCode.SmsProviderNotSupportSendingSmsGlobally.GetNumericString());
            }

            Contact contact = await GetContact(tempContact);

            // create new if not exist
            if (contact == null)
            {
                await CreateContact(tempContact);
            }
            else
            {
                tempContact = contact;
                tempContact.DefaultSendingMethod = ContactsExtenstions.IsValidEmail(singleLink.Contact) ? SendingMethod.Email : SendingMethod.SMS;
            }

            if (tempContact == null || tempContact.Id == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.InvalidContactId.GetNumericString());
            }

            return tempContact;
        }

        private Task<Contact> GetContact(Contact contact) => _contactConnector.Read(contact);

        private Task CreateContact(Contact contact) => _contactConnector.Create(contact);

        private void UpdateDocumentPdfFile(DocumentCollection documentCollection)
        {
            foreach (var document in documentCollection?.Documents ?? Enumerable.Empty<Document>())
            {
                _documentPdf.Load(document.Id);

                var fields = _documentPdf.GetAllFields();

                foreach (var signer in documentCollection?.Signers ?? Enumerable.Empty<Signer>())
                {
                    SetValueToFields(document, fields, signer);
                }
                _documentPdf.TextFields.Update(fields.TextFields);
                _documentPdf.CheckBoxFields.Update(fields.CheckBoxFields);
                _documentPdf.ChoiceFields.Update(fields.ChoiceFields);
                _documentPdf.RadioGroupFields.Update(fields.RadioGroupFields);
                _documentPdf.SignatureFields.Update(fields.SignatureFields);

                _documentPdf.SaveDocument();

                // PATCH Hebrew issue with Debenu
                //    _documentPdf.EmbadTextDataFields(fields.TextFields);
            }
        }

        private  void AddSignerAttchments(SignleLinkGetDataResult signleLinkGetDataResult, Signer signer)
        {
            if (signleLinkGetDataResult.SingleLinkAdditionalResources.Any())
            {
                var signerAttachments = new List<SignerAttachment>();

                foreach (var signerLink in signleLinkGetDataResult.SingleLinkAdditionalResources.Where(x => x.Type == Common.Enums.Templates.SingleLinkAdditionalResourceType.Attachment))
                {

                    signerAttachments.Add(new()
                    {
                        Name = signerLink.Data,
                        IsMandatory = signerLink.IsMandatory,
                    });
                }
                if (signerAttachments.Any())
                {
                    signer.SignerAttachments = signerAttachments;
                }
            }
        }
        private void SetValueToFields(Document document, PDFFields fields, Signer signer)
        {
            var signerFields = signer.SignerFields.Where(x => x.TemplateId == document.TemplateId);

            foreach (var signerField in signerFields ?? Enumerable.Empty<SignerField>())
            {
                var textField = fields.TextFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower());

                if (textField != null && !string.IsNullOrWhiteSpace(signerField.FieldValue))
                {
                    textField.Value = signerField.FieldValue;
                    continue;
                }

                var choiceField = fields.ChoiceFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower());

                if (choiceField != null && !string.IsNullOrWhiteSpace(signerField.FieldValue))
                {
                    choiceField.SelectedOption = signerField.FieldValue;
                    continue;
                }

                var checkBoxField = fields.CheckBoxFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower());

                if (checkBoxField != null && !string.IsNullOrWhiteSpace(signerField.FieldValue))
                {
                    bool.TryParse(signerField.FieldValue, out bool result);
                    checkBoxField.IsChecked = result;
                    continue;
                }

                var radioGroupField = fields.RadioGroupFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower());

                if (radioGroupField != null && !string.IsNullOrWhiteSpace(signerField.FieldValue))
                {
                    radioGroupField.SelectedRadioName = signerField.FieldValue;
                }
            }
        }

        private Task Update(DocumentCollection documentCollection)
        {
            UpdateDocumentPdfFile(documentCollection);
            return UpdateDb(documentCollection);
        }

        private async Task UpdateDb(DocumentCollection documentCollection)
        {
            foreach (var signer in documentCollection?.Signers ?? Enumerable.Empty<Signer>())
            {
                foreach (var signerField in signer?.SignerFields ?? Enumerable.Empty<SignerField>())
                {
                    var document = documentCollection.Documents?.FirstOrDefault(d => d.TemplateId == signerField?.TemplateId);
                    if (document != null)
                    {
                        signerField.DocumentId = document.Id;
                    }
                }
                signer.TimeSent = _dater.UtcNow();
            }

            await _documentCollectionConnector.Update(documentCollection);

            foreach (var document in documentCollection?.Documents ?? Enumerable.Empty<Document>())
            {
                var template = new Template() { Id = document.TemplateId };
                await _templateConnector.IncrementUseCount(template, 1);
            }
        }

        #endregion
    }
}