using Common.Enums.Documents;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.MessageSending.Sms;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Settings;
using Common.Models.Sms;
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

namespace Common.Handlers.SendingMessages.SMS
{
    public class SmsTelemessageHandler : ISmsProvider
    {
        private ILogger _logger;
        private IOptions<GeneralSettings> _generalSetting;
        private IEncryptor _encryptor;
    
        private IServiceScopeFactory _scopeFactory;
        private IHttpClientFactory _httpClientFactory;
        private readonly string _telemessageAPIUrl = @"https://secure.telemessage.com/jsp/receiveSMS.jsp";

        public SmsTelemessageHandler(ILogger logger, IOptions<GeneralSettings> generalSetting, IEncryptor encryptor,  IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _generalSetting = generalSetting;
            _encryptor = encryptor;
        
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
        }

        public void SendAsync(Sms smsInfo, SmsConfiguration configuration, DocumentCollection documentCollection = null)
        {

            var phone = smsInfo.Phones.FirstOrDefault().Replace("+","");

            Task.Run(async () =>
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {

                    

                    string request = $"{_telemessageAPIUrl}?userid={configuration.User}&password={ _encryptor.Decrypt(configuration.Password)}&to={phone}&text={smsInfo.Message}";
                    var response = await httpClient.GetAsync(request);
                    string jsonStringResponse = await response.Content.ReadAsStringAsync();
                    _logger.Debug("SmsTelemessageHandler- Send SMS - \n{@smsInfo}  \nResult - {jsonStringResponse}", smsInfo, jsonStringResponse);
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();

                        if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            await SendDocSuccessfully(smsInfo, documentCollection, dependencyService);
                        }
                        else
                        {
                            await SendDocFailed(smsInfo, documentCollection, dependencyService);
                        }
                    }
                    

            }
            });
        }

        public void SendBatchAsync(List<Tuple<Sms, DocumentCollection>> smsInfo, SmsConfiguration configuration)
        {
            foreach(var item in smsInfo)
            {
                SendAsync(item.Item1, configuration, item.Item2);
            }
        }

        private async Task SendDocFailed(Sms smsInfo, DocumentCollection documentCollection, IDocumentCollectionConnector dependencyService)
        {
            if (documentCollection != null)
            {
                await dependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed);
            }
            _logger.Error("SmsTelemessageHandler - Failed to sent SMS to {PhoneNumbers}", smsInfo.Phones);
        }

        private async Task SendDocSuccessfully(Sms smsInfo, DocumentCollection documentCollection, IDocumentCollectionConnector dependencyService)
        {
            if (documentCollection != null)
            {

                await dependencyService.UpdateStatus(documentCollection,
                documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : documentCollection.DocumentStatus == DocumentStatus.ExtraServerSigned ? DocumentStatus.ExtraServerSigned : DocumentStatus.Sent);
                if (documentCollection.DocumentStatus != DocumentStatus.Signed && documentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
                {
                    var signer = documentCollection.Signers?.FirstOrDefault(x => (x.Contact?.Phone == smsInfo?.Phones.First()
                    || x.Contact?.PhoneExtension + x.Contact?.Phone == smsInfo?.Phones.First()) && x.SendingMethod == SendingMethod.SMS);
                    await dependencyService.UpdateSignerSendingTime(documentCollection, signer);
                }
            }
            _logger.Debug("SmsTelemessageHandler - Successfully sent SMS to {PhoneNumbers}", smsInfo.Phones);
        }
    }
}
