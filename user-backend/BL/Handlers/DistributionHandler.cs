using Common.Consts;
using Common.Enums.Documents;
using Common.Enums.PDF;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.MessageSending;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace BL.Handlers
{
    public class DistributionHandler : IDistribution
    {
        private readonly IUsers _users;
                
        private readonly IDater _dater;
        private readonly ITemplatePdf _templatePDFHandler;
        private readonly IDocumentPdf _documentPdf;
        private readonly IDocumentCollections _documentCollectionsHandler;
        private readonly IDataUriScheme _dataUriScheme;
        private readonly IValidator _validator;
        private readonly ILogger _logger;
        private readonly ISendingMessageHandler _sendingMessageHandler;
        private readonly IMemoryCache _memoryCache;
        private readonly string CONTACT_SECONDARY_MEANS = "signersecondarymeans";
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IProgramConnector _programConnector;
        private readonly ICompanyConnector _companyConnector;        
        private readonly ITemplateConnector _templateConnector;
        private readonly IContactConnector _contactConnector;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly IConfigurationConnector _configurationConnector;

        public DistributionHandler(IDocumentCollectionConnector documentCollectionConnector, ICompanyConnector companyConnector,
            ITemplateConnector templateConnector, IContactConnector contactConnector, IProgramUtilizationConnector programUtilizationConnector, 
            IConfigurationConnector configurationConnector, IProgramConnector programConnector,
            IUsers users, IDater dater,
            ITemplatePdf templatePDFHandler, IDocumentPdf documentPdf, IDocumentCollections documentCollections, 
            IDataUriScheme dataUriScheme, IValidator validator, ILogger logger,
            ISendingMessageHandler sendingMessageHandler, IMemoryCache memoryCache)
        {
            _documentCollectionConnector = documentCollectionConnector;
            _programConnector = programConnector;
            _companyConnector = companyConnector;
            
            _templateConnector = templateConnector;
            _contactConnector = contactConnector;
            _programUtilizationConnector = programUtilizationConnector;
            _configurationConnector = configurationConnector;
            _users = users;
                       
            _dater = dater;
            _templatePDFHandler = templatePDFHandler;
            _documentPdf = documentPdf;
            _documentCollectionsHandler = documentCollections;
            _dataUriScheme = dataUriScheme;
            _validator = validator;
            _logger = logger;
            _sendingMessageHandler = sendingMessageHandler;
            _memoryCache = memoryCache;
        }

        public async Task SendDocumentsUsingDistributionMechanism(IEnumerable<DocumentCollection> documentCollections)
        {
            DateTime StartDate = _dater.UtcNow();
            (User user, Template template) = await Validate(documentCollections);
            Guid templateId = template.Id;
            _templatePDFHandler.Load(templateId);
            string templateName = template?.Name;
            PDFFields fields = _templatePDFHandler.GetAllFields();
            UpdateTextFieldsType(fields.TextFields, template.Fields.TextFields);
            ValidateInputFieldsValue(fields.TextFields, documentCollections);
            byte[] bytes = _templatePDFHandler.Download();
            Guid distributionId = Guid.NewGuid();            
            _logger.Debug("SendDocumentsUsingDistributionMechanism - start process for distribution id [{DistributionId}] by user [{UserId}:{UserName}]", distributionId, user.Id, user.Name);
            CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });
            Configuration appConfiguration = await _configurationConnector.Read();
            int sendDocsCount = 0;
            int numberOfDocuments = 0;
            List<MessageInfo> batchSMS = new List<MessageInfo>();
            // remove signed fields 
            fields.SignatureFields = fields.SignatureFields.Where(x => string.IsNullOrWhiteSpace(x.Image)).ToList();
         

            for (int i = 0; i < documentCollections.Count(); i++)
            {
                var doc = documentCollections.ElementAt(i);
                try
                {
                    doc.UserId = user.Id;
                    doc.GroupId = user.GroupId;
                    doc.DistributionId = distributionId;
                    doc.CreationTime = _dater.UtcNow();
                    doc.DocumentStatus = DocumentStatus.Created;
                    doc.Mode = DocumentMode.GroupSign;
                    doc.ShouldEnableMeaningOfSignature = doc.ShouldEnableMeaningOfSignature && companyConfiguration.ShouldEnableMeaningOfSignatureOption;
                    doc.Name = !string.IsNullOrWhiteSpace(doc.Name) ? doc.Name : templateName;
                    bool isCreatedNew =await SetContactForDocument(doc);
                    
                    SetCallBackUrl(doc, companyConfiguration);
                    AssignAllFieldsToSingleSigner(doc, doc.Documents.First(), fields);
                    await _documentCollectionConnector.Create(doc);
                    if (doc.Id == default)
                    {
                        throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
                    }
                    _documentPdf.Create(doc.Documents.First().Id, bytes, true, shouldLoadToMemory: false);
                    _documentPdf.CreateImagesFromExternalList(_templatePDFHandler.Images);

                    bool shouldUpdateFieldsValue = false;
                    foreach (var signerField in doc.Signers.First().SignerFields)
                    {
                        if (!string.IsNullOrWhiteSpace(signerField.FieldValue))
                        {
                            shouldUpdateFieldsValue = true;
                            break;
                        }
                    }
                    if (shouldUpdateFieldsValue)
                    {
                        _documentCollectionsHandler.UpdateDocumentPdfFile(doc, Enumerable.Empty<SignerField>());
                    }
                    //?? NEED TO REMOVE???
                    await _documentCollectionsHandler.UpdateDb(doc, true);
                    if (!isCreatedNew)
                    {
                        await _contactConnector.UpdateLastUsed(doc.Signers.First().Contact);
                    }

                    _logger.Debug("SendDocumentsUsingDistributionMechanism - successfully create document collection [{DocId}: {DocName}] for distribution id [{DistributionId}] , document collection index [{DocIndex}] of [{DocAmount}]"
                    , doc.Id, doc.Name, distributionId, i + 1, documentCollections.Count());

                    if (doc.Signers.First().SendingMethod != SendingMethod.SMS)
                    {
                        await _documentCollectionsHandler.SendSignerLinks(doc);
                        sendDocsCount++;
                    }
                    else
                    {
                        batchSMS.Add(await _documentCollectionsHandler.BuildSignerLink(doc, companyConfiguration, appConfiguration));
                    }
                    numberOfDocuments++;


                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "SendDocumentsUsingDistributionMechanism - Failed to send document collection id [{DocId}] to signer [{Signer}]", doc.Id, doc.Signers.FirstOrDefault()?.Contact?.Name);
                }
            }
            try
            {
                if (batchSMS.Count > 0)
                {
                    var messageSender = _sendingMessageHandler.ExecuteCreation(SendingMethod.SMS);
                     await messageSender.SendBatch(appConfiguration, companyConfiguration, batchSMS);
                }

                
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SendDocumentsUsingDistributionMechanism - Failed to send batch of SMS");
            }


            DateTime EndDate = _dater.UtcNow();
            string processTime = (EndDate - StartDate).TotalMinutes >= 1 ? $"{(EndDate - StartDate).TotalMinutes} minutes" : $"{(EndDate - StartDate).TotalSeconds} seconds";
          
            _logger.Debug("SendDocumentsUsingDistributionMechanism - successfully send [{SentDocsCount}] documents in {ProcessTime}", sendDocsCount, processTime);
            await _programUtilizationConnector.AddDocument(user, numberOfDocuments);

            await _templateConnector.IncrementUseCount(new Template { Id = templateId }, numberOfDocuments);
        }


        private void SetCallBackUrl(DocumentCollection documentCollection, CompanyConfiguration documentCompanyConfiguration)
        {
            
            if (documentCompanyConfiguration.ShouldSendDocumentNotifications)
            {
                documentCollection.CallbackUrl = documentCompanyConfiguration.DocumentNotificationsEndpoint;
            }

        }
        private async Task<bool> SetContactForDocument(DocumentCollection doc)
        {
            Contact contact = doc.Signers.First().Contact;
            contact.UserId = doc.UserId;
            contact.GroupId = doc.GroupId;
            (var dbContact, bool isCreatedNew) = await GetOrCreateContact(contact);
            if (!string.IsNullOrWhiteSpace(contact.Email) && !string.IsNullOrWhiteSpace(contact.Phone) &&
               (contact.Email.ToLower().Trim() != dbContact.Email.ToLower().Trim() || contact.Phone.ToLower().Trim() != dbContact.Phone.ToLower().Trim()))
            {
                Contact contactByEmail = await _contactConnector.ReadByContactMeans(new Contact { Email = contact.Email,GroupId = contact.GroupId  });
                Contact contactByPhone = await _contactConnector.ReadByContactPhone(new Contact { Phone = contact.Phone, GroupId = contact.GroupId });

                if (contactByEmail == null || contactByPhone == null)
                {
                    if((contactByEmail == null && !string.IsNullOrWhiteSpace( dbContact.Email)) ||
                        contactByPhone == null && !string.IsNullOrWhiteSpace(dbContact.Phone))
                    {
                        
                        throw new InvalidOperationException(ResultCode.DuplicateContactData.GetNumericString());
                    }
                    dbContact.Email = contact.Email;
                    dbContact.Phone = contact.Phone;
                    await _contactConnector.Update(dbContact);
                }
                else
                {
                    throw new InvalidOperationException(ResultCode.DuplicateContactData.GetNumericString());
                }

            }

            doc.Signers.First().Contact = dbContact;


            return isCreatedNew;
        }

        public async Task<IEnumerable<BaseSigner>> ExtractSignersFromExcel(string base64File)
        {
            (User user, var _) = await _users.GetUser();

            _logger.Debug("Extract users from distribution excel file by user {UserId}: {UserName}", user.Id, user.Name);
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            var byteArray = _dataUriScheme.GetBytes(base64File);
            Stream stream = new MemoryStream(byteArray);
            Workbook workbook = new Workbook();
            workbook.LoadFromStream(stream);
            List<BaseSigner> result = new List<BaseSigner>();            
            Worksheet worksheet = workbook.Worksheets[0];
            var titles = worksheet.Rows[0];
            for (int j = 1; j < worksheet.Rows.Length; j++)
            {
                var signerDataFromExcel = worksheet.Rows[j];
                if (signerDataFromExcel.CellList.Count > 1)
                {
                    string signerMeans = signerDataFromExcel.CellList[2].DisplayedText.Trim();
                    if (ContactsExtenstions.IsValidPhone(signerMeans.Replace("-", "")))
                    {
                        signerMeans = signerMeans.Replace("-", "");
                    }
                    if (string.IsNullOrWhiteSpace(signerDataFromExcel.CellList[0].DisplayedText.Trim()) &&
                        string.IsNullOrWhiteSpace(signerDataFromExcel.CellList[1].DisplayedText.Trim()) &&
                        string.IsNullOrWhiteSpace(signerDataFromExcel.CellList[2].DisplayedText.Trim()) )
                    {
                        continue;
                    }
                    var signer = new BaseSigner
                    {
                        FullName = string.IsNullOrWhiteSpace(signerDataFromExcel.CellList[1].DisplayedText) ? signerDataFromExcel.CellList[0].DisplayedText.Trim() : $"{signerDataFromExcel.CellList[0].DisplayedText.Trim()} {signerDataFromExcel.CellList[1].DisplayedText.Trim()}",
                        SignerMeans = signerMeans
                    };
                    AddFieldsToContact(titles, signerDataFromExcel, signer);
                    if(result.FirstOrDefault(x=>x.SignerMeans == signer.SignerMeans) == null)
                    {
                        result.Add(signer);
                    }
                }
            }


            _logger.Debug("Extract users from distribution excel file by user {UserId}: {UserName} done found {Users} users in file", user.Id, user.Name, result.Count);
            return result;
        }
        
        public async Task<(IEnumerable<DocumentCollection>, int)> Read(string key, string from, string to, int offset, int limit)
        {
            
            (User user, var _) = await _users.GetUser();

            IEnumerable<DocumentCollection> documents = _documentCollectionConnector.ReadDistribution(user, key, offset, limit, out int totalCount);  
            return (documents, totalCount);
        }

        public async Task<(IEnumerable<DocumentCollection>, int)> Read(Guid distributionId, string key, string from, string to, int offset,
            int limit, Dictionary<DocumentStatus, int> statusCounts)
        {
             
            (User user, var _) = await _users.GetUser();
            
            IEnumerable<DocumentCollection> documents =  _documentCollectionConnector.ReadByDistributionId(user, distributionId,  offset,  limit , out int totalCount);
            if (statusCounts != null)
            {
                var cashedCounters = _memoryCache.Get<Dictionary<DocumentStatus, int>>($"{distributionId}_Counters");
                if (cashedCounters != null)
                {
                    statusCounts.Clear();
                    foreach (var pair in cashedCounters)
                    {
                        statusCounts.Add(pair.Key, pair.Value);
                    }
                }
                else
                {
                    await _documentCollectionConnector.ReadDistributionItemCounters(user, distributionId, statusCounts);
                    _memoryCache.Set($"{distributionId}_Counters", statusCounts, TimeSpan.FromSeconds(15));
                }
            }

            return (documents, totalCount);
        }

       

        public async Task DeleteAllDocuments(Guid distributionId)
        {
            (User user, var _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            _logger.Information("user {UserId}: {UserEmail} deleted all documents from distribution process id {DistributionId}", user.Id, user.Email, distributionId);
            await _documentCollectionConnector.UpdateStatusAllDistributionCollenttion(distributionId, DocumentStatus.Deleted);
            
        }

        public async Task ReSendUnSignedDocuments(Guid distributionId)
        {            
            (var documentCollections, int totalCount) = await Read(distributionId, "", null, null, 0, Consts.UNLIMITED, null ); 
            var unsignedDocs = documentCollections.Where(x => x.DocumentStatus == DocumentStatus.Sent ||
            x.DocumentStatus == DocumentStatus.Created || x.DocumentStatus == DocumentStatus.SendingFailed ||
            x.DocumentStatus == DocumentStatus.Viewed);
            await ResendDocs(unsignedDocs);
        }
        public async Task ReSendDocumentsInStatus(Guid distributionId, DocumentStatus status)
        {
            
            (var documentCollections, int totalCount) = await Read(distributionId, "", null, null, 0, Consts.UNLIMITED, null); 
            var unsignedDocs = documentCollections.Where(x => x.DocumentStatus == status);
            await ResendDocs(unsignedDocs);

        }

        private async Task ResendDocs(IEnumerable<DocumentCollection> unsignedDocs)
        {
            (User user, var _) = await _users.GetUser();
            if (!unsignedDocs.Any())
            {
                throw new InvalidOperationException(ResultCode.CannotResendSignedDocument.GetNumericString());
            }
            int count = unsignedDocs.Count(x => x.Signers.First().SendingMethod == SendingMethod.SMS);
            if (!await _programConnector.CanAddSms(user, count))
            {
                throw new InvalidOperationException(ResultCode.ProgramUtilizationGetToMax.GetNumericString());
            }
            foreach (var unsignedDoc in unsignedDocs)
            {
                await _documentCollectionsHandler.ResendDocument(unsignedDoc);
            }
        }

        public async Task<(List<(string SignerName, IDictionary<Guid, (string name, byte[] content)> Files)>, string)> DownloadSignedDocuments(Guid distributionId)
        {
            string documentName;
            (User user, var _) =await  _users.GetUser();
            (var documentCollections, int totalCount) = await Read(distributionId, "", null, null, 0, Consts.UNLIMITED, null); 
            documentName = documentCollections.FirstOrDefault()?.Name;
            var signedDocs = documentCollections.Where(x => x.DocumentStatus == DocumentStatus.Signed ||
                                x.DocumentStatus == DocumentStatus.ExtraServerSigned);
            if (!signedDocs.Any())
            {
                throw new InvalidOperationException(ResultCode.CannotDownloadUnsignedDocument.GetNumericString());
            }
            var documentCollectionsFiles = new List<(string SignerName, IDictionary<Guid, (string name, byte[] content)> Files)>();
            foreach (var signedDoc  in signedDocs)
            {
                var documentsFileData = await _documentCollectionsHandler.Download(signedDoc);

                if (documentsFileData.Count > 0)
                {
                    var pair = documentsFileData.FirstOrDefault();
                    Dictionary<Guid, (string name, byte[] content)> data = new Dictionary<Guid, (string name, byte[] content)>
                    {
                        { pair.Key, (pair.Value.name, pair.Value.content) }
                    };
                    documentCollectionsFiles.Add((signedDoc.Signers.FirstOrDefault().Contact?.Name, data));

                }
            }

            return (documentCollectionsFiles, documentName);
        }

        #region Private Functions

        private async Task<(User, Template)> Validate(IEnumerable<DocumentCollection> documentCollections)
        {
            Template template;
            (User user, var _) =  await _users.GetUser();

            if(documentCollections == null)
            {
                throw new ArgumentException("Null input", "documentCollections");
            }
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            if (!await _programConnector.CanAddDocument(user, documentCollections.Count()))
            {
                throw new InvalidOperationException(ResultCode.ProgramUtilizationGetToMax.GetNumericString());
            }
            int smsSignersCount = 0;
            documentCollections.ForEach(doc =>
            {
                if(doc.Signers.FirstOrDefault()?.SendingMethod == SendingMethod.SMS)
                {
                    smsSignersCount++;
                }
            });
            if (!await _programConnector.CanAddSms(user, smsSignersCount))
            {
                throw new InvalidOperationException(ResultCode.ProgramUtilizationGetToMax.GetNumericString());
            }
            template = await ValidateTemplateLogic(documentCollections, user);

            return (user, template);
        }

        private async Task<(Contact, bool)> GetOrCreateContact(Contact contact)
        {
            
            Contact dbContact = await  _contactConnector.Read(contact);
            
            if (dbContact == null)
            {
                contact.LastUsedTime = _dater.UtcNow();
                await _contactConnector.Create(contact);                
                return (contact, true);
            }
            return (dbContact, false);
        }

        private async Task<Template> ValidateTemplateLogic(IEnumerable<DocumentCollection> documentsCollections, User user)
        {
            if (!documentsCollections.Any())
            {
                throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
            }

            Template template = new Template() { Id = documentsCollections.FirstOrDefault().Documents.FirstOrDefault()?.TemplateId  ?? Guid.Empty};
            if (!await _templateConnector.Exists(template))
            {
                throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
            }
            template = await _templateConnector.Read(template);
            if (template.GroupId != user.GroupId)
            {
                throw new InvalidOperationException(ResultCode.TemplateNotBelongToUserGroup.GetNumericString());
            }

            return template;
        }

        private void AssignAllFieldsToSingleSigner(DocumentCollection documentCollection, Document document, PDFFields fields)
        {
            //TODO validate that all fields exist in template

            var singleSignerFields = documentCollection.Signers.FirstOrDefault().SignerFields;
            List<SignerField> signerFields = new List<SignerField>();

            fields.TextFields.ForEach(x =>
            {
                var textField = singleSignerFields.FirstOrDefault(y => y.FieldName.ToUpper() == x.Name.ToUpper() || y.FieldName.ToUpper() == x.Description.ToUpper());
                if (textField != null)
                {
                    signerFields.Add(new SignerField { TemplateId = document.TemplateId, FieldName = x.Name, FieldValue = textField.FieldValue });
                }
                else
                {
                    signerFields.Add(new SignerField { TemplateId = document.TemplateId, FieldName = x.Name, FieldValue = x.Value });
                }
            });
            fields.CheckBoxFields.ForEach(x => signerFields.Add(new SignerField { TemplateId = document.TemplateId, FieldName = x.Name }));
            fields.ChoiceFields.ForEach(x => signerFields.Add(new SignerField { TemplateId = document.TemplateId, FieldName = x.Name }));
            fields.RadioGroupFields.ForEach(x =>
            {
                x.RadioFields.ForEach(
                    y => {
                        signerFields.Add(new SignerField { TemplateId = document.TemplateId, FieldName = y.Name });
                    });
            }
            );
          
            fields.SignatureFields.ForEach(x => signerFields.Add(new SignerField { TemplateId = document.TemplateId, FieldName = x.Name }));
            documentCollection.Signers.FirstOrDefault().SignerFields = signerFields;
        }

        private  void AddFieldsToContact(CellRange titles, CellRange signerDataFromExcel, BaseSigner signer)
        {
            for (int i = 3; i < signerDataFromExcel.CellList.Count; i++)
            {
                string fieldValue = signerDataFromExcel.CellList[i].DisplayedText;
                if (!string.IsNullOrWhiteSpace(fieldValue))
                {
                    if (i == 3 && titles.CellList[i].DisplayedText.ToLower().Trim() == CONTACT_SECONDARY_MEANS)
                    {
                        string additionalMeans = fieldValue.Trim();
                        if (ContactsExtenstions.IsValidPhone(additionalMeans.Replace("-", "")))
                        {
                            additionalMeans = additionalMeans.Replace("-", "");
                        }
                        signer.SignerSecondaryMeans = additionalMeans;
                    }
                    else
                    {
                        signer.Fields.Add(new FieldNameToValuePair
                        {
                            FieldName = titles.CellList[i].DisplayedText,
                            FieldValue = fieldValue
                        });
                    }
                }
            }
        }

        private void ValidateInputFieldsValue(List<TextField> textFields, IEnumerable<DocumentCollection> documentCollections)
        {
            documentCollections.ForEach(x =>
            {
                string signerName = x.Signers.FirstOrDefault().Contact?.Name;
                foreach (var signerInputField in x.Signers.FirstOrDefault().SignerFields)
                {
                    // Fixed: Use case-insensitive comparison to match field names
                    var templateField = textFields.FirstOrDefault(y => 
                        y.Name.Equals(signerInputField.FieldName, StringComparison.OrdinalIgnoreCase) || 
                        y.Description.Equals(signerInputField.FieldName, StringComparison.OrdinalIgnoreCase));
                    if(templateField == null)
                    {
                        throw new InvalidOperationException(ResultCode.InvalidFieldNameNotExistInTemplate.GetNumericString(), new ArgumentException($"Missing Field Name {signerInputField.FieldName}", signerInputField.FieldName));
                    }


                    if (templateField?.TextFieldType == TextFieldType.Email)
                    {
                        bool isValid = ContactsExtenstions.IsValidEmail(signerInputField.FieldValue);
                        if (!isValid)
                        {
                            throw new InvalidOperationException(ResultCode.InvalidFieldValueAccordingToFieldType.GetNumericString(), new ArgumentException($"For signer name: {signerName}", signerInputField.FieldName));
                        }
                    }
                    if (templateField?.TextFieldType == TextFieldType.Phone)
                    {
                        bool isValid = ContactsExtenstions.IsValidPhone(signerInputField.FieldValue);
                        if (!isValid)
                        {
                            throw new InvalidOperationException(ResultCode.InvalidFieldValueAccordingToFieldType.GetNumericString(), new ArgumentException($"For signer name: {signerName}", signerInputField.FieldName));
                        }
                    }
                }
            });
        }

        private void UpdateTextFieldsType(List<TextField> textFieldsFromFile, List<TextField> templateTextFields)
        {
            foreach (var fieldFromFile in textFieldsFromFile)
            {
                var templateField = templateTextFields.FirstOrDefault(x => x.Name == fieldFromFile.Name);
                fieldFromFile.TextFieldType = templateField.TextFieldType;
            }
        }

        #endregion
    }

}

