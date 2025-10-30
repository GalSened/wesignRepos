using Common.Enums.Documents;
using Common.Interfaces;
using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces.DB;
using Common.Models.Documents;

namespace Common.Handlers
{
    public class HttpDocumentCollectionOperationsNotifier : IDocumentCollectionOperationsNotifier
    {
        private readonly ILogger _logger;
        private readonly IDater _dater;
        private readonly IJson _json;
        
        private readonly GeneralSettings _generalSettings;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ICompanyConnector _companyConnector;

        public HttpDocumentCollectionOperationsNotifier(ILogger logger, IDater dater, IJson json,ICompanyConnector companyConnector, IOptions<GeneralSettings> generalSettings, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _dater = dater;
            _json = json;            
            _generalSettings = generalSettings.Value;
            _clientFactory = clientFactory;
            _companyConnector = companyConnector;


        }

        public async Task AddNotification(DocumentCollection documentCollection, DocumentNotification notifiaction, Signer signer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentCollection?.CallbackUrl))
                {
                    return;
                }
                DocumentOperationNotification documentOperationNotification = await GetDocumentExtraInfoModel(documentCollection, notifiaction);

                



                foreach (var document in documentCollection?.Documents ?? Enumerable.Empty<Document>())
                {
                    documentOperationNotification.TemplatesIds.Add(document?.TemplateId.ToString() ?? "");
                }


                if (documentOperationNotification.NotificationType != DocumentNotification.DocumentDeleted && signer != null)
                {
                    documentOperationNotification.SignerId = signer.Id;
                    documentOperationNotification.SignerName = signer.Contact.Name;
                    if (_generalSettings.AddNotificationExtraInfo)
                    {
                        (documentOperationNotification as DocumentOperationNotificationExtraInfo).ContactId = signer.Contact.Id;
                        (documentOperationNotification as DocumentOperationNotificationExtraInfo).ContactEmail = signer.Contact.Email;
                        (documentOperationNotification as DocumentOperationNotificationExtraInfo).ContactPhone = signer.Contact.Phone;

                    }
                    if (documentOperationNotification.NotificationType == DocumentNotification.DocumentRejeceted)
                    {
                        documentOperationNotification.SignerMessage = signer.Notes?.SignerNote;
                    }

                }
                else
                {
                    documentOperationNotification.UserId = documentCollection.User.Id;
                    documentOperationNotification.UserName = documentCollection.User.Name;
                }
                _logger.Debug("Trying to post notification {@DocumentNotification} to {DocumentCollectionCallBackUrl}", documentCollection.CallbackUrl, documentOperationNotification);

                StringContent stringContent = new StringContent(_generalSettings.AddNotificationExtraInfo ?
                    _json.Serialize((documentOperationNotification as DocumentOperationNotificationExtraInfo)) : _json.Serialize(documentOperationNotification), Encoding.UTF8, "application/json");

                using (var httpClient = _clientFactory.CreateClient())
                {
                    var data = await httpClient.PostAsync(documentCollection.CallbackUrl, stringContent); // change to using

                    if (!data.IsSuccessStatusCode)
                    {
                        _logger.Warning("Failed to post notification to {DocumentCollectionCallbackUrl}, error code {ErrorCode}, content {@Content}", documentCollection.CallbackUrl, data.StatusCode, data.Content);

                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to post notification to {DocumentCollectionCallbackUrl}", documentCollection.CallbackUrl);
            }

        }

        private async Task<DocumentOperationNotification> GetDocumentExtraInfoModel(DocumentCollection documentCollection, DocumentNotification notifiaction)
        {
            if (_generalSettings.AddNotificationExtraInfo)
            {
                return new DocumentOperationNotificationExtraInfo()
                {
                    NotificationType = notifiaction,
                    DocumentCollectionId = documentCollection.Id,
                    DocumentStatus = documentCollection.DocumentStatus,
                    DocumentName = documentCollection.Name,
                    OccuranceTimeStamp = _dater.UtcNow(),
                    CompanyId = documentCollection.User.CompanyId,
                    CompanyName = (await _companyConnector.Read(new Company() { Id = documentCollection.User.CompanyId }))?.Name,
                    GroupId = documentCollection.GroupId,
                    UserId = documentCollection.User.Id,
                    UserName = documentCollection.User.Name,
                    
                    
                };
            }


            return  new DocumentOperationNotification()
            {
                NotificationType = notifiaction,
                DocumentCollectionId = documentCollection.Id,
                DocumentStatus = documentCollection.DocumentStatus,
                DocumentName = documentCollection.Name,
                OccuranceTimeStamp = _dater.UtcNow(),
                CompanyId = documentCollection.User.CompanyId,
                CompanyName = (await _companyConnector.Read(new Company() { Id = documentCollection.User.CompanyId }))?.Name,
                UserId = documentCollection.User.Id,
                 UserName= documentCollection.User.Name
                 
            };
        }
    }
}
