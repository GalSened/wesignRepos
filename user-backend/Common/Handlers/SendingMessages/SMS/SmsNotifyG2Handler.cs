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
using RabbitMQ.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.SMS
{
    public class SmsNotifyG2Handler : ISmsProvider
    {
        private ILogger _logger;
        private GeneralSettings _generalSettings;
        private IEncryptor _encryptor;
   
        private IServiceScopeFactory _scopeFactory;
        private IHttpClientFactory _httpClientFactory;
        private EnvironmentExtraInfo _environmentExtraInfo;

        public SmsNotifyG2Handler(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor,  IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory,
            IOptions<EnvironmentExtraInfo> environmentExtraInfo)
        {
            
            _logger = logger;
            _generalSettings = generalSettings.Value;
            _encryptor = encryptor;

            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _environmentExtraInfo = environmentExtraInfo.Value;
        }

        public void SendAsync(Sms smsInfo, SmsConfiguration configuration, DocumentCollection documentCollection = null)
        {
            NotifyG2Request notifyG2Request = new NotifyG2Request()
            {
                applicationId = _encryptor.Decrypt(configuration.Password),
                recipient = smsInfo.Phones.FirstOrDefault().Replace("+972", ""),
                recipientType = 0,
                smsMessageText = smsInfo.Message,
                smsSenderName = configuration.From,
                tagName = "Wesign"
            };
            Task.Run(async () =>
            {
                try
                {
                    Dictionary<string, string> headers = GetHeadersInfo();

                    using var httpClient = _httpClientFactory.CreateClient();
                    foreach (var pair in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                    }
                    string json = JsonConvert.SerializeObject(notifyG2Request);
                    StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(_generalSettings.NotifySmsApiEndpoint, httpContent);


                    string jsonStringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var payCallResult = JsonConvert.DeserializeObject<NotifyG2Response>(jsonStringResponse);
                    _logger.Debug("SmsNotifyG2Handler - Send SMS - \n{@smsInfo}  \nResult - {jsonStringResponse}", smsInfo, jsonStringResponse);
                    if (documentCollection != null)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                            if (payCallResult?.StatusCode != 0)
                            {
                                await dependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed);
                                _logger.Error("SmsNotifyG2Handler - Failed to send SMS to {PhoneNumbers}, Result = [{jsonStringResponse}]", smsInfo.Phones.FirstOrDefault(), jsonStringResponse);
                            }
                            else
                            {
                                await dependencyService.UpdateStatus(documentCollection,
                                     documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : documentCollection.DocumentStatus == DocumentStatus.ExtraServerSigned ? DocumentStatus.ExtraServerSigned : DocumentStatus.Sent);
                                if (documentCollection.DocumentStatus != DocumentStatus.Signed && documentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
                                {
                                    var signer = documentCollection.Signers?.FirstOrDefault(x => (x.Contact?.Phone == smsInfo?.Phones.First()
                                        || x.Contact?.PhoneExtension + x.Contact?.Phone == smsInfo?.Phones.First()) && x.SendingMethod == SendingMethod.SMS);
                                    await dependencyService.UpdateSignerSendingTime(documentCollection, signer);
                                }
                                _logger.Debug("SmsNotifyG2Handler - Successfully sent SMS to {PhoneNumbers} , Response [{jsonStringResponse}]", smsInfo.Phones.FirstOrDefault(), jsonStringResponse);
                            }
                        };
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Faild to send SMS NotifyG2");
                }
            });

    
        }

        public void SendBatchAsync(List<Tuple<Sms, DocumentCollection>> smsInfo, SmsConfiguration configuration)
        {
            foreach (var pair in smsInfo)
            {
                SendAsync(pair.Item1, configuration, pair.Item2);
            }
        }


        private Dictionary<string, string> GetHeadersInfo()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            if (_environmentExtraInfo != null && _environmentExtraInfo.NotifyHeaders != null && _environmentExtraInfo.NotifyHeaders.Count > 0)
            {
                foreach (var header in _environmentExtraInfo.NotifyHeaders)
                {
                    headers.Add(header.Name, _encryptor.Decrypt(header.Value));
                }
            }


            return headers;

        }

    }


    public class NotifyG2Request
    {
        public string applicationId { get; set; }
        public string smsMessageText { get; set; }
        public string smsSenderName { get; set; }
        public string recipient { get; set; }
        public int recipientType { get; set; }
        public string tagName { get; set; }


    }
    public class NotifyG2Response
    {
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string MessageId { get; set; }
    }
}
