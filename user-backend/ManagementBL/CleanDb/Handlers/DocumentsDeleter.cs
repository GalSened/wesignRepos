using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Interfaces.PDF;
using Common.Interfaces.RabbitMQ;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Files.PDF;
using Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Twilio.Jwt;

namespace ManagementBL.CleanDb.Handlers
{
    public class DocumentsDeleter : IDeleter
    {
        private GeneralSettings _generalSettings;
        private RabbitMQSettings _rabbitMqSettings;

        private readonly ILogger _logger;
        private readonly IEncryptor _encryptor;
        private readonly IDocumentPdf _documentPdf;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFilesWrapper _filesWrapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _historyDocumentsRoute;
        private IRabbitConnector _rabbitConnector;
        private readonly IDocumentCollectionOperationsNotifier _documentCollectionOperationsNotifier;
        public DocumentsDeleter(IOptions<GeneralSettings> generalSettings, IOptions<RabbitMQSettings> rabbitSettings, ILogger logger, IDater dater, IDocumentPdf documentPdf, IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            IFilesWrapper filesWrapper, IHttpClientFactory httpClientFactory, IRabbitConnector rabbitConnector,
            IEncryptor encryptor, IDocumentCollectionOperationsNotifier documentCollectionOperationsNotifier)
        {
            _historyDocumentsRoute = "/documentcollections";
            _generalSettings = generalSettings.Value;
            _rabbitMqSettings = rabbitSettings.Value;
            _logger = logger;
            _encryptor = encryptor;
            _documentPdf = documentPdf;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _filesWrapper = filesWrapper;
            _httpClientFactory = httpClientFactory;
            _rabbitConnector = rabbitConnector;
            if (_generalSettings.UseRabbitMQForHistoryDocuments)
            {
                rabbitConnector.InIt(null, _rabbitMqSettings.DocumentsCollectionQueueName);
            }

            _documentCollectionOperationsNotifier = documentCollectionOperationsNotifier;
        }



        public async Task<bool> DeleteProcess()
        {
            try
            {

                Dictionary<Guid, string> companyNames = new Dictionary<Guid, string>();
                using var scope = _scopeFactory.CreateScope();
                IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                int skip = 0;
                int totalCount = 0;
                int batchSize = 250;
                var appConfiguration = await _configuration.ReadAppConfiguration();
                do
                {
                    List<Company> companies = companyConnector.Read(string.Empty, skip, batchSize, null, out totalCount).ToList();
                    foreach (var company in companies)
                    {
                        try
                        {
                            if(!companyNames.ContainsKey(company.Id))
                            {
                                companyNames.Add(company.Id, company.Name);
                            }
                            int unsignedInterval = _configuration.GetDocumentsDeletionInterval(appConfiguration, company, DocumentStatus.Sent);
                            int signedInterval = _configuration.GetDocumentsDeletionInterval(appConfiguration, company, DocumentStatus.Signed);
                            IEnumerable<DocumentCollection> docToDelete = documentCollectionConnector.ReadDocumentsCollectionToDeleteByInterval(company, signedInterval, unsignedInterval);
                            if (docToDelete.Any())
                            {
                                await DeleteDocumentsFromFsAndDb(docToDelete, companyNames, true);
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Failed to clean deleted documents from company {CompanyId}", company.Id);
                            throw;
                        }


                    }
                    skip += batchSize;
                } while (skip <= totalCount);

                IEnumerable<DocumentCollection> documentCollections = documentCollectionConnector.ReadDeletedCollections();
                if (documentCollections.Any())
                {
                    await DeleteDocumentsFromFsAndDb(documentCollections, companyNames, false);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to clean deleted documents");
            }


            return true;
        }



        private async Task DeleteDocumentsFromFsAndDb(IEnumerable<DocumentCollection> documents, Dictionary<Guid, string> companyNames, bool isDeletedByDateInterval)
        {
            _logger.Debug("In DeleteDocumentsFromFsAndDb ");

          

            
            var configuration = await _configuration.ReadAppConfiguration();
            string encriptedHistoryApiKey = _encryptor.Decrypt(configuration.HistoryIntegratorServiceAPIKey);
            bool shouldSendDeletedDocsToHistory = !string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceURL) && !string.IsNullOrWhiteSpace(encriptedHistoryApiKey);
            foreach (var documentCollection in documents)
            {
                try
                {
                    _logger.Debug("In DeleteDocumentsFromFsAndDb - doc id [{DocumentCollectionId}] name [{DocumentCollectionName}]", documentCollection.Id, documentCollection.Name);
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                        if (!companyNames.ContainsKey(documentCollection.User.CompanyId))
                        {
                            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                            var company = await companyConnector.Read(new Company() { Id = documentCollection.User.CompanyId });
                            companyNames.Add(company.Id, company.Name);
                        }
                        await dependencyService.Delete(documentCollection, DeleteDocumentCollectionFromFS);


                        if (isDeletedByDateInterval)
                        {
                            await _documentCollectionOperationsNotifier.AddNotification(documentCollection, DocumentNotification.DocumentDeleted, null);
                        }
                        if (_generalSettings.UseRabbitMQForHistoryDocuments || shouldSendDeletedDocsToHistory)
                          { 
                            var deletedDoc = CastToDeletedDocumentCollection(documentCollection, scope, companyNames[documentCollection.User.CompanyId]);
                            deletedDoc.User.CompanyName = companyNames[documentCollection.User.CompanyId];
                            if (_generalSettings.UseRabbitMQForHistoryDocuments)
                            {
                                await _rabbitConnector.SendMessage(deletedDoc);
                            }
                            else  // if (shouldSendDeletedDocsToHistory)
                            {
                                await SendDeletedDocumentCollectionToHistoryAPI(deletedDoc, configuration.HistoryIntegratorServiceURL, encriptedHistoryApiKey);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to clean DocCollection {DocumentCollectionId} name [{DocumentCollectionName}]", documentCollection.Id, documentCollection.Name);
                }
            }
        }

        private async Task SendDeletedDocumentCollectionToHistoryAPI(DeletedDocumentCollection deletedDoc, string historyApiUrl, string historyApiKey)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                // Serialize the DeletedDocumentCollection object to JSON
                var json = JsonConvert.SerializeObject(deletedDoc);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("AppKey",  historyApiKey);
                // Post the request
                var response = await client.PostAsync($"{string.Concat(historyApiUrl, _historyDocumentsRoute)}", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error(ResultCode.FailedWriteDeletedDocInMongo.GetNumericString());
                    var errorContent = await response.Content.ReadAsStringAsync();
                }
            }
        }

        private void DeleteDocumentCollectionFromFS(DocumentCollection documentsCollecteion)
        {
            DeleteAppendices(documentsCollecteion);
            DeleteAttachments(documentsCollecteion);

            foreach (var document in documentsCollecteion.Documents)
            {
                _documentPdf.SetId(document.Id);
                _documentPdf.Delete();
            }
        }

        private void DeleteAttachments(DocumentCollection documentsCollecteion)
        {
            _filesWrapper.Documents.DeleteAttachments(documentsCollecteion);
        }

        private void DeleteAppendices(DocumentCollection documentsCollecteion)
        {
            _filesWrapper.Documents.DeleteAppendices(documentsCollecteion);


        }

        #region DeletedModelsExtensions
        private DeletedDocumentCollection CastToDeletedDocumentCollection(DocumentCollection documentCollection, IServiceScope scope, string companyName)
        {
            var deletedDocCollection = new DeletedDocumentCollection()
            {
                Id = documentCollection.Id,
                UserId = documentCollection.UserId,
                GroupId = documentCollection.GroupId,
                DistributionId = documentCollection.DistributionId,
                DocumentStatus = documentCollection.DocumentStatus,
                CreationTime = documentCollection.CreationTime,
                User = new DeletedDocumentUser
                {
                   
                    CompanyId = documentCollection.User?.CompanyId ?? Guid.Empty,
                    CompanyName = companyName,
                    Email = documentCollection.User?.Email 

                }
            };


            ITemplateConnector dependencyService = scope.ServiceProvider.GetService<ITemplateConnector>();
            List<Guid> documentsId = documentCollection.Documents?.Select(x => x.TemplateId).Distinct().ToList();
            List<Template> templates = new List<Template>();
            if (documentsId != null && documentsId.Any())
            {
                templates = dependencyService.ReadWithFieldsData(documentsId).ToList();
                
            }


            if (documentCollection.Documents != null)
            {
                deletedDocCollection.Documents = documentCollection.Documents.Select(doc => CastToDeletedDocument(companyName, doc, templates.FirstOrDefault(x => x.Id == doc.TemplateId))).ToList();
            }
            return deletedDocCollection;
        }

        private DeletedDocument CastToDeletedDocument(string companyName, Document document, Template template)
        {
            if (document == null)
                return null;
            return new DeletedDocument(document, CastToDeletedDocumentTemplate( companyName, template));
        }

        private DeletedDocumentTemplate CastToDeletedDocumentTemplate(string companyName, Template template)
        {
            if (template == null )
                return null;

            DeletedDocumentTemplate deletedDocTemplate = new DeletedDocumentTemplate();
            deletedDocTemplate.TemplateId = template.Id;

            if (template.Fields != null)
            {
        
                foreach (var field in template.Fields.SignatureFields ?? Enumerable.Empty<SignatureField>())
                {
                    deletedDocTemplate.TemplateSignatureFields.Add(CastToDeletedTemplateSignatureField(companyName, field));
                }
            }
            
            return deletedDocTemplate;
        }

        private DeletedTemplateSignatureField CastToDeletedTemplateSignatureField(string companyName, SignatureField signatureField)
        {
            if (signatureField == null)
                return null;
            return new DeletedTemplateSignatureField()
            {
                CompanyName = companyName,
                SignatureFieldType = signatureField.SigningType
            };
        }
        #endregion
    }
}
