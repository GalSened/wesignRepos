
using Common.Dictionaries;
using Common.Enums;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO.Compression;
using System.IO;
using Cloudmersive.APIClient.NETCore.DocumentAndDataConvert.Model;
using Org.BouncyCastle.Cms;
using MimeKit;

namespace BL.Handlers
{
    public class DocumentCollectionsHandler : IDocumentCollections
    {

        private readonly IConfiguration _configuration;
        private readonly IDocumentPdf _documentPdf;
        private readonly ITemplatePdf _templatePdf;
        private readonly IValidator _validator;
        private readonly IGenerateLinkHandler _generateLinkHandler;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IProgramConnector _programConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly ISignerTokenMappingConnector _signerTokenMappingConnector;
        private readonly ITemplateConnector _templateConnector;
        private readonly IDocumentConnector _documentConnector;
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IContactConnector _contactConnector;
        private readonly ISendingMessageHandler _sendingMessageHandler;
        private readonly IUsers _users;
        private readonly ILogger _logger;
        private readonly IDater _dater;
        private readonly IAppendices _appendices;
        private readonly IXmlHandler<PDFFields> _xmlHandler;
        private readonly IDoneActionsHelper _doneActionsHelper;
        private readonly IMemoryCache _memoryCache;
        private readonly IDocumentCollectionOperations _documentCollectionOperations;
        private readonly IDocumentCollectionOperationsNotifier _documentCollectionOperationsNotifier;
        private readonly IFilesWrapper _fileWrapper;
        private readonly ISignersConnector _signersConnector;
        private readonly IOcrService _ocrService;

        public DocumentCollectionsHandler(IDocumentCollectionConnector documentCollectionConnector,
            IProgramConnector programConnector, ICompanyConnector companyConnector, IProgramUtilizationConnector programUtilizationConnector,
            ISignerTokenMappingConnector signerTokenMappingConnector, ITemplateConnector templateConnector,
            IDocumentConnector documentConnector, IConfigurationConnector configurationConnector, IContactConnector contactConnector,
            IDocumentPdf documentPdfHandler,
            ITemplatePdf templatePdfHandler, IValidator validator, ISignersConnector signersConnector,
            IConfiguration configuration, IUsers users, ISendingMessageHandler sendingMessageHandler,
            IGenerateLinkHandler generateLinkHandler, IMemoryCache memoryCache,
            ILogger logger, IDater dater, IAppendices appendices, IXmlHandler<PDFFields> xmlHandler,
            IDoneActionsHelper actionsHelper,
            IDocumentCollectionOperations documentCollectionOperations,
            IDocumentCollectionOperationsNotifier documentCollectionOperationsNotifier,
            IFilesWrapper fileWrapper, IOcrService ocrService)
        {
            _documentCollectionConnector = documentCollectionConnector;
            _programConnector = programConnector;
            _companyConnector = companyConnector;
            _programUtilizationConnector = programUtilizationConnector;
            _signerTokenMappingConnector = signerTokenMappingConnector;
            _templateConnector = templateConnector;
            _documentConnector = documentConnector;
            _configurationConnector = configurationConnector;
            _contactConnector = contactConnector;
            _sendingMessageHandler = sendingMessageHandler;
            _documentPdf = documentPdfHandler;
            _templatePdf = templatePdfHandler;
            _generateLinkHandler = generateLinkHandler;
            _validator = validator;
            _configuration = configuration;
            _users = users;
            _logger = logger;
            _dater = dater;
            _appendices = appendices;
            _xmlHandler = xmlHandler;
            _doneActionsHelper = actionsHelper;
            _memoryCache = memoryCache;
            _documentCollectionOperations = documentCollectionOperations;
            _documentCollectionOperationsNotifier = documentCollectionOperationsNotifier;
            _fileWrapper = fileWrapper;
            _signersConnector = signersConnector;
            _ocrService = ocrService;
        }

        public async Task Create(DocumentCollection documentCollection, IEnumerable<SignerField> readOnlyFields)
        {
            (User user, _) = await _users.GetUser();

            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            if (!await _programConnector.CanAddDocument(user))
            {
                throw new InvalidOperationException(ResultCode.ProgramUtilizationGetToMax.GetNumericString());
            }

            await ValidateTemplatesExistsAndBelongToUserGroup(user, documentCollection.Documents);
            await ValidateContactsBelongToUserGroupAndValidateSendingMethod(user, documentCollection.Signers);
            await ValidateVisualIdentificationCapacity(user, documentCollection.Signers);
            CleanFieldsFromCollectionFromCache(documentCollection);
            ValidateFieldsExistsInTemplates(documentCollection, readOnlyFields);
            await ValidateCleanFiles(documentCollection);
            await GetCallBackUrl(documentCollection, user);
            Company company = await _companyConnector.Read(new Company { Id = user.CompanyId });
            documentCollection.UserId = user.Id;
            documentCollection.GroupId = user.GroupId;
            documentCollection.ShouldEnableMeaningOfSignature = documentCollection.ShouldEnableMeaningOfSignature && (company?.CompanyConfiguration?.ShouldEnableMeaningOfSignatureOption ?? false);


            documentCollection.Documents.ForEach(document =>
            {
                if (documentCollection.Mode == DocumentMode.Online)
                {
                    AssignAllFieldsToSingleSigner(documentCollection, document);
                }
            });

            await _documentCollectionConnector.Create(documentCollection);

            foreach (Document document in documentCollection.Documents ?? Enumerable.Empty<Document>())
            {
                _documentPdf.SetId(document.Id);
                _documentPdf.CreateFromExistingTemplate(document.TemplateId);

            }

            await _programUtilizationConnector.AddDocument(user);
            int comsignIDPSigners = documentCollection.Signers.Count(s => s.SignerAuthentication.AuthenticationMode == AuthMode.ComsignVisualIDP);
            if (comsignIDPSigners > 0)
            {
                await _programUtilizationConnector.AddVisualIdentification(user, comsignIDPSigners);
            }
            _appendices.Create(documentCollection);

            documentCollection.Signers.ForEach(signer => _appendices.Create(documentCollection.Id, signer));

            _logger.Information("Successfully create documentCollection Id [{DocumentCollectionId}]", documentCollection.Id);
        }

        private async Task ValidateCleanFiles(DocumentCollection documentCollection)
        {
            foreach (Appendix senderAppendix in documentCollection.SenderAppendices ?? Enumerable.Empty<Appendix>())
            {
                senderAppendix.Base64File = (await _validator.ValidateIsCleanFile(senderAppendix.Base64File))?.CleanFile;
                senderAppendix.Name = SanitizeFilename(senderAppendix.Name);
            }


            foreach (Signer signer in documentCollection.Signers ?? Enumerable.Empty<Signer>())
            {
                foreach (Appendix senderAppendix in signer.SenderAppendices ?? Enumerable.Empty<Appendix>())
                {
                    senderAppendix.Base64File = (await _validator.ValidateIsCleanFile(senderAppendix.Base64File))?.CleanFile;
                }
            }
        }

        private string SanitizeFilename(string filename)
        {
            // Define a regular expression that matches all illegal characters except Hebrew characters
            // The regex allows ASCII characters, Hebrew characters (Unicode range \u0590-\u05FF), and replaces spaces with underscores
            string illegalCharsPattern = @"[#%&{}\\<>*?/$!'" + "`" + ":@+|=]|[^\x00-\x7F\u0590-\u05FF]";

            // Remove illegal characters from the filename
            filename = Regex.Replace(filename, illegalCharsPattern, "");

            // Replace spaces with underscores
            filename = filename.Replace(" ", "_");

            return filename;
        }

        public async Task<DocumentCollection> Read(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();
            var info = _memoryCache.Get($"{documentCollection.Id}_{user.Id}_INFO_READ");
            if (info != null)
            {

                _logger.Warning("User {UserId} : {UserEmail} - try to read collection id {DocumentCollectionId} again- after the user notified before. InvalidDocumentCollectionId", user.Id, user.Email, documentCollection.Id);
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            info = _memoryCache.Get($"{documentCollection.Id}_{user.Id}_INFO_READ_DocumentNotBelongToUserGroup");
            if (info != null)
            {

                _logger.Warning("User {UserId} : {UserEmail} - try to read collection id {DocumentCollectionId} again- after the user notified before. DocumentNotBelongToUserGroup", user.Id, user.Email, documentCollection.Id);
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            DocumentCollection collection = await _documentCollectionConnector.Read(documentCollection);
            if (collection == null)
            {
                _memoryCache.Set<bool>($"{documentCollection.Id}_{user.Id}_INFO_READ", true, TimeSpan.FromHours(3));
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            if (!await IsDocumentCollectionBelongToUserGroup(collection, false))
            {

                _memoryCache.Set<bool>($"{documentCollection.Id}_{user.Id}_INFO_READ_DocumentNotBelongToUserGroup", true, TimeSpan.FromHours(3));
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            RemoveSignedAttchmentIfNotAttchedInSignedDoc(new List<DocumentCollection>() { collection });
            return collection;

        }


        public async Task<(IEnumerable<DocumentCollection>, int)> Read(string key, bool sent, bool viewed, bool signed, bool declined, bool sendingFailed, bool canceled, string from, string to, int offset, int limit, SearchParameter searchParameter)
        {

            (User user, _) = await _users.GetUser();
            IEnumerable<DocumentCollection> documents = _documentCollectionConnector.Read(user, key, sent, viewed, signed, declined,
                            sendingFailed, canceled, from, to, offset, limit, out int totalCount, searchParameter: searchParameter);
            List<DocumentCollection> docs = documents.ToList();
            await UpdateDocumentsDeletionIn24Hours(docs); // TODO - WHY HERE? include bool parameter

            RemoveSignedAttchmentIfNotAttchedInSignedDoc(docs);

            return (docs, totalCount);
        }

        public async Task<string> GetSenderLiveLink(DocumentCollection documentCollection, Guid signerId)
        {
            documentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (documentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            if (documentCollection.Mode != DocumentMode.Online || (documentCollection.DocumentStatus != DocumentStatus.Viewed) && (documentCollection.DocumentStatus != DocumentStatus.Sent))
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            Signer signer = documentCollection.Signers.FirstOrDefault(x => x.Id == signerId);
            if (signer == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidSignerId.GetNumericString());
            }
            (User user, _) = await _users.GetUser();
            if (!await IsDocumentCollectionBelongToUserGroup(documentCollection, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });
            IEnumerable<SignerLink> links = await _generateLinkHandler.GenerateSigningLink(documentCollection, user, companyConfiguration, false);
            return links.First(x => x.Link.Contains("sender")).Link;

        }

        public async Task<IEnumerable<DocumentCollection>> ReadByStatusAndDate(Company company, DateTime notifyWhatBeforeDate)
        {
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            Company dbCompany = await _companyConnector.Read(company);
            if (dbCompany == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            if (notifyWhatBeforeDate > DateTime.UtcNow)
            {
                throw new InvalidOperationException(ResultCode.InvalidDateTime.GetNumericString());
            }

            IEnumerable<DocumentCollection> dc = _documentCollectionConnector.ReadByStatusAndDate(dbCompany, notifyWhatBeforeDate);
            return dc;
        }

        public async Task Update(DocumentCollection documentCollection, IEnumerable<SignerField> readOnlyFields)
        {
            _ = await ReadAndValidateDocumentCollectionGroup(documentCollection);

            UpdateDocumentPdfFile(documentCollection, readOnlyFields);
            await UpdateDb(documentCollection);
            _logger.Information("Successfully update documentCollection Id [{DocumentCollectionId}]", documentCollection.Id);
        }

        public async Task Delete(DocumentCollection documentCollection)
        {

            (User user, _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            var info = _memoryCache.Get($"{documentCollection.Id}_{user.Id}_INFO_READ");
            if (info != null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            Guid idForCache = documentCollection.Id;
            documentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (documentCollection == null)
            {
                _memoryCache.Set<bool>($"{idForCache}_{user.Id}_INFO_READ", true, TimeSpan.FromHours(3));
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            if (!await IsDocumentCollectionBelongToUserGroup(documentCollection, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            _logger.Information("User {UserId}:{UserEmail} Deleted document collection {DocumentCollectionId} name {DocumentCollectionName} - that created at {DocumentCollectionCreationTime} was in status {DocumentCollectionDocumentStatus}.", user.Id, user.Email,
                documentCollection.Id, documentCollection.Name, documentCollection.CreationTime, documentCollection.DocumentStatus);
            await _documentCollectionConnector.UpdateStatus(documentCollection, DocumentStatus.Deleted);
            await Task.Run(async () => await _documentCollectionOperationsNotifier.AddNotification(documentCollection, DocumentNotification.DocumentDeleted, null));
        }

        public async Task Cancel(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            var info = _memoryCache.Get($"{documentCollection.Id}_{user.Id}_INFO_READ");
            if (info != null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            Guid idForCache = documentCollection.Id;
            documentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (documentCollection == null)
            {
                _memoryCache.Set<bool>($"{idForCache}_{user.Id}_INFO_READ", true, TimeSpan.FromMinutes(3));
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            if (!await IsDocumentCollectionBelongToUserGroup(documentCollection, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            if (documentCollection.DocumentStatus == DocumentStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.CannotCancelSignedDocument.GetNumericString());
            }
            if (documentCollection.DocumentStatus == DocumentStatus.Canceled)
            {
                throw new InvalidOperationException(ResultCode.DocumentAlreadyCanceled.GetNumericString());
            }

            foreach (Signer signer in documentCollection.Signers)
            {
                await _signerTokenMappingConnector.Delete(new SignerTokenMapping { SignerId = signer.Id });
            }


            await _documentCollectionConnector.UpdateStatus(documentCollection, DocumentStatus.Canceled);
            _logger.Information("Document canceled Successfully {DocumentCollectionId}: name {DocumentCollectionName} by user {UserId}: {UserEmail}", documentCollection.Id, documentCollection.Name, user.Id, user.Email);
            await Task.Run(async () => await _documentCollectionOperationsNotifier.AddNotification(documentCollection, DocumentNotification.DocumentCanceled, null));

        }

        public async Task<IDictionary<Guid, (string, string, byte[])>> Download(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.ReadWithTemplateInfo(documentCollection);
            if (dbDocumentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            if (dbDocumentCollection.Mode == DocumentMode.SelfSign && dbDocumentCollection.DocumentStatus != DocumentStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.CannotDownloadUnsignedDocument.GetNumericString());
            }
            if (!await IsDocumentCollectionBelongToUserGroup(dbDocumentCollection, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            if (dbDocumentCollection.DocumentStatus == DocumentStatus.Canceled)
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }

            documentCollection.Name = dbDocumentCollection.Name;
            Dictionary<Guid, (string, string, byte[])> documents = new Dictionary<Guid, (string, string, byte[])>();
            foreach (Document document in dbDocumentCollection.Documents ?? Enumerable.Empty<Document>())
            {
                AddDocumentDataToDocumentsDictionary(documents, document);

            }

            DocumentCollection dbDocumentCollectionWithSigners = await _documentCollectionConnector.Read(documentCollection);
            foreach (var signer in dbDocumentCollectionWithSigners.Signers)
            {
                if (signer.SignerAttachments.Count() != 0 )
                {
                    var attachments = (await DownloadAttachmentsForSigner(documentCollection, signer.Id)).ToDictionary(attachment => attachment.Key, attachment => attachment.Value);
                    attachments.ForEach(attachment => documents.Add(StringToGuid(attachment.Key + "." + attachment.Value.Item1.ToString()), (attachment.Key + "." + attachment.Value.Item1.ToString(), null, attachment.Value.Item2)));
                }
            }

            _logger.Debug("User {UserName} id {UserId} downloading collection name {CollectionName} collectionId {CollectionId}"
                , user.Name, user.Id, documentCollection.Name, documentCollection.Id);
            return documents;
        }

        public Guid StringToGuid(string str)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
                byte[] guidBytes = new byte[16];
                Array.Copy(hashBytes, guidBytes, 16);
                return new Guid(guidBytes);
            }
        }

        public async Task<IDictionary<Guid, (string, string, byte[])>> DownloadAllSelected(IEnumerable<DocumentCollection> documentCollections)
        {
            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            List<Guid> ids = new List<Guid>();
            foreach (DocumentCollection dc in documentCollections)
            {
                ids.Add(dc.Id);
            }
            IEnumerable<DocumentCollection> dbDocumentCollections = _documentCollectionConnector.ReadCollectionsInList(ids);
            dbDocumentCollections = dbDocumentCollections.Where(dc => dc.DocumentStatus == DocumentStatus.Signed || dc.DocumentStatus == DocumentStatus.ExtraServerSigned);
            if (dbDocumentCollections == null || !dbDocumentCollections.Any())
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            IEnumerable<DocumentCollection> filteredDocuments = await FilterUndownloadableDocumentCollections(dbDocumentCollections);

            if (filteredDocuments == null || !filteredDocuments.Any())
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            documentCollections.FirstOrDefault().Name = filteredDocuments.FirstOrDefault().Name;
            Dictionary<Guid, (string, string, byte[])> documents = new Dictionary<Guid, (string, string, byte[])>();
            Dictionary<Guid, (string, string, byte[])> filesInDocument = new Dictionary<Guid, (string, string, byte[])>();

            foreach (DocumentCollection dbDocumentCollection in filteredDocuments)
            {
                DocumentCollection dbDocumentCollectionWithSigners = await _documentCollectionConnector.Read(dbDocumentCollection);
                filesInDocument.Clear();
                
                bool zipFile = false;
                bool isFirstDocument = true;
                Guid firstDocumentId = Guid.Empty;
                string firstTemplateId = string.Empty;
                string firstDocumentName = string.Empty;

                foreach (Document document in dbDocumentCollection?.Documents ?? Enumerable.Empty<Document>())
                {
                    if (isFirstDocument)
                    {
                        firstDocumentId = document.Id;
                        firstDocumentName = document.Name;
                        firstTemplateId = document.TemplateId.ToString();
                        isFirstDocument = false; 
                    }
                    AddDocumentDataToDocumentsDictionary(filesInDocument, document);
                }
                
                foreach (var signer in dbDocumentCollectionWithSigners.Signers)
                {
                    if (dbDocumentCollection.Documents.Count() == 1)
                    {
                        if (signer.SignerAttachments.Count() > 0)
                        {
                            var attachments = (await DownloadAttachmentsForSigner(dbDocumentCollection, signer.Id)).ToDictionary(attachment => attachment.Key, attachment => attachment.Value);
                            var attachmentsKey = StringToGuid(attachments.Keys.First() + "." + attachments.Values.First().Item1.ToString());
                            attachments.ForEach(attachment => filesInDocument.Add(StringToGuid(attachment.Key + "." + attachment.Value.Item1.ToString()), (attachment.Key + "." + attachment.Value.Item1.ToString(), null, attachment.Value.Item2)));
                            zipFile = true;
                        }
                    }
                    else if (dbDocumentCollection.Documents.Count() > 1)
                    {
                        if (signer.SignerAttachments.Count() != 0)
                        { 
                            var attachments = (await DownloadAttachmentsForSigner(dbDocumentCollection, signer.Id)).ToDictionary(attachment => attachment.Key, attachment => attachment.Value);
                            attachments.ForEach(attachment => filesInDocument.Add(StringToGuid(attachment.Key + "." + attachment.Value.Item1.ToString()), (attachment.Key + "." + attachment.Value.Item1.ToString(), null, attachment.Value.Item2)));
                            
                        }
                        zipFile = true;
                    }
                }
                if (zipFile) // Creates zip file and add it to documents
                {
                    var zipStream = CreateZipArchive(filesInDocument);
                    var zipFileName = firstDocumentName + ".zip";
                    documents.Add(firstDocumentId, (zipFileName, firstTemplateId, zipStream.ToArray()));
                    zipFile = false;
                }
                else
                {
                    foreach (var fileEntry in filesInDocument) //Add every file in filesInDocument to documents
                    {
                        if (!documents.ContainsKey(fileEntry.Key))
                        {
                            documents.Add(fileEntry.Key, fileEntry.Value);
                        }
                    }
                }
            }
            _logger.Debug("User {UserName} id {UserID} downloading multiple documents collection {DocumensIds}", user.Name, user.Id,
               string.Join(", ", filteredDocuments.Select(x => $"{x.Id} Name: {x.Name}")));

            return documents;
        }

        private MemoryStream CreateZipArchive(IDictionary<Guid, (string name, string templateId, byte[] content)> files)
        {
            var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var zipItem in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(zipItem.Value.name);
                    var fileExtension = Path.GetExtension(zipItem.Value.name);

                    if (string.IsNullOrEmpty(fileExtension))
                    {
                        fileExtension = ".pdf";
                    }
                    var entry = zip.CreateEntry($"{fileName.Replace(".", "_")}{fileExtension}");
                    using (var entryStream = entry.Open())
                    {
                        var content = new MemoryStream(zipItem.Value.content);
                        content.CopyTo(entryStream);
                    }
                }
            }
            zipStream.Position = 0;
            return zipStream;
        }

        private void AddDocumentDataToDocumentsDictionary(Dictionary<Guid, (string, string, byte[])> documents, Document document)
        {
            if (documents.ContainsKey(document.Id))
            {
                return;
            }

            if (!_fileWrapper.Documents.IsDocumentExist(DocumentType.Document, document.Id))
            {
                throw new Exception($"File [{DocumentType.Document}]  [{document.Id}] not exist");
            }
            documents.Add(document.Id, (document.Name, document.TemplateId.ToString(),
                 _fileWrapper.Documents.ReadDocument(DocumentType.Document, document.Id)));
        }

        public async Task<IDictionary<string, (FileType, byte[])>> DownloadAttachmentsForSigner(DocumentCollection documentCollection, Guid signerId)
        {
            Dictionary<string, (FileType, byte[])> result;


            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }

            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (dbDocumentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            await ValidateDownloadAttchments(dbDocumentCollection);

            Signer dbSigner = dbDocumentCollection.Signers.FirstOrDefault(x => x.Id == signerId);
            if (dbSigner == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidSignerId.GetNumericString());
            }

            documentCollection.Name = dbDocumentCollection.Name;
            result = _fileWrapper.Signers.ReadAttachments(dbSigner);


            if (result.Count == 0)
            {
                throw new InvalidOperationException(ResultCode.SignerNotUpladedAttchmentsOrNotRequoredTo.GetNumericString());
            }

            _logger.Debug("User {UserName} id {UserID} downloading signer attachments for document {DocumentName} id {DocumentId}",
                user.Name, user.Id, documentCollection.Name, dbDocumentCollection.Id);
            return result;
        }

        public async Task<(string name, byte[] content)> DownloadTrace(DocumentCollection documentCollection, int offset)
        {
            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (dbDocumentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            if (dbDocumentCollection?.Mode == DocumentMode.SelfSign && dbDocumentCollection.DocumentStatus != DocumentStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.CannotDownloadUnsignedDocument.GetNumericString());
            }
            DocumentCollectionAuditTrace documentCollectionAuditTrace = DocumentCollectionToListString(dbDocumentCollection);
            byte[] bytes = _documentPdf.CreateTraceFile(documentCollectionAuditTrace, dbDocumentCollection.Mode);

            //  List<string> rows = DocumentCollectionToListString(dbDocumentCollection, offset);

            //  bytes = _documentPdf.CreateTraceFile(rows);

            _logger.Debug("User {UserName} id {UserID} downloading traces for document {DocumentName} id {DocumentId}",
                user.Name, user.Id, dbDocumentCollection.Name, dbDocumentCollection.Id);

            return ($"{dbDocumentCollection.Name}_trace", bytes);
        }

        public async Task<MessageInfo> BuildSignerLink(DocumentCollection documentCollection, CompanyConfiguration companyConfiguration, Configuration configuration)
        {
            (User user, _) = await _users.GetUser();
            if (!await IsDocumentCollectionBelongToUserGroup(documentCollection, true))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }

            IEnumerable<SignerLink> links = await _generateLinkHandler.GenerateSigningLink(documentCollection, user, companyConfiguration);
            Signer signer = documentCollection.Signers.First();

            return BuildMessageInfo(documentCollection, user, companyConfiguration, configuration, signer, links?.FirstOrDefault(x => x.SignerId == signer.Id).Link);
        }
        public async Task<IEnumerable<SignerLink>> SendSignerLinks(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();
            if (!await IsDocumentCollectionBelongToUserGroup(documentCollection, true))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            CompanyConfiguration companyConfiguration = (await _companyConnector.Read(new Company() { Id = user.CompanyId }))?.CompanyConfiguration;
            IEnumerable<SignerLink> links = await _generateLinkHandler.GenerateSigningLink(documentCollection, user, companyConfiguration);
            //TODO - BLOCK THIS SEND IF
            if (documentCollection.Notifications.ShouldSendDocumentForSigning ?? true)
            {
                await SendSignerLinks(documentCollection, links, user, companyConfiguration);
            }

            return links;
        }

        public async Task<Document> GetPageInfoByDocumentId(DocumentCollection documentCollection, int page)
        {
            DocumentCollection dbdocumentCollection = await ReadAndValidateDocumentCollectionGroup(documentCollection);
            if (!IsDocumentBelongToDocumentCollection(dbdocumentCollection, documentCollection))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToDocumentCollection.GetNumericString());
            }


            Document document = documentCollection.Documents.FirstOrDefault();
            // get the page image only
            int imagesCount = GetImagesCount(documentCollection, document);

            if (page > imagesCount)
            {
                throw new InvalidOperationException(ResultCode.InvalidPageNumber.GetNumericString());
            }

            LoadPdfFields(page, document);

            Template dbTemplate = await _templateConnector.Read(new Template { Id = document.TemplateId });
            FillTemplateFieldsMissingData(dbTemplate?.Fields, document?.Fields);

            PdfImage image = GetPdfImageByIndex(documentCollection, page);
            image.Base64Image = $"data:image/jpeg;base64,{image.Base64Image}";
            document.Images = new List<PdfImage>() { image };
            document.PagesCount = imagesCount;

            return document;
        }

        public async Task<string> GetOcrHtmlFromImage(string base64Image) {
            return await _ocrService.GenerateOcrHtmlFromBase64ImageAsync(base64Image);
        }

        public async Task<Document> GetPagesInfoByDocumentId(DocumentCollection documentCollection, int offset, int limit)
        {
            DocumentCollection dbDocumentCollection = await ReadAndValidateDocumentCollectionGroup(documentCollection);

            if (!IsDocumentBelongToDocumentCollection(dbDocumentCollection, documentCollection))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToDocumentCollection.GetNumericString());
            }


            Document document = dbDocumentCollection.Documents.FirstOrDefault(x => x.Id == documentCollection.Documents.FirstOrDefault().Id);

            DocumentMemoryCache documentMemoryCache = _memoryCache.Get<DocumentMemoryCache>(document.Id);

            int imagesCount = 0;
            if (documentMemoryCache == null)
            {
                imagesCount = GetImagesCount(dbDocumentCollection, document);
            }
            else
            {
                SetIdInPdfHandler(dbDocumentCollection, document);
                imagesCount = documentMemoryCache.PageCount;
            }


            if (offset > imagesCount)
            {
                throw new InvalidOperationException(ResultCode.InvalidPageNumber.GetNumericString());
            }

            int endPage = offset + limit > imagesCount ? imagesCount + 1 : offset + limit;

            if (documentMemoryCache == null)
            {
                LoadPdfFields(offset, endPage, document, dbDocumentCollection.DocumentStatus);
            }
            else
            {

                document.Fields = _documentPdf.GetAllFieldsInRange(offset, endPage, documentMemoryCache.pdfFields);
                if (dbDocumentCollection.DocumentStatus == DocumentStatus.Signed || dbDocumentCollection.DocumentStatus == DocumentStatus.ExtraServerSigned)
                {
                    IEnumerable<DocumentSignatureField> sigsDoc = _documentConnector.ReadSignatures(new Document { Id = document.Id });
                    foreach (SignatureField signatureField in document.Fields.SignatureFields)
                    {
                        DocumentSignatureField sigDoc = sigsDoc.FirstOrDefault(x => x.FieldName == signatureField.Name);
                        if (sigDoc != null)
                        {
                            signatureField.Image = sigDoc.Image;
                        }
                    }
                }
            }

            Template dbTemplate = await _templateConnector.Read(new Template { Id = document.TemplateId });
            FillTemplateFieldsMissingData(dbTemplate?.Fields, document?.Fields);

            IList<PdfImage> images = GetPdfImages(dbDocumentCollection, offset, endPage);
            foreach (PdfImage image in images)
            {
                image.Base64Image = $"data:image/jpeg;base64,{image.Base64Image}";
            }

            document.Images = images;
            document.PagesCount = imagesCount;

            return document;
        }

        public async Task<(Document, string)> GetPagesCountByDocumentId(DocumentCollection documentCollection)
        {

            string documentCollectionName;
            DocumentCollection dbDocumentCollection = await ReadAndValidateDocumentCollectionGroup(documentCollection);

            if (dbDocumentCollection.Documents.FirstOrDefault(x => x.Id == documentCollection.Documents.FirstOrDefault().Id) == null)
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToDocumentCollection.GetNumericString());
            }

            documentCollectionName = dbDocumentCollection.Name;
            if (dbDocumentCollection.Mode == DocumentMode.SelfSign)
            {
                DocumentStatus newStatus = dbDocumentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.Viewed;
                if (newStatus != dbDocumentCollection.DocumentStatus)
                {
                    await _documentCollectionConnector.UpdateStatus(dbDocumentCollection, newStatus);
                }
            }

            DocumentMemoryCache documentMemoryCache = new DocumentMemoryCache();

            Document document = dbDocumentCollection.Documents.FirstOrDefault(x => x.Id == documentCollection.Documents.FirstOrDefault().Id);
            _documentPdf.SetId(document.Id);
            documentMemoryCache.PageCount = _documentPdf.GetPagesCount();
            documentMemoryCache.pdfFields = _documentPdf.GetAllFields(1, documentMemoryCache.PageCount);
            document.PagesCount = documentMemoryCache.PageCount;
            document.Images = new PdfImage[documentMemoryCache.PageCount];
            documentMemoryCache.Images = document.Images.ToList();
            _memoryCache.Set(document.Id, documentMemoryCache, TimeSpan.FromSeconds(20));

            return (document, documentCollectionName);
        }

        public async Task<SignerLink> ResendDocument(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }

            DocumentCollection dbDocumentCollection = await ReadAndValidateDocumentCollectionGroup(documentCollection);

            Signer signer = dbDocumentCollection.Signers.FirstOrDefault(x => x.Id == documentCollection.Signers.First().Id);
            if (signer == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidSignerId.GetNumericString());
            }
            signer.SendingMethod = documentCollection.Signers.FirstOrDefault().SendingMethod;
            
            await _signersConnector.UpdateOtpAttempts(signer.Id, 0);

            if (dbDocumentCollection.Signers.FirstOrDefault(x => x.Id == documentCollection.Signers.First().Id).Status == SignerStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.CannotResendSignedDocument.GetNumericString());
            }

            if (dbDocumentCollection.Mode == DocumentMode.OrderedGroupSign)
            {
                int currentIndex = dbDocumentCollection.Signers.ToList().IndexOf(signer);
                Signer prevSigner = dbDocumentCollection.Signers.ElementAtOrDefault(currentIndex - 1);
                if (prevSigner != null && prevSigner.Status != SignerStatus.Signed)
                {
                    throw new InvalidOperationException(ResultCode.TryToSendToSendDocNotInTheRightOrder.GetNumericString());
                }
            }
            bool sendDoc = (dbDocumentCollection?.Notifications?.ShouldSendDocumentForSigning ?? true)
                || (documentCollection?.Notifications?.ShouldSendDocumentForSigning ?? true);



            CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });
            return (await _documentCollectionOperations.SendLinkToSpecificSigner(dbDocumentCollection, signer, user, companyConfiguration, sendDoc, MessageType.BeforeSigning)).FirstOrDefault();
        }

        public async Task<List<SignerLink>> ReactivateDocument(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();

            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }

            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);

            if (dbDocumentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            if (dbDocumentCollection.DocumentStatus != DocumentStatus.Declined)
            {
                throw new InvalidOperationException(ResultCode.DocumentNotDeclined.GetNumericString());
            }

            if (!await IsDocumentCollectionBelongToUserGroup(dbDocumentCollection, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }

            List<Signer> sendToSigenrs = new List<Signer>();

            if (dbDocumentCollection.Mode == DocumentMode.GroupSign)
            {
                sendToSigenrs = dbDocumentCollection.Signers.Where(signer => signer.Status != SignerStatus.Signed).ToList();

                if (sendToSigenrs.Count == 0)
                {
                    throw new InvalidOperationException(ResultCode.DocumentAlreadySignedBySigner.GetNumericString());
                }
            }

            else
            {
                Signer signer = dbDocumentCollection.Signers.FirstOrDefault(signer => signer.Status == SignerStatus.Rejected);

                if (signer == null)
                {
                    throw new InvalidOperationException(ResultCode.InvalidSignerId.GetNumericString());
                }

                sendToSigenrs.Add(signer);
            }

            await UpdateDoucmentReactivated(dbDocumentCollection, sendToSigenrs.Select(x => x.Id).ToList());

            dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);

            bool sendDoc = (dbDocumentCollection?.Notifications?.ShouldSendDocumentForSigning ?? true) || (documentCollection?.Notifications?.ShouldSendDocumentForSigning ?? true);

            var signerLinks = new List<SignerLink>();

            Company company = new Company() 
            { 
                Id = user.CompanyId
            };

            CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(company);

            foreach (Signer signer in sendToSigenrs)
            {
                signerLinks.Add((await _documentCollectionOperations.SendLinkToSpecificSigner(dbDocumentCollection, signer, user, companyConfiguration, sendDoc, MessageType.BeforeSigning)).FirstOrDefault());
            }

            return signerLinks;
        }

        public async Task<IEnumerable<SignerLink>> GetDocumentCollectionSigningLinks(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }

            documentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (documentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            if (!await IsDocumentCollectionBelongToUserGroup(documentCollection, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            if (documentCollection.DocumentStatus != DocumentStatus.Created &&
                 documentCollection.DocumentStatus != DocumentStatus.Sent &&
                  documentCollection.DocumentStatus != DocumentStatus.Viewed)
            {
                throw new InvalidOperationException(ResultCode.CannotCreateSigningLinkToSignerThatSignedOrDecline.GetNumericString());
            }

            List<Signer> signers = new List<Signer>();

            if (documentCollection.Mode == DocumentMode.GroupSign)
            {
                signers = documentCollection.Signers.Where(x => x.Status != SignerStatus.Signed && x.Status != SignerStatus.Rejected).ToList();
            }
            else if (documentCollection.Mode == DocumentMode.OrderedGroupSign)
            {
                signers = documentCollection.Signers.Where(x => x.Status != SignerStatus.Signed && x.Status != SignerStatus.Rejected).Take(1).ToList();
            }

            if (signers.Count == 0)
            {
                throw new InvalidOperationException(ResultCode.CannotCreateSigningLinkToSignerThatSignedOrDecline.GetNumericString());
            }


            List<SignerLink> links = new List<SignerLink>();
            CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });

            foreach (Signer signer in signers)
            {
                links.Add((await _documentCollectionOperations.SendLinkToSpecificSigner(documentCollection, signer, user, companyConfiguration, false, MessageType.BeforeSigning, false)).FirstOrDefault());
            }

            return links;
        }

        public async Task ShareDocument(DocumentCollection documentCollection)
        {
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
            if (dbDocumentCollection.DocumentStatus != DocumentStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.CannotShareUnsignedDocument.GetNumericString());
            }

            Configuration appConfiguration = await _configurationConnector.Read();
            CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });

            Signer signer = dbDocumentCollection.Signers.FirstOrDefault(x => x.Contact?.Id == documentCollection.Signers.First().Contact.Id);
            if (signer == null)
            {
                signer = documentCollection.Signers.First();
                Guid res = Guid.NewGuid();
                signer.Id = res;
            }
            SignerLink signerLink = await _generateLinkHandler.GenerateDocumentDownloadLink(dbDocumentCollection, signer, user, companyConfiguration);
            SendingMethod sendingMethod = documentCollection.Signers.First().SendingMethod;
            var contact = await _contactConnector.Read(documentCollection.Signers.First().Contact);
            string messageContent = "";
            IMessageSender messageSender = _sendingMessageHandler.ExecuteCreation(sendingMethod);
            if (sendingMethod == SendingMethod.SMS)
            {
                messageContent = _configuration.GetShareDocument(user.UserConfiguration.Language);
                messageContent = messageContent.Replace("[CONTACT_NAME]", contact.Name).Replace("[LINK]", signerLink.Link).Replace("[SENDER_NAME]", user.Name).Replace("[DOCUMENT_NAME]", dbDocumentCollection.Name);

            }
            MessageInfo messageInfo = new MessageInfo()
            {
                User = user,
                MessageType = MessageType.SharedDocumentNotification,
                Link = signerLink?.Link,
                Contact = contact,
                DocumentCollection = dbDocumentCollection,
                MessageContent = messageContent

            };
            await messageSender.Send(appConfiguration, companyConfiguration, messageInfo);
        }

        public async Task ReplaceSigner(DocumentCollection documentCollection, Guid oldSignerId)
        {
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
            Signer oldSigner = dbDocumentCollection.Signers?.FirstOrDefault(x => x.Id == oldSignerId);
            if (oldSigner == null || oldSigner.Status == SignerStatus.Rejected || oldSigner.Status == SignerStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.InvalidSignerId.GetNumericString());
            }
            if (dbDocumentCollection.DocumentStatus == DocumentStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.DocumentAlreadySignedBySigner.GetNumericString());
            }
            bool needToSendDoc = true;
            if (dbDocumentCollection.Mode == DocumentMode.OrderedGroupSign)
            {
                int currentIndex = dbDocumentCollection.Signers.ToList().IndexOf(oldSigner);
                Signer prevSigner = dbDocumentCollection.Signers.ElementAtOrDefault(currentIndex - 1);
                if (prevSigner != null && prevSigner.Status != SignerStatus.Signed)
                {
                    needToSendDoc = false;
                }
            }
            Signer newSigner = documentCollection.Signers.First();
            oldSigner.Contact = newSigner.Contact;
            if (needToSendDoc)
            {
                oldSigner.Status = SignerStatus.Sent;
                oldSigner.TimeSent = _dater.UtcNow();
                oldSigner.TimeLastSent = _dater.UtcNow();
            }

            if(oldSigner?.SignerAuthentication?.OtpDetails != null)
            {
                oldSigner.SignerAuthentication.OtpDetails.Attempts = 0;
            }

            oldSigner.TimeViewed = newSigner.TimeViewed;
            oldSigner.SendingMethod = newSigner.SendingMethod;

            if (needToSendDoc)
            {
                Configuration appConfiguration = await _configurationConnector.Read();
                CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });
                int expirationTime = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);
                SignerLink signerLink = await _generateLinkHandler.GenerateSigningLinkToSingleSigner(dbDocumentCollection, true, expirationTime, appConfiguration, oldSigner);
                string messageBefore = _configuration.GetBeforeMessage(user, appConfiguration, companyConfiguration);
                messageBefore = _documentCollectionOperations.UpdateMessage(newSigner.SendingMethod, messageBefore, signerLink.Link, dbDocumentCollection.Name);
                IMessageSender messageSender = _sendingMessageHandler.ExecuteCreation(oldSigner.SendingMethod);
                MessageInfo messageInfo = new MessageInfo()
                {
                    User = user,
                    MessageType = MessageType.BeforeSigning,
                    Link = signerLink?.Link,
                    Contact = oldSigner.Contact,
                    DocumentCollection = dbDocumentCollection,
                    MessageContent = messageBefore
                };
                await messageSender.Send(appConfiguration, companyConfiguration, messageInfo);
            }
            await _documentCollectionConnector.Update(dbDocumentCollection);
        }

        public async Task<byte[]> ExportDistributionDocumentsCollection(Language language)
        {
            (User user, _) = await _users.GetUser();
            IEnumerable<DocumentCollection> documents = _documentCollectionConnector.Read(user, key: null, true, true,
                                                                  true, true, true, true, from: null,
                                                                  to: null, offset: 0, limit: -1, out int _, true);
            IList<IDictionary<string, object>> rows = GetCsvInfo(documents, language);
            string result = DocumentsRowsToString(rows);

            return Encoding.UTF8.GetBytes(result);

        }
        public async Task<byte[]> ExportDocumentsCollection(bool sent, bool viewed, bool signed, bool declined, bool sendingFailed, bool canceled, Language language)
        {
            (User user, _) = await _users.GetUser();
            IEnumerable<DocumentCollection> documents = _documentCollectionConnector.Read(user, key: null, sent, viewed,
                                                                  signed, declined, sendingFailed, canceled, from: null,
                                                                  to: null, offset: 0, limit: -1, out int _);
            IList<IDictionary<string, object>> rows = GetCsvInfo(documents, language);
            string result = DocumentsRowsToString(rows);

            return Encoding.UTF8.GetBytes(result);
        }


        public Task<PDFFields> ExportFieldsFromPdfData(DocumentCollection documentCollection)
        {
            return ExportAllInfoFromDocumentCollection(documentCollection);
        }


        public async Task<(byte[] xml, byte[] csv)> ExportFieldsFromPdf(DocumentCollection documentCollection, bool xmlOnly)
        {
            PDFFields fieldsResults = await ExportAllInfoFromDocumentCollection(documentCollection);


            byte[] csvData = null;
            if (!xmlOnly)
            {
                csvData = CreateCSVForFieldExport(fieldsResults);
            }
            string result = _xmlHandler.ToXml(fieldsResults);

            return (Encoding.UTF8.GetBytes(result), csvData);
        }

        public async Task UpdateDb(DocumentCollection documentCollection, bool isFromDistributionMechanism = false)
        {
            foreach (Signer signer in documentCollection?.Signers ?? Enumerable.Empty<Signer>())
            {
                foreach (SignerField signerField in signer?.SignerFields ?? Enumerable.Empty<SignerField>())
                {
                    Document document = documentCollection.Documents?.FirstOrDefault(d => d.TemplateId == signerField?.TemplateId);
                    if (document != null)
                    {
                        signerField.DocumentId = document.Id;
                    }
                }
            }
            await _documentCollectionConnector.Update(documentCollection);

            if (isFromDistributionMechanism)
            {
                return;
            }
            foreach (Document document in documentCollection?.Documents ?? Enumerable.Empty<Document>())
            {
                await _templateConnector.IncrementUseCount(new Template { Id = document.TemplateId }, 1);
            }
        }

        public void UpdateDocumentPdfFile(DocumentCollection documentCollection, IEnumerable<SignerField> readOnlyFields, bool isLoaded = false)
        {
            foreach (Document document in documentCollection?.Documents ?? Enumerable.Empty<Document>())
            {
                if (!isLoaded)
                {
                    _documentPdf.Load(document.Id);
                }

                PDFFields fields = _documentPdf.GetAllFields();

                foreach (Signer signer in documentCollection?.Signers ?? Enumerable.Empty<Signer>())
                {
                    SetValueToFields(document, fields, signer.SignerFields);
                    SetValueToFields(document, fields, readOnlyFields);
                }
                _documentPdf.DoUpdateFields(fields);

            }
        }


        public async Task ExtraServerSigning(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (dbDocumentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            if (!await IsDocumentCollectionBelongToUserGroup(dbDocumentCollection, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            if (dbDocumentCollection.DocumentStatus != DocumentStatus.Signed)
            {
                throw new InvalidOperationException(ResultCode.DocumentNotSigned.GetNumericString());
            }
            if (!dbDocumentCollection.ShouldSignUsingSigner1AfterDocumentSigningFlow)
            {
                throw new InvalidOperationException(ResultCode.DocumentNotConfigureToBeSignUsingSigner1AfterDocumentSigningFlow.GetNumericString());
            }
            (User currentUser, _) = await _users.GetUser();
            CompanySigner1Details companySigner1Details1 = (await _companyConnector.Read(new Company() { Id = currentUser.CompanyId })).CompanySigner1Details;
            ResultCode result = await _doneActionsHelper.HandlerSigningUsingSigner1AfterDocumentSigningFlow(dbDocumentCollection, companySigner1Details1);
            if (result != ResultCode.Success)
            {
                throw new InvalidOperationException(result.GetNumericString(),
                                                        new Exception($"Failed to sign using signer1 after document collection [{dbDocumentCollection.Id}] signing flow"));
            }
        }

        #region Private
        private async Task<IEnumerable<DocumentCollection>> FilterUndownloadableDocumentCollections(IEnumerable<DocumentCollection> dbDocumentCollections)
        {
            List<DocumentCollection> downloadableDocuments = new List<DocumentCollection>();
            List<DocumentCollection> undownloadable = new List<DocumentCollection>();
            foreach (DocumentCollection dbDocument in dbDocumentCollections) // checking for errors 
            {
                if (await IsDocumentCollectionDownloadable(dbDocument))
                {
                    downloadableDocuments.Add(dbDocument);
                }
                else
                {
                    undownloadable.Add(dbDocument);
                }
            }
            foreach (DocumentCollection documentCollection in undownloadable)
            {
                _logger.Warning("Can't download this document collection: {DocumentCollectionName}", documentCollection.Name);
                foreach (Document document in documentCollection.Documents)
                {

                    _logger.Warning("The document {DocumentName} has not been downloaded", document.Name);
                }
            }
            return downloadableDocuments;
        }

        private async Task<bool> IsDocumentCollectionDownloadable(DocumentCollection dbDocument)
        {
            if (dbDocument?.DocumentStatus != DocumentStatus.Signed && dbDocument?.DocumentStatus != DocumentStatus.ExtraServerSigned)
            {
                return false;
            }
            else if (!await IsDocumentCollectionBelongToUserGroup(dbDocument, false))
            {
                return false;
            }
            return true;
        }

        private async Task ValidateTemplatesExistsAndBelongToUserGroup(User user, IEnumerable<Document> documents)
        {
            if (!documents.Any())
            {
                throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
            }

            List<Guid> templatesIds = documents.Select(x => x.TemplateId).Distinct().ToList();
            List<Template> templates;
            if (templatesIds.Count == 1)
            {
                templates = new List<Template>();
                Template template = await _templateConnector.Read(new Template() { Id = templatesIds.FirstOrDefault() });
                templates.Add(template);
            }
            else
            {
                templates = _templateConnector.Read(templatesIds).ToList();
            }
            if (templatesIds.Count != templates.Count)
            {
                throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
            }
            foreach (Template template in templates ?? Enumerable.Empty<Template>())
            {
                if (template == null)
                {
                    throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());

                }
                if (template.GroupId != user.GroupId)
                {
                    throw new InvalidOperationException(ResultCode.TemplateNotBelongToUserGroup.GetNumericString());
                }
            }


        }

        private async Task ValidateContactsBelongToUserGroupAndValidateSendingMethod(User user, IEnumerable<Signer> signers)
        {
            foreach (Signer signer in signers ?? Enumerable.Empty<Signer>())
            {
                signer.Contact.UserId = user.Id;
                signer.Contact.GroupId = user.GroupId;
                signer.Contact = await _contactConnector.Read(signer.Contact);
                if (signer.Contact == null || signer.Contact.GroupId != user.GroupId)
                {
                    throw new InvalidOperationException(ResultCode.ContactNotCreatedByUser.GetNumericString());
                }

                if (signer.SendingMethod == SendingMethod.Email && string.IsNullOrWhiteSpace(signer.Contact.Email) ||
                  signer.SendingMethod == SendingMethod.SMS && string.IsNullOrWhiteSpace(signer.Contact.Phone))
                {
                    throw new InvalidOperationException(ResultCode.SignerMethodNotFeetToContactMeans.GetNumericString());
                }

            }
        }

        private async Task ValidateVisualIdentificationCapacity(User user, IEnumerable<Signer> signers)
        {
            int ComsignIDPSigners = signers.Count(s => s.SignerAuthentication.AuthenticationMode == AuthMode.ComsignVisualIDP);
            if (ComsignIDPSigners == 0)
            {
                return;
            }
            if (!await _programConnector.CanAddVisualIdentifications(user, ComsignIDPSigners))
            {
                throw new InvalidOperationException(ResultCode.VisualIdentificationsExceedLicenseLimit.GetNumericString());
            }
        }

        private void ValidateFieldsExistsInTemplates(DocumentCollection documentCollection, IEnumerable<SignerField> readOnlyFields)
        {
            foreach (Document document in documentCollection.Documents ?? Enumerable.Empty<Document>())
            {
                _templatePdf.Load(document.TemplateId);
                PDFFields fields = _templatePdf.GetAllFields(false);
                foreach (Signer signer in documentCollection.Signers ?? Enumerable.Empty<Signer>())
                {
                    IEnumerable<SignerField> signerFields = signer.SignerFields.Where(x => x.TemplateId == document.TemplateId);
                    foreach (SignerField signerField in signerFields ?? Enumerable.Empty<SignerField>())
                    {
                        if (fields.TextFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower() || x.Description.ToLower() == signerField.FieldName.ToLower()) == null &&
                            fields.ChoiceFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower() || x.Description.ToLower() == signerField.FieldName.ToLower()) == null &&
                            fields.CheckBoxFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower() || x.Description.ToLower() == signerField.FieldName.ToLower()) == null &&
                            fields.SignatureFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower() || x.Description.ToLower() == signerField.FieldName.ToLower()) == null &&
                            !RadioExistInFields(signerField, fields))
                        {
                            throw new InvalidOperationException(ResultCode.FieldNameNotExist.GetNumericString(), new Exception($"Field name [{signerField.FieldName}] not exist"));
                        }
                    }
                }
                IEnumerable<SignerField> actualReadOnlyFields = readOnlyFields.Where(x => x.TemplateId == document.TemplateId);
                foreach (SignerField readOnlyField in actualReadOnlyFields ?? Enumerable.Empty<SignerField>())
                {
                    if (fields.TextFields.FirstOrDefault(x => x.Name.ToLower() == readOnlyField.FieldName.ToLower() ||
                    x.Description.ToLower() == readOnlyField.FieldName.ToLower()) == null &&
                        fields.ChoiceFields.FirstOrDefault(x => x.Name.ToLower() == readOnlyField.FieldName.ToLower() ||
                        x.Description.ToLower() == readOnlyField.FieldName.ToLower()) == null &&
                        fields.CheckBoxFields.FirstOrDefault(x => x.Name.ToLower() == readOnlyField.FieldName.ToLower() ||
                        x.Description.ToLower() == readOnlyField.FieldName.ToLower()) == null &&
                        fields.SignatureFields.FirstOrDefault(x => x.Name.ToLower() == readOnlyField.FieldName.ToLower() ||
                        x.Description.ToLower() == readOnlyField.FieldName.ToLower()) == null &&
                        !RadioExistInFields(readOnlyField, fields))
                    {
                        throw new InvalidOperationException(ResultCode.FieldNameNotExist.GetNumericString(), new Exception($"Field name [{readOnlyField.FieldName}] not exist"));
                    }
                }

            }
        }

        private bool RadioExistInFields(SignerField signerField, PDFFields fields)
        {
            bool radioExist = false;
            foreach (RadioGroupField radioGroup in fields.RadioGroupFields ?? Enumerable.Empty<RadioGroupField>())
            {
                if (radioGroup.RadioFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower()) != null)
                {
                    radioExist = true;
                    break;
                }

            }

            return radioExist;

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

        private bool IsDocumentBelongToDocumentCollection(DocumentCollection dbDocumentCollection, DocumentCollection documentCollection)
        {

            return dbDocumentCollection.Documents.FirstOrDefault(x => x.Id == documentCollection.Documents.FirstOrDefault().Id) != null;



        }


        private void LoadPdfFields(int page, Document document)
        {
            _documentPdf.SetId(document.Id);

            document.Fields = _documentPdf.GetAllFields(page, page);


            foreach (SignatureField signatureField in document.Fields.SignatureFields)
            {
                IEnumerable<DocumentSignatureField> sigsDoc = _documentConnector.ReadSignatures(new Document { Id = document.Id });
                DocumentSignatureField sigDoc = sigsDoc.FirstOrDefault(x => x.FieldName == signatureField.Name);
                if (sigDoc != null)
                {
                    signatureField.Image = sigDoc.Image;
                }
            }
        }

        private void LoadPdfFields(int startPage, int endPage, Document document, DocumentStatus documentStatus)
        {
            _documentPdf.SetId(document.Id);
            document.Fields = _documentPdf.GetAllFields(startPage, endPage);
            Dictionary<Guid, IEnumerable<DocumentSignatureField>> signatureFieldForDoc = new Dictionary<Guid, IEnumerable<DocumentSignatureField>>();
            if (documentStatus == DocumentStatus.Signed || documentStatus == DocumentStatus.ExtraServerSigned)
            {
                foreach (SignatureField signatureField in document.Fields.SignatureFields)
                {

                    if (!signatureFieldForDoc.ContainsKey(document.Id))
                    {
                        signatureFieldForDoc.Add(document.Id, _documentConnector.ReadSignatures(new Document { Id = document.Id }));

                    }
                    IEnumerable<DocumentSignatureField> sigsDoc = signatureFieldForDoc[document.Id];
                    DocumentSignatureField sigDoc = sigsDoc.FirstOrDefault(x => x.FieldName == signatureField.Name);
                    if (sigDoc != null)
                    {
                        signatureField.Image = sigDoc.Image;
                    }
                }
            }
        }

        private async Task SendSignerLinks(DocumentCollection documentCollection, IEnumerable<SignerLink> links, User user, CompanyConfiguration companyConfiguration)
        {
            Configuration appConfiguration = await _configurationConnector.Read();
            if (documentCollection?.Mode == DocumentMode.Online || documentCollection?.Mode == DocumentMode.OrderedGroupSign)
            {
                Signer firstSigner = documentCollection.Signers?.ElementAt(0);
                await _documentCollectionOperations.SendDocumentLinkToSigner(documentCollection, links, user, companyConfiguration, appConfiguration, firstSigner, MessageType.BeforeSigning);

            }
            if (documentCollection?.Mode == DocumentMode.GroupSign)
            {
                await SendSignerLinksParallelly(documentCollection, links, user, companyConfiguration, appConfiguration);
            }
        }



        private async Task<PDFFields> ExportAllInfoFromDocumentCollection(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            DocumentCollection documents = await _documentCollectionConnector.Read(documentCollection);
            if (documents == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            if (!await IsDocumentCollectionBelongToUserGroup(documents, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            PDFFields fieldsResults = new PDFFields();

            IEnumerable<DocumentSignatureField> documentsSignatures = _documentConnector.ReadSignaturesByDocumentsId(documents.Documents.Select(x => x.Id).Distinct().ToList());
            foreach (Document document in documents.Documents)
            {
                _documentPdf.Load(document.Id);
                PDFFields fields = _documentPdf.GetAllFields();
                IEnumerable<DocumentSignatureField> signaturesInDocuemnt = documentsSignatures.Where(x => x.Id == document.Id);
                foreach (DocumentSignatureField sig in signaturesInDocuemnt.Where(sigField => !string.IsNullOrEmpty(sigField.Image)))
                {
                    SignatureField sigField = fields.SignatureFields.FirstOrDefault(x => sig.FieldName.ToLower() == x.Name.ToLower());
                    sigField.Image = sig?.Image;
                }
                fieldsResults.TextFields.AddRange(fields.TextFields);
                fieldsResults.RadioGroupFields.AddRange(fields.RadioGroupFields);
                fieldsResults.SignatureFields.AddRange(fields.SignatureFields);
                fieldsResults.CheckBoxFields.AddRange(fields.CheckBoxFields);
                fieldsResults.ChoiceFields.AddRange(fields.ChoiceFields);

            }

            return fieldsResults;
        }

        private byte[] CreateCSVForFieldExport(PDFFields fieldsResults)
        {

            StringBuilder result = new StringBuilder();
            // Create Headers
            result.Append($"Type,Name,Description,X,Y,Width,Height,Mandatory,Page,Value,SignatureType");
            result.AppendLine();
            foreach (TextField item in fieldsResults.TextFields ?? Enumerable.Empty<TextField>())
            {
                result.Append($"TextField,{item.Name},{item.Description},{item.X},{item.Y},{item.Width},{item.Height},{item.Mandatory},{item.Page},{item.Value},");
                result.AppendLine();
            }
            foreach (SignatureField item in fieldsResults.SignatureFields ?? Enumerable.Empty<SignatureField>())
            {
                result.Append($"SignatureField,{item.Name},{item.Description},{item.X},{item.Y},{item.Width},{item.Height},{item.Mandatory},{item.Page},{item.Image},{item.SigningType}");
                result.AppendLine();
            }
            foreach (ChoiceField item in fieldsResults.ChoiceFields ?? Enumerable.Empty<ChoiceField>())
            {
                result.Append($"ChoiceField,{item.Name},{item.Description},{item.X},{item.Y},{item.Width},{item.Height},{item.Mandatory},{item.Page},{item.SelectedOption},");
                result.AppendLine();
            }
            foreach (CheckBoxField item in fieldsResults.CheckBoxFields ?? Enumerable.Empty<CheckBoxField>())
            {
                result.Append($"CheckBoxField,{item.Name},{item.Description},{item.X},{item.Y},{item.Width},{item.Height},{item.Mandatory},{item.Page},{item.IsChecked},");
                result.AppendLine();
            }
            foreach (RadioGroupField group in fieldsResults.RadioGroupFields ?? Enumerable.Empty<RadioGroupField>())
            {
                result.Append($"RadioGroup,{group.Name},,,,,,,,{group.SelectedRadioName},");
                result.AppendLine();
                foreach (RadioField item in group.RadioFields ?? Enumerable.Empty<RadioField>())
                {
                    result.Append($"RadioButton,{item.Name},{item.Description},{item.X},{item.Y},{item.Width},{item.Height},{item.Mandatory},{item.Page},{item.Value},");
                    result.AppendLine();
                }
            }

            return Encoding.UTF8.GetBytes("\ufeff" + result.ToString());
        }

        private async Task SendSignerLinksParallelly(DocumentCollection documentCollection, IEnumerable<SignerLink> links, User user, CompanyConfiguration companyConfiguration, Configuration appConfiguration)
        {
            if (documentCollection != null && documentCollection.Signers != null)
            {
                foreach (Signer signer in documentCollection.Signers)
                {
                    try
                    {

                        string signerLink = links?.FirstOrDefault(x => x.SignerId == signer.Id).Link;

                        MessageInfo messageInfo = BuildMessageInfo(documentCollection, user, companyConfiguration, appConfiguration, signer, signerLink);

                        IMessageSender messageSender = _sendingMessageHandler.ExecuteCreation(signer.SendingMethod);
                        await messageSender.Send(appConfiguration, companyConfiguration, messageInfo);

                        // temporary fix - need to fix the update in document collection when sending multiple senders not in order  AKA GroupSign
                        // need to lock or find a solution for the callback after sent success -  it trying to update the same document multiple time.                        
                        await Task.Delay(70);

                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                            throw ex.InnerException;
                        throw;
                    }
                }
            }
        }


        private MessageInfo BuildMessageInfo(DocumentCollection documentCollection, User user, CompanyConfiguration companyConfiguration, Configuration appConfiguration, Signer signer, string signerLink)
        {
            string messageBefore = _configuration.GetBeforeMessage(user, appConfiguration, companyConfiguration);

            messageBefore = _documentCollectionOperations.UpdateMessage(signer.SendingMethod, messageBefore, signerLink, documentCollection.Name);
            MessageInfo messageInfo = new MessageInfo()
            {
                MessageType = MessageType.BeforeSigning,
                User = user,
                DocumentCollection = documentCollection,
                Contact = signer.Contact,
                MessageContent = messageBefore,
                Link = signerLink
            };
            return messageInfo;
        }

        private void SetValueToFields(Document document, PDFFields fields, IEnumerable<SignerField> signerFields)
        {
            IEnumerable<SignerField> templateSignerFields = signerFields.Where(x => x.TemplateId == document.TemplateId);
            foreach (SignerField signerField in templateSignerFields ?? Enumerable.Empty<SignerField>())
            {
                IEnumerable<TextField> textFields = fields.TextFields.Where(x => x.Name.ToLower() == signerField.FieldName.ToLower()
                || x.Description.ToLower() == signerField.FieldName.ToLower());
                if (textFields != null && textFields.Any() && !string.IsNullOrWhiteSpace(signerField.FieldValue))
                {
                    textFields.ForEach((x) => x.Value = signerField.FieldValue);
                    continue;
                }
                ChoiceField choiceField = fields.ChoiceFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower()
                || x.Description.ToLower() == signerField.FieldName.ToLower());
                if (choiceField != null && !string.IsNullOrWhiteSpace(signerField.FieldValue))
                {
                    choiceField.SelectedOption = signerField.FieldValue;
                    continue;
                }
                CheckBoxField checkBoxField = fields.CheckBoxFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower()
                || x.Description.ToLower() == signerField.FieldName.ToLower());
                if (checkBoxField != null && !string.IsNullOrWhiteSpace(signerField.FieldValue))
                {
                    bool.TryParse(signerField.FieldValue, out bool result);
                    checkBoxField.IsChecked = result;
                    continue;
                }
                RadioGroupField radioGroupField = fields.RadioGroupFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower());
                if (radioGroupField != null && !string.IsNullOrWhiteSpace(signerField.FieldValue))
                {
                    radioGroupField.SelectedRadioName = signerField.FieldValue;
                }
            }
        }

        private async Task UpdateDocumentsDeletionIn24Hours(IEnumerable<DocumentCollection> documentCollections)
        {
            if (documentCollections == null || !documentCollections.Any())
            {
                return;
            }
            Configuration appConfig = await _configurationConnector.Read();
            User user = documentCollections.FirstOrDefault()?.User;
            Company company = await _companyConnector.Read(new Company() { Id = user?.CompanyId ?? Guid.Empty });

            int signedInterval = _configuration.GetDocumentsDeletionInterval(appConfig, company, DocumentStatus.Signed);
            int unsignedInterval = _configuration.GetDocumentsDeletionInterval(appConfig, company, DocumentStatus.Sent);

            foreach (DocumentCollection documentCollection in documentCollections ?? Enumerable.Empty<DocumentCollection>())
            {
                if (documentCollection.DocumentStatus == DocumentStatus.Signed)
                {
                    int daysPassedSinceSigned = _dater.UtcNow().Subtract(documentCollection.SignedTime).Days;
                    documentCollection.IsWillDeletedIn24Hours = signedInterval > -1 && daysPassedSinceSigned > signedInterval;
                }
                else
                {
                    int daysPassedSinceCreation = _dater.UtcNow().Subtract(documentCollection.CreationTime).Days;
                    documentCollection.IsWillDeletedIn24Hours = unsignedInterval > -1 && daysPassedSinceCreation > unsignedInterval;
                }
            }
        }

        private int GetImagesCount(DocumentCollection documentCollection, Document document)
        {
            if (documentCollection.Mode == DocumentMode.SelfSign)
            {
                _documentPdf.SetId(document.Id);
                return _documentPdf.GetPagesCount();
            }

            _templatePdf.SetId(document.TemplateId);
            return _templatePdf.GetPagesCount();
        }


        private void SetIdInPdfHandler(DocumentCollection documentCollection, Document document)
        {
            if (documentCollection.Mode == DocumentMode.SelfSign)
            {
                _documentPdf.SetId(document.Id);

            }

            _templatePdf.SetId(document.TemplateId);

        }
        private PdfImage GetPdfImageByIndex(DocumentCollection documentCollection, int page)
        {
            if (documentCollection.Mode == DocumentMode.SelfSign)
            {
                return _documentPdf.GetPdfImageByIndex(page, _documentPdf.GetId());
            }
            return _templatePdf.GetPdfImageByIndex(page, _templatePdf.GetId());
        }

        private IList<PdfImage> GetPdfImages(DocumentCollection documentCollection, int startPage, int endPage)
        {
            if (documentCollection.Mode == DocumentMode.SelfSign)
            {
                return _documentPdf.GetPdfImages(startPage, endPage, _documentPdf.GetId());
            }
            return _templatePdf.GetPdfImages(startPage, endPage, _templatePdf.GetId());
        }

        private string DocumentsRowsToString(IList<IDictionary<string, object>> rows)
        {
            string delimiter = ",";
            StringBuilder result = new StringBuilder();
            foreach (var item in rows ?? Enumerable.Empty<IDictionary<string, object>>())
            {
                foreach (var innerItem in item ?? Enumerable.Empty<KeyValuePair<string, object>>())
                {
                    result.Append(innerItem.Value);
                    result.Append(delimiter);
                }
                result.AppendLine();
            }
            return result.ToString();
        }

        private IList<IDictionary<string, object>> GetCsvInfo(IEnumerable<DocumentCollection> documents, Language language)
        {
            if (!documents.Any())
            {
                return null;
            }
            int maxSigners = documents.Max(x => x.Signers.Count());
            IDictionary<string, object> header = GetCsvHeader(maxSigners, language);
            IList<IDictionary<string, object>> rows = new List<IDictionary<string, object>>() { header };
            AddCsvBody(documents, maxSigners, rows);
            return rows;
        }

        private void AddCsvBody(IEnumerable<DocumentCollection> documents, int maxSigners, IList<IDictionary<string, object>> rows)
        {
            foreach (DocumentCollection documentCollection in documents ?? Enumerable.Empty<DocumentCollection>())
            {
                IDictionary<string, object> row = new ExpandoObject();
                row.Add("Id", documentCollection.Id);
                row.Add("Name", Sanitize(documentCollection.Name));
                row.Add("Sender", Sanitize(documentCollection.User?.Name));
                row.Add("CreationTime", documentCollection.CreationTime);

                for (int i = 1; i <= maxSigners; i++)
                {
                    Signer signer = documentCollection.Signers.ElementAtOrDefault(i - 1);
                    row.Add($"SentTo{i}", Sanitize(signer?.Contact?.Name));
                    row.Add($"EmailPhone{i}", signer?.SendingMethod == SendingMethod.SMS ? Sanitize(signer?.Contact?.Phone, true) : signer?.SendingMethod == SendingMethod.Email ?
                        Sanitize(signer?.Contact?.Email) : "Tablet");
                    row.Add($"TimeSent{i}", signer?.TimeSent != DateTime.MinValue ? signer?.TimeSent : null);
                    row.Add($"TimeViewed{i}", signer?.TimeViewed != DateTime.MinValue ? signer?.TimeViewed : null);
                    row.Add($"TimeSigned{i}", signer?.TimeSigned != DateTime.MinValue ? signer?.TimeSigned : null);
                    row.Add($"TimeRejected{i}", signer?.TimeRejected != DateTime.MinValue ? signer?.TimeRejected : null);
                }
                rows.Add(row);
            }
        }

        private string Sanitize(string input, bool isPhone = false)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "";
            }
            if (input.StartsWith('=') || input.StartsWith('+') || input.StartsWith('-') || input.StartsWith('@'))
            {
                return "'" + input;
            }
            if (isPhone)
            {
                if (input.StartsWith('0'))
                {
                    return "'" + input + "'";
                }
            }

            return input;
        }
        private IDictionary<string, object> GetCsvHeader(int maxSigners, Language language)
        {


            IDictionary<string, object> header = new ExpandoObject();


            header.Add("Id", LanguageDictionary.languageDictionary[language]["id"]);
            header.Add("Document Name", LanguageDictionary.languageDictionary[language]["Document Name"]);
            header.Add("Sender", LanguageDictionary.languageDictionary[language]["Sender"]);
            header.Add("Creation Time", LanguageDictionary.languageDictionary[language]["Creation Time"]);

            for (int i = 1; i <= maxSigners; i++)
            {
                header.Add($"Sent To{i}", LanguageDictionary.languageDictionary[language]["Sent To"] + $"{i}");
                header.Add($"EmailPhone{i}", LanguageDictionary.languageDictionary[language]["EmailPhone"] + $"{i}");
                header.Add($"Time Sent{i}", LanguageDictionary.languageDictionary[language]["Time Sent"] + $"{i}");
                header.Add($"Time Viewed{i}", LanguageDictionary.languageDictionary[language]["Time Viewed"] + $"{i}");
                header.Add($"Time Signed{i}", LanguageDictionary.languageDictionary[language]["Time Signed"] + $"{i}");
                header.Add($"Time Rejected{i}", LanguageDictionary.languageDictionary[language]["Time Rejected"] + $"{i}");
            }

            return header;
        }

        private void FillTemplateFieldsMissingData(PDFFields templateFields, PDFFields debenuFields)
        {
            if (templateFields == null || debenuFields == null)
            {
                return;
            }


            var textFieldMap = templateFields.TextFields.Select(textField => new Tuple<string, TextField>(textField.Name, textField));

            foreach (TextField item in debenuFields.TextFields ?? Enumerable.Empty<TextField>())
            {

                if (textFieldMap.Any(x => x.Item1 == item.Name))
                {
                    item.TextFieldType = textFieldMap.FirstOrDefault(x => x.Item1 == item.Name)?.Item2?.TextFieldType ?? Common.Enums.PDF.TextFieldType.Text;
                    item.CustomerRegex = textFieldMap.FirstOrDefault(x => x.Item1 == item.Name)?.Item2?.CustomerRegex ?? "";

                }
            }

            //I didnt change this part because a signature field cannot have the same field name twice in the document.
            Dictionary<string, SignatureField> signatureFieldMap = templateFields.SignatureFields.ToDictionary(x => x.Name, x => x);
            foreach (SignatureField item in debenuFields.SignatureFields ?? Enumerable.Empty<SignatureField>())
            {
                if (signatureFieldMap.ContainsKey(item.Name))
                {
                    item.SigningType = signatureFieldMap[item.Name].SigningType;
                    item.SignatureKind = signatureFieldMap[item.Name].SignatureKind;
                }
            }
        }
        private DocumentCollectionAuditTrace DocumentCollectionToListString(DocumentCollection documentCollection)
        {
            DocumentCollectionAuditTrace documentCollectionAuditTrace = new()
            {
                CollectionId = documentCollection.Id,
                CollectionName = documentCollection.Name,
                CreationIp = documentCollection.SenderIP,
                CreationTime = documentCollection.CreationTime,
                UserEmail = documentCollection.User?.Email,
                UserPhone = documentCollection.User?.Phone ?? "",
                UserName = documentCollection.User?.Name
            };

            for (int i = 0; i < documentCollection.Signers.Count(); i++)
            {
                Signer signer = documentCollection.Signers.ElementAtOrDefault(i);
                AuditTraceSigner auditTraceSigner = new()
                {
                    Means = signer?.SendingMethod == SendingMethod.Email ? signer?.Contact?.Email : signer?.SendingMethod == SendingMethod.SMS ? signer?.Contact?.Phone : "Tablet",
                    Email = signer?.SendingMethod == SendingMethod.Tablet ? "Tablet" : signer?.Contact?.Email,
                    Phone = signer?.SendingMethod == SendingMethod.Tablet ? "Tablet" : signer?.Contact?.Phone,
                    SignedFromIpAddress = signer.IPAddress,
                    DeviceInformation = signer.DeviceInformation,
                    Name = signer?.Contact?.Name,
                    FirstViewIPAddress = signer.FirstViewIPAddress
                };
                OtpMode mode = signer.SignerAuthentication?.OtpDetails?.Mode ?? OtpMode.None;
                AuthMode authenticationMode = signer.SignerAuthentication?.AuthenticationMode ?? AuthMode.None;
                auditTraceSigner.DocumentPassword = (mode == OtpMode.PasswordRequired || mode == OtpMode.CodeAndPasswordRequired) ? "YES" : "NO";
                auditTraceSigner.DocumentOTP = (mode == OtpMode.CodeRequired || mode == OtpMode.CodeAndPasswordRequired) ? "YES" : "NO";
                auditTraceSigner.DocumentIDP = (authenticationMode == AuthMode.IDP || authenticationMode == AuthMode.ComsignVisualIDP) ? "YES" : "NO";
                auditTraceSigner.TimeLastSent = signer?.TimeLastSent ?? DateTime.MinValue;
                auditTraceSigner.TimeSent = signer?.TimeSent ?? DateTime.MinValue;
                auditTraceSigner.TimeSigned = signer?.TimeSigned ?? DateTime.MinValue;
                auditTraceSigner.TimeViewed = signer?.TimeViewed ?? DateTime.MinValue;
                auditTraceSigner.TimeRejected = signer?.TimeRejected ?? DateTime.MinValue;
                documentCollectionAuditTrace.AuditTraceSigners.Add(auditTraceSigner);
            }


            return documentCollectionAuditTrace;
        }

        private List<string> DocumentCollectionToListString(DocumentCollection documentCollection, int offset)
        {
            List<string> result = new List<string>
            {
                $"Id: {documentCollection.Id}",
                $"Name: {documentCollection.Name}",
                $"Sender: { documentCollection.User?.Name} {documentCollection.User?.Email}",
                $"CreationTime: {documentCollection.CreationTime.AddHours(offset)}    ( UTC TIME - {documentCollection.CreationTime})",
                $"Sender IP address: {documentCollection.SenderIP}"
            };

            for (int i = 0; i < documentCollection.Signers.Count(); i++)
            {
                Signer signer = documentCollection.Signers.ElementAtOrDefault(i);
                string signerMeans = signer?.SendingMethod == SendingMethod.Email ? signer?.Contact?.Email : signer?.Contact?.Phone;
                string ip = string.IsNullOrWhiteSpace(signer.IPAddress) ? string.Empty : $", IP : {signer.IPAddress}";
                string viewip = string.IsNullOrWhiteSpace(signer.FirstViewIPAddress) ? string.Empty : $", IP : {signer.FirstViewIPAddress}";
                string device = string.IsNullOrWhiteSpace(signer.DeviceInformation) ? string.Empty : $", Device : {signer.DeviceInformation}";
                result.Add("");
                result.Add($"SentTo{i + 1}: {signer?.Contact?.Name} {ip} {device}");
                result.Add(signerMeans);

                List<string> signerAuthRows = GetSignerAuthenticationRows(signer.SignerAuthentication);
                result.AddRange(signerAuthRows);

                if (signer?.TimeLastSent != null && signer?.TimeLastSent != DateTime.MinValue)
                {
                    result.Add($"TimeSent{i + 1}: {(signer.TimeLastSent).AddHours(offset)}    ( UTC TIME - {signer.TimeLastSent})");
                }
                if (signer?.TimeViewed != null && signer?.TimeViewed != DateTime.MinValue)
                {
                    result.Add($"TimeViewed{i + 1}: {((DateTime)signer?.TimeViewed).AddHours(offset)}    ( UTC TIME - {signer?.TimeViewed}) {viewip}");
                }
                if (signer?.TimeSigned != null && signer?.TimeSigned != DateTime.MinValue)
                {
                    result.Add($"TimeSigned{i + 1}: {((DateTime)signer?.TimeSigned).AddHours(offset)}    ( UTC TIME - {signer?.TimeSigned})");
                }
                if (signer?.TimeRejected != null && signer?.TimeRejected != DateTime.MinValue)
                {
                    result.Add($"TimeRejected{i + 1}: {((DateTime)signer?.TimeRejected).AddHours(offset)}    ( UTC TIME - {signer?.TimeRejected})");
                }
            }

            return result;
        }


        private async Task<DocumentCollection> ReadAndValidateDocumentCollectionGroup(DocumentCollection documentCollection)
        {
            DocumentCollection dbDocumentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (dbDocumentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            if (!await IsDocumentCollectionBelongToUserGroup(dbDocumentCollection, false))
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }
            return dbDocumentCollection;
        }


        private List<string> GetSignerAuthenticationRows(SignerAuthentication signerAuthentication)
        {
            List<string> rows = new List<string>();
            OtpMode mode = signerAuthentication.OtpDetails?.Mode ?? OtpMode.None;
            AuthMode authenticationMode = signerAuthentication.AuthenticationMode;


            switch (mode)
            {
                case OtpMode.None:
                    rows.Add($"Document password: NO");
                    rows.Add($"OTP: NO");
                    break;
                case OtpMode.PasswordRequired:
                    rows.Add($"Document password: YES");
                    rows.Add($"OTP: NO");
                    break;
                case OtpMode.CodeRequired:
                    rows.Add($"Document password: NO");
                    rows.Add($"OTP: YES");
                    break;
                case OtpMode.CodeAndPasswordRequired:
                    rows.Add($"Document password: YES");
                    rows.Add($"OTP: YES");
                    break;

            }

            switch (authenticationMode)
            {
                case AuthMode.None:
                    rows.Add($"IDP: NO");
                    break;
                case AuthMode.IDP:
                    rows.Add($"IDP: YES");
                    break;
                case AuthMode.ComsignVisualIDP:
                    rows.Add($"IDP: YES");
                    break;
            }

            return rows;
        }

        private void AssignAllFieldsToSingleSigner(DocumentCollection documentCollection, Document document)
        {
            IEnumerable<SignerField> singleSignerFields = documentCollection.Signers.FirstOrDefault().SignerFields;
            _templatePdf.Load(document.TemplateId);
            _templatePdf.TextFields.ForEach(x => singleSignerFields = singleSignerFields.Concat(new List<SignerField> { new SignerField { TemplateId = document.TemplateId, FieldName = x.Name } }));
            _templatePdf.CheckBoxFields.ForEach(x => singleSignerFields = singleSignerFields.Concat(new List<SignerField> { new SignerField { TemplateId = document.TemplateId, FieldName = x.Name } }));
            _templatePdf.ChoiceFields.ForEach(x => singleSignerFields = singleSignerFields.Concat(new List<SignerField> { new SignerField { TemplateId = document.TemplateId, FieldName = x.Name } }));
            _templatePdf.RadioGroupFields.ForEach(x =>
            {
                x.RadioFields.ForEach(
                    y =>
                    {
                        singleSignerFields = singleSignerFields.Concat(new List<SignerField> { new SignerField { TemplateId = document.TemplateId, FieldName = y.Name } });
                    });
            }
            );
            _templatePdf.SignatureFields.ForEach(x => singleSignerFields =
            string.IsNullOrWhiteSpace(x.Image) ?
            singleSignerFields.Concat(new List<SignerField> { new SignerField { TemplateId = document.TemplateId, FieldName = x.Name } }) :
            singleSignerFields);

            documentCollection.Signers.FirstOrDefault().SignerFields = singleSignerFields;
        }

        private void RemoveSignedAttchmentIfNotAttchedInSignedDoc(List<DocumentCollection> docs)
        {
            List<DocumentCollection> docDoCheck = docs.Where(x => x.DocumentStatus == DocumentStatus.Signed).ToList();
            docDoCheck.AsParallel().ForEach(x =>
            {
                x.Signers.ForEach(y =>
                {
                    if (y.SignerAttachments.Any())
                    {
                        if (!_fileWrapper.Signers.IsAttachmentExist(y))

                        {
                            docs.FirstOrDefault(t => x.Id == t.Id).Signers.FirstOrDefault(q => q.Id == y.Id).SignerAttachments = new List<SignerAttachment>();
                        }
                    }
                });

            });

        }

        private async Task ValidateDownloadAttchments(DocumentCollection documentCollection)
        {
            (User user, _) = await _users.GetUser();

            if (documentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            if (documentCollection.GroupId != user.GroupId)
            {
                throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
            }

        }
        private void CleanFieldsFromCollectionFromCache(DocumentCollection documentCollection)
        {
            foreach (Document doc in documentCollection.Documents)
            {
                _templatePdf.SetId(doc.TemplateId);

            }

            foreach (Document doc in documentCollection.Documents)
            {
                _documentPdf.SetId(doc.Id);
            }
        }

        private async Task GetCallBackUrl(DocumentCollection documentCollection, User user)
        {
            CompanyConfiguration documentCompanyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId }) ??
                new CompanyConfiguration();

            if (documentCompanyConfiguration.ShouldSendDocumentNotifications)
            {
                documentCollection.CallbackUrl = documentCompanyConfiguration.DocumentNotificationsEndpoint;
            }

        }

        public async Task DeleteBatch(RecordsBatch documentBatch)
        {
            (User user, _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);

            IEnumerable<DocumentCollection> documentsCollections = _documentCollectionConnector.ReadCollectionsInList(documentBatch.Ids);
            if (!documentsCollections.Any())
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            Dictionary<Guid, string> collectionIds = new Dictionary<Guid, string>();
            foreach (DocumentCollection docCollection in documentsCollections)
            {
                if (!((user.GroupId != Guid.Empty && docCollection.GroupId == user.GroupId) ||
                  (user.Id != Guid.Empty && docCollection.UserId == user.Id && docCollection.Mode == DocumentMode.SelfSign)))
                {
                    throw new InvalidOperationException(ResultCode.DocumentNotBelongToUserGroup.GetNumericString());
                }
                collectionIds.Add(docCollection.Id, docCollection.Name);
            }

            _logger.Information("User {UserId}: {UserEmail} Deleted batch of documents collection {CollectionIds}", user.Id, user.Email, collectionIds);
            foreach (DocumentCollection docCollection in documentsCollections)
            {
                await _documentCollectionConnector.UpdateStatus(docCollection, DocumentStatus.Deleted);
            }

        }

        private async Task UpdateDoucmentReactivated(DocumentCollection documentCollection, List<Guid> signersId)
        {
            await _documentCollectionConnector.ReactivateCollection(documentCollection, signersId);
            documentCollection.DocumentStatus = DocumentStatus.Sent;

            //await _documentCollectionConnector.Update(documentCollection);
        }

        #endregion
    }
}