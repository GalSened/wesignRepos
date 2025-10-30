using Common.Consts;
using Common.Enums;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.PDF;
using Common.Enums.Results;
using Common.Extensions;
using Common.Hubs;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Oauth;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Documents.SplitSignature;
using Common.Models.Files.PDF;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto;
using PdfHandler.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Threading.Tasks;
using Document = Common.Models.Documents.Document;

namespace BL.Handlers
{
    public class SelfSignHandler : ISelfSign
    {

        private readonly IValidator _validator;

        private readonly IDocumentPdf _documentPdf;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly ISignerTokenMappingConnector _signerTokenMappingConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IContactConnector _contactConnector;
        private readonly ITemplateConnector _templateConnector;
        private readonly IProgramConnector _programConnector;
        private readonly ICertificate _certificate;

        private readonly IUsers _users;
        private readonly IDataUriScheme _dataUriScheme;
        private readonly IDater _dater;
        private readonly ITemplatePdf _templatePdf;
        private readonly IJWT _jwt;
        private readonly JwtSettings _jwtSettings;

        private readonly IDocumentCollections _documentCollections;
        private readonly ITemplates _templates;
        private readonly IPdfConverter _pdfConverter;
        private readonly IEncryptor _encryptor;
        private readonly ISignConnector _signConnector;
        private readonly ILogger _logger;
        private readonly ISmartCardSigningProcess _smartCardSigningProcess;
        private readonly GeneralSettings _generalSettings;
        private readonly IOauth _oauth;

        public SelfSignHandler(IValidator validator, IDocumentPdf documentPdfHandler, IDocumentCollectionConnector documentCollectionConnector,
            ICompanyConnector companyConnector, ITemplateConnector templateConnector, IProgramConnector programConnector, IContactConnector contactConnector,
            IProgramUtilizationConnector programUtilizationConnector, ISignerTokenMappingConnector signerTokenMappingConnector,
            ICertificate certificate,
            IOptions<JwtSettings> jwtSettings, IUsers users, IDataUriScheme dataUriScheme,
            IDater dater, ITemplatePdf templatePdf, IJWT jwt, IDocumentCollections documentCollections,
            IEncryptor encryptor, ILogger logger,
            ITemplates templates, IPdfConverter pdfConverter,
            ISignConnector signConnector,
            IOptions<GeneralSettings> generalSettings,
            IOauth oauth,
             ISmartCardSigningProcess smartCardSigningProcess)
        {
            _validator = validator;
            _documentPdf = documentPdfHandler;
            _documentCollectionConnector = documentCollectionConnector;
            _programUtilizationConnector = programUtilizationConnector;
            _signerTokenMappingConnector = signerTokenMappingConnector;
            _companyConnector = companyConnector;
            _contactConnector = contactConnector;
            _templateConnector = templateConnector;
            _programConnector = programConnector;
            _certificate = certificate;
            _users = users;
            _dataUriScheme = dataUriScheme;
            _dater = dater;
            _templatePdf = templatePdf;
            _jwt = jwt;
            _jwtSettings = jwtSettings.Value;
            _documentCollections = documentCollections;
            _templates = templates;
            _pdfConverter = pdfConverter;
            _encryptor = encryptor;
            _signConnector = signConnector;
            _logger = logger;
            _smartCardSigningProcess = smartCardSigningProcess;
            _generalSettings = generalSettings.Value;
            _oauth = oauth;
        }
        public async Task<Guid> CreateSmartCardSigningFlow(SmartCardSigningFlow smartCardSigningFlow)
        {
            (User user, _) = await _users.GetUser();

            DocumentCollection documentCollection = await _documentCollectionConnector.Read(new DocumentCollection { Id = smartCardSigningFlow.Token });

            if (!await IsDocumentCollectionBelongToUser(documentCollection) || documentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            // validate all fields are exist in the document 
            // all fields are signed????
            // Create in memory all the data
            // notify to remote server if needed

            return Guid.NewGuid();
        }

        public Task<SplitDocumentProcess> ProcessAfterSignerAuth(IdentityFlow identityFlow)
        {
            string callBackUrl = $"{_generalSettings.UserFronendApplicationRoute}/oauth";
            return _oauth.ProcessAfterSignerAuth(identityFlow, callBackUrl);
        }

        public async Task<DocumentCollection> Create(Template selfSignTemplate, string remoteIpAddress)
        {

            bool selfSignFromExistingTemplate = false;
            Guid origTemplateId = Guid.Empty;
            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }

            bool isValidWord = _dataUriScheme.IsOctetStreamIsValidWord(selfSignTemplate.Base64File, out FileType fileType);
            if (!isValidWord && !_dataUriScheme.IsValidFileType(selfSignTemplate.Base64File, out fileType))
            {
                throw new InvalidOperationException(ResultCode.InvalidFileType.GetNumericString());
            }
            selfSignTemplate.Base64File = (await _validator.ValidateIsCleanFile(selfSignTemplate.Base64File))?.CleanFile;

            if (user.CompanyId == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
            }

            Company company = await _companyConnector.Read(new Company() { Id = user.CompanyId });
            _certificate.Create(user, company.CompanyConfiguration);

            if (Guid.Empty != selfSignTemplate.Id)
            {

                Template dbTemplate = await _templateConnector.Read(new Template() { Id = selfSignTemplate.Id });
                if (!await IsTemplateBelongToUserGroup(dbTemplate))
                {
                    throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
                }

                await AddFieldsDataFromTheOrigPDF(selfSignTemplate);
                selfSignFromExistingTemplate = true;
                origTemplateId = dbTemplate.Id;

            }
            selfSignTemplate.FileType = fileType;
            selfSignTemplate.UserId = user.Id;
            selfSignTemplate.GroupId = user.GroupId;
            selfSignTemplate.CreationTime = selfSignTemplate.LastUpdatetime = selfSignTemplate.LastUsedTime = _dater.UtcNow();

            await _templateConnector.Create(selfSignTemplate);
            Contact contact = await GetOrCreateUserAsContact(user);
            DocumentCollection selfSignDocument = new DocumentCollection()
            {
                CreationTime = _dater.UtcNow(),
                Mode = DocumentMode.SelfSign,
                Name = selfSignTemplate.Name,
                UserId = user.Id,
                GroupId = user.GroupId,
                ShouldEnableMeaningOfSignature = false,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        Name = selfSignTemplate.Name,
                        TemplateId = selfSignTemplate.Id
                    }
                },
                Signers = new List<Signer> { new Signer { Status = SignerStatus.Viewed, Contact = contact, FirstViewIPAddress = remoteIpAddress, IPAddress = remoteIpAddress } },
                SenderIP = remoteIpAddress
            };
            await _documentCollectionConnector.Create(selfSignDocument);

            string base64content = selfSignTemplate.FileType != FileType.PDF ?
                   ConvertToPdf(selfSignTemplate.Base64File)
                   : _dataUriScheme.Getbase64Content(selfSignTemplate.Base64File);
            if (!string.IsNullOrEmpty(selfSignTemplate.MetaData))
            {

            }

            _documentPdf.Create(selfSignDocument.Documents.First().Id, Convert.FromBase64String(base64content), selfSignFromExistingTemplate);

            if (selfSignFromExistingTemplate)
            {
                _documentPdf.CopyPagesImagesFromTemplate(origTemplateId);
                await _templateConnector.IncrementUseCount(new Template() { Id = origTemplateId }, 1);
            }
            else
            {
                _documentPdf.CreateImages();
            }
            await _contactConnector.UpdateLastUsed(contact);


            selfSignDocument.Documents.First().PagesCount = _documentPdf.GetPagesCount();

            return selfSignDocument;

        }

        private async Task<bool> IsTemplateBelongToUserGroup(Template dbTemplate)
        {
            (User user, _) = await _users.GetUser();
            return dbTemplate?.GroupId == user.GroupId;
        }

        private async Task AddFieldsDataFromTheOrigPDF(Template selfSignTemplate)
        {
            Template templatedb = await _templateConnector.Read(selfSignTemplate);
            if (templatedb != null)
            {
                selfSignTemplate.Fields = templatedb.Fields;
            }
            selfSignTemplate.Id = Guid.Empty;
        }

        public async Task<SelfSignUpdateDocumentResult> Update(DocumentCollection documentCollection, DocumentOperation operation, bool useForAllFields = false)
        {
            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (!await IsDocumentCollectionBelongToUser(documentCollection) || dbDocumentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            if (!await IsDocumentBelongToDocumentCollection(documentCollection))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToDocumentCollection.GetNumericString());
            }

            dbDocumentCollection.Documents.First().Fields = documentCollection.Documents.First().Fields;
            await _documentCollectionConnector.Update(dbDocumentCollection);
            Document document = documentCollection.Documents.FirstOrDefault();
            UpdateDocumentPdfFields(document, dbDocumentCollection);
            await UpdateTemplateDbFields(dbDocumentCollection, document);

            if (operation == DocumentOperation.Close)
            {
                if (document.Fields.SignatureFields.Exists(x => x.SigningType == SignatureFieldType.SmartCard))
                {

                    return await GetSignerTokenForSmartCardSigningFlow(dbDocumentCollection, documentCollection);
                }

                if (document.Fields.SignatureFields.Exists(x => x.SigningType == SignatureFieldType.Server) && _generalSettings.ComsignIDPActive)
                {

                    return await GetInfoForEidasSigningFlow(dbDocumentCollection, documentCollection);
                }
                (User user, _) = await _users.GetUser();
                Company userCompany = await _companyConnector.Read(new Company() { Id = user.CompanyId });

                SigningInfo signingInfo = GetSigningInfo(documentCollection, user, userCompany);

                await _documentPdf.Sign(signingInfo,false, useForAllFields);

                await UpdateDocumentCollectionStatus(documentCollection);
                await _programUtilizationConnector.AddDocument(user);
            }

            return new SelfSignUpdateDocumentResult();
        }

        //public async Task<SelfSignUpdateDocumentResult> UpdateGovDocument(DocumentCollection documentCollection)
        //{
        //    DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);
        //    if (!await IsDocumentCollectionBelongToUser(documentCollection) || dbDocumentCollection == null)
        //    {
        //        throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
        //    }
        //    if (!await IsDocumentBelongToDocumentCollection(documentCollection))
        //    {
        //        throw new InvalidOperationException(ResultCode.DocumentNotBelongToDocumentCollection.GetNumericString());
        //    }

        //    dbDocumentCollection.Documents.First().Fields = documentCollection.Documents.First().Fields;
        //    await _documentCollectionConnector.Update(dbDocumentCollection);

        //    return await GetSignerTokenForSmartCardSigningFlow(dbDocumentCollection, documentCollection, true);
        //}

        private SigningInfo GetSigningInfo(DocumentCollection documentCollection, User user, Company userCompany)
        {
            Document document = documentCollection.Documents.FirstOrDefault();

            SigningInfo signingInfo = new SigningInfo
            {
                Reason = user?.Name ?? "",
                SignerAuthentication = documentCollection.Signers.FirstOrDefault()?.SignerAuthentication,
                Certificate = _certificate.Get(user, userCompany.CompanyConfiguration),
                Signatures = document.Fields.SignatureFields,
                CompanySigner1Details = userCompany.CompanySigner1Details != null ? userCompany.CompanySigner1Details : new CompanySigner1Details()
            };

            if (userCompany?.CompanySigner1Details?.Signer1Configuration != null && !string.IsNullOrWhiteSpace(userCompany.CompanySigner1Details.Signer1Configuration.Endpoint))
            {
                signingInfo.SignerAuthentication.Signer1Configuration = userCompany.CompanySigner1Details.Signer1Configuration;
            }

            return signingInfo;
        }

        public async Task Delete(DocumentCollection documentCollection)
        {

            (User user, _) = await _users.GetUser();
            //TODO add rollback / transaction
            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (dbDocumentCollection.GroupId != user.GroupId && documentCollection.UserId != user.Id)
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }

            foreach (Document document in dbDocumentCollection.Documents ?? Enumerable.Empty<Document>())
            {
                await _templates.Delete(new Template { Id = document.TemplateId });
            }




            _logger.Information("user {UserId}: {UserEmail} deleted self sign document {DbDocumentCollectionId} named {DbDocumentCollectionName}", user.Id, user.Email, documentCollection.Id, documentCollection.Name);

            await _documentCollections.Delete(dbDocumentCollection);
        }

        public async Task VerifySigner1Credential(SignerAuthentication input)
        {
            (User _, CompanySigner1Details companySigner1Details) = await _users.GetUser();
            await _documentPdf.VerifySigner1Credential(input, companySigner1Details);
        }

        public async Task<Signer1FileSigingResult> SignFileUsingSigner1(Signer1FileSiging signer1FileSiging)
        {

            string certId = signer1FileSiging.Signer1Credential?.CertificateId;
            string token = signer1FileSiging.Signer1Credential?.SignerToken;
            signer1FileSiging.Base64File = (await _validator.ValidateIsCleanFile(signer1FileSiging.Base64File))?.CleanFile;

            if (!string.IsNullOrWhiteSpace(token) &&
               (string.IsNullOrWhiteSpace(signer1FileSiging.Signer1Credential.Password) && string.IsNullOrWhiteSpace(certId)))
            {
                string deccryptData = _encryptor.Decrypt(signer1FileSiging.Signer1Credential?.SignerToken ?? "");
                string[] splitString = deccryptData.Split(new string[] { $"{Consts.SAML_SEPARATOR}" }, StringSplitOptions.RemoveEmptyEntries);

                // fix for MAMAZ ...
                if (splitString.Length > 1 && !string.IsNullOrWhiteSpace(splitString[0]))
                {
                    certId = splitString[0];
                    token = splitString[1];
                }
                else if (splitString.Length == 1 && !string.IsNullOrWhiteSpace(splitString[0]))
                {
                    certId = signer1FileSiging.Signer1Credential.CertificateId = splitString[0];
                }
            }
            (_, CompanySigner1Details _companySigner1Details) = await _users.GetUser();

            (Signer1ResCode ResultCode, byte[] SignedBytes) apiResult;
            switch (signer1FileSiging.SigingFileType)
            {
                case SigningFileType.EXEL:
                    {
                        apiResult = await _signConnector.SignExcel(certId, Convert.FromBase64String(signer1FileSiging.Base64File),
                            signer1FileSiging.Signer1Credential?.Password, token, _companySigner1Details);
                        break;
                    }
                case SigningFileType.WORD:
                    {
                        apiResult = await _signConnector.SignWord(certId, Convert.FromBase64String(signer1FileSiging.Base64File),
                           signer1FileSiging.Signer1Credential?.Password, token, _companySigner1Details);
                        break;
                    }
                case SigningFileType.XML:
                    {
                        apiResult = await _signConnector.SignXML(certId, Convert.FromBase64String(signer1FileSiging.Base64File),
                           signer1FileSiging.Signer1Credential?.Password, token, _companySigner1Details);
                        break;
                    }
                case SigningFileType.PDF:
                    {
                        apiResult = await _signConnector.SignPdf(certId, Convert.FromBase64String(signer1FileSiging.Base64File),
                           signer1FileSiging.Signer1Credential?.Password, token, _companySigner1Details);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException(ResultCode.UnsupportedFileTypeForSigning.GetNumericString());
                    }
            }

            if (apiResult.ResultCode != Signer1ResCode.SUCCESS)
            {
                throw new InvalidOperationException(ResultCode.SignOperationFailed.GetNumericString(),
                                                            new Exception($"Sign using signer1 failed - {apiResult.ResultCode})"));
            }

            Signer1FileSigingResult result = new Signer1FileSigingResult
            {
                Base64SignedFile = apiResult.SignedBytes,
                SigingFileType = signer1FileSiging.SigingFileType
            };

            return result;
        }


        #region Private Functions


        private void ValidateMandatoryFields(PDFFields InputsFields)
        {
            PDFFields documentFields = _documentPdf.GetAllFields(false);

            foreach (TextField field in documentFields.TextFields ?? Enumerable.Empty<TextField>())
            {
                if (field.Mandatory)
                {
                    TextField textField = InputsFields.TextFields.FirstOrDefault(x => x.Name == field.Name || x.Description == field.Description);
                    if (textField == null || string.IsNullOrWhiteSpace(textField.Value))
                    {
                        throw new InvalidOperationException(ResultCode.NotAllMandatoryFieldsFilledIn.GetNumericString());
                    }
                }
            }
            foreach (CheckBoxField field in documentFields.CheckBoxFields ?? Enumerable.Empty<CheckBoxField>())
            {
                if (field.Mandatory)
                {
                    CheckBoxField inputcheckBoxField = InputsFields.CheckBoxFields.FirstOrDefault(x => x.Name == field.Name || x.Description == field.Description);
                    if (inputcheckBoxField == null || !inputcheckBoxField.IsChecked)
                    {
                        throw new InvalidOperationException(ResultCode.NotAllMandatoryFieldsFilledIn.GetNumericString());
                    }
                }
            }
            foreach (ChoiceField field in documentFields.ChoiceFields ?? Enumerable.Empty<ChoiceField>())
            {
                if (field.Mandatory)
                {
                    ChoiceField inputChoiceBoxField = InputsFields.ChoiceFields.FirstOrDefault(x => x.Name == field.Name || x.Description == field.Description);
                    if (inputChoiceBoxField == null || string.IsNullOrWhiteSpace(inputChoiceBoxField.SelectedOption))
                    {
                        throw new InvalidOperationException(ResultCode.NotAllMandatoryFieldsFilledIn.GetNumericString());
                    }
                }
            }

            foreach (RadioGroupField field in documentFields.RadioGroupFields ?? Enumerable.Empty<RadioGroupField>())
            {

                if (field.RadioFields != null && field.RadioFields.Any() && field.RadioFields[0].Mandatory)
                {
                    RadioGroupField radioGroupField = InputsFields.RadioGroupFields.FirstOrDefault(x => x.Name == field.Name);

                    if (radioGroupField == null || string.IsNullOrWhiteSpace(radioGroupField.SelectedRadioName))
                    {
                        throw new InvalidOperationException(ResultCode.NotAllMandatoryFieldsFilledIn.GetNumericString());
                    }
                }
            }

            foreach (SignatureField field in documentFields.SignatureFields ?? Enumerable.Empty<SignatureField>())
            {
                if (field.Mandatory)
                {
                    SignatureField signatureField = InputsFields.SignatureFields.FirstOrDefault(x => x.Name == field.Name);

                    if (signatureField == null || string.IsNullOrWhiteSpace(signatureField.Image))
                    {
                        throw new InvalidOperationException(ResultCode.NotAllMandatoryFieldsFilledIn.GetNumericString());
                    }
                }
            }
        }


        private void UpdateDocumentPdfFields(Document document, DocumentCollection dbDocCollection)
        {
            _documentPdf.Load(document.Id);

            ValidateMandatoryFields(document.Fields);

            IEnumerable<TextField> dateFields = _templateConnector.GetTextFieldsByType(new Template { Id = dbDocCollection?.Documents?.FirstOrDefault(x => x.Id == document.Id)?.TemplateId ?? Guid.Empty }, TextFieldType.Date);
            if (dateFields.Any())
            {
                document.Fields?.TextFields?.ForEach(field =>
                {


                    //More correct way is to load textfieldType and compare to it if it is date type
                    if (dateFields.FirstOrDefault(x => x.Name == field.Name) != null &&
                        DateTime.TryParse(field.Value, out DateTime dateTime))
                    {
                        field.Value = dateTime.ToString("dd MMM yyyy");
                    }


                });
            }
            _documentPdf.DoUpdateFields(document.Fields);
            _documentPdf.SetAllFieldsToReadOnly();
            _documentPdf.SaveDocument();
        }

        private async Task UpdateTemplateDbFields(DocumentCollection dbDocumentCollection, Document document)
        {
            Document dbDocument = dbDocumentCollection.Documents.FirstOrDefault(x => x.Id == document.Id);
            Template template = await _templateConnector.Read(new Template { Id = dbDocument.TemplateId });
            template.Fields = document.Fields;
            await _templateConnector.Update(template);
        }

        private async Task UpdateDocumentCollectionStatus(DocumentCollection documentCollection)
        {
            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);
            dbDocumentCollection.DocumentStatus = DocumentStatus.Signed;
            dbDocumentCollection.SignedTime = _dater.UtcNow();
            if (dbDocumentCollection?.Signers?.FirstOrDefault() != null)
            {
                dbDocumentCollection.Signers.FirstOrDefault().IPAddress = documentCollection.Signers?.FirstOrDefault()?.IPAddress ?? "";
            }

            await _documentCollectionConnector.Update(dbDocumentCollection);
        }

        private string ConvertToPdf(string base64file)
        {
            //ImageType
            if (_dataUriScheme.IsValidImageType(base64file, out _))
            {
                return _templatePdf.ConvertImageToPdf(_dataUriScheme.Getbase64Content(base64file));
            }


            string content = _dataUriScheme.Getbase64Content(base64file);
            string pdfBase64string = _pdfConverter.Convert(content, base64file?.Split(new char[] { ',' })?.FirstOrDefault());
            return pdfBase64string;

        }

        private async Task<bool> IsDocumentCollectionBelongToUser(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();
            documentCollection = await _documentCollectionConnector.Read(documentCollection);

            return documentCollection != null && documentCollection.UserId == user.Id;
        }

        private async Task<bool> IsDocumentBelongToDocumentCollection(DocumentCollection documentCollection)
        {
            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);

            return dbDocumentCollection.Documents.FirstOrDefault().Id == documentCollection.Documents.FirstOrDefault().Id;
        }

        private async Task<Contact> GetOrCreateUserAsContact(User user)
        {
            Contact contact = await _contactConnector.Read(new Contact { Email = user.Email, GroupId = user.GroupId });
            if (contact == null)
            {
                contact = new Contact
                {
                    Name = user.Name,
                    GroupId = user.GroupId,
                    Email = user.Email,
                    DefaultSendingMethod = SendingMethod.Email
                    ,
                    UserId = user.Id,
                    LastUsedTime = _dater.UtcNow()
                };
                await _contactConnector.Create(contact);
            }

            return contact;
        }

        private async Task<SelfSignUpdateDocumentResult> GetInfoForEidasSigningFlow(DocumentCollection dbDocumentCollection,
            DocumentCollection inputDocumentCollection)
        {
            Signer signer = dbDocumentCollection.Signers.FirstOrDefault();
            inputDocumentCollection.Name = dbDocumentCollection.Name;
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping()
            {
                DocumentCollectionId = dbDocumentCollection.Id,
                SignerId = signer.Id,
                GuidToken = Guid.NewGuid(),
                JwtToken = _jwt.GenerateSignerToken(signer, _jwtSettings.SignerLinkExpirationInHours)
            };
            await _signerTokenMappingConnector.Create(signerTokenMapping);

            _oauth.SaveDataForEidasProcess(signerTokenMapping, inputDocumentCollection);
            string callBackUrl = $"{_generalSettings.UserFronendApplicationRoute}/oauth";
            return new SelfSignUpdateDocumentResult
            {
                RedirectUrl = _oauth.GetURLForStartAuthForEIdasFlow(signerTokenMapping, callBackUrl)
            };
        }

        private async Task<SelfSignUpdateDocumentResult> GetSignerTokenForSmartCardSigningFlow(DocumentCollection dbDocumentCollection,
            DocumentCollection inputDocumentCollection, bool isGovDocument = false)
        {
            Signer signer = dbDocumentCollection.Signers.FirstOrDefault();
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping()
            {
                DocumentCollectionId = dbDocumentCollection.Id,
                SignerId = signer.Id,
                GuidToken = Guid.NewGuid(),
                JwtToken = _jwt.GenerateSignerToken(signer, _jwtSettings.SignerLinkExpirationInHours)
            };
            await _signerTokenMappingConnector.Create(signerTokenMapping);

            SaveDataForSmartCardSign(signerTokenMapping, dbDocumentCollection, isGovDocument);


            return new SelfSignUpdateDocumentResult
            {
                Token = signerTokenMapping.GuidToken
            };
        }

        private void SaveDataForSmartCardSign(SignerTokenMapping signerTokenMapping, DocumentCollection inputDocumentCollection, bool isGovDocument = false)
        {
            SmartCardInput smartCardInput = new SmartCardInput()
            {
                CollectionId = inputDocumentCollection.Id,
                SignerTokenMapping = signerTokenMapping,
                //IsGovDoc = isGovDocument
            };

            for (int i = 0; i < inputDocumentCollection.Documents.Count(); i++)
            {
                DocumentSplitSignatureDataProcessInput documentSmartCardInput = new DocumentSplitSignatureDataProcessInput()
                {
                    Id = inputDocumentCollection.Documents.ElementAt(i).Id,
                    SignatureFields = new List<SignatureFieldData>()
                };
                if (!isGovDocument)
                {
                    foreach (SignatureField sigField in inputDocumentCollection.Documents.ElementAt(i)?.Fields?.SignatureFields ?? Enumerable.Empty<SignatureField>())
                    {
                        documentSmartCardInput.SignatureFields.Add(new SignatureFieldData()
                        {
                            Image = sigField.Image,
                            Name = sigField.Name,
                        });
                    }
                }
                else
                {
                    documentSmartCardInput.SignatureFields.Add(new SignatureFieldData()
                    {
                        Name = inputDocumentCollection.Name
                    });
                }

                if (documentSmartCardInput.SignatureFields.Count > 0)
                {
                    smartCardInput.Documents.Add(documentSmartCardInput);
                }
            }

            _smartCardSigningProcess.UpdateSmartCardInput(signerTokenMapping.GuidToken.ToString(), smartCardInput);
        }
        #endregion

    }
}
