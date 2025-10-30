using Common.Enums.Documents;
using Common.Extensions;
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
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.SMS

{
    public class SmsPayCallHandler : ISmsProvider
    {
        private const string SMS = "SMS";

        private readonly ILogger _logger;
        private readonly IEncryptor _encryptor;
        private readonly GeneralSettings _generalSetting;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public SmsPayCallHandler(ILogger logger, IOptions<GeneralSettings> generalSetting, IEncryptor encryptor, 
            IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _encryptor = encryptor;
            _generalSetting = generalSetting.Value;
            _httpClientFactory = httpClientFactory;
            _scopeFactory = scopeFactory;
        }


        public void SendBatchAsync(List<Tuple<Sms, DocumentCollection>> smsInfo, SmsConfiguration configuration)
        {

            // for now send one by one
            foreach (var pair in smsInfo)
            {
                SendAsync(pair.Item1, configuration, pair.Item2);
            }
        }
        public void SendAsync(Sms smsInfo, SmsConfiguration configuration, DocumentCollection documentCollection = null)
        {
            var isGlobalNumber = smsInfo.Phones.FirstOrDefault(x => !x.Contains("+972") && x.Contains("+")) != null;
            IEnumerable<string> recipients = !isGlobalNumber ? smsInfo.Phones.Select(p => p.Replace("+972", "")) : smsInfo.Phones;
            var httpClient = _httpClientFactory.CreateClient();
            var smsModal = new SmsPayCallModel
            {
                User = new PayCallUser
                {
                    Username = configuration.User,
                    Password = _encryptor.Decrypt(configuration.Password)
                },
                send = new List<PayCallSend>{
                    new PayCallSend
                    {
                        Recipients = recipients,
                        Content = new PayCallContent { Message = smsInfo.Message, Type = SMS },
                        Settings = new PayCallSettings { Sender = configuration.From, international = isGlobalNumber? 1 : 0 }
                    }
                }
            };
            Task.Run(async () =>
            {
                string json = JsonConvert.SerializeObject(smsModal);
                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_generalSetting.PayCallSmsApiEndpoint, httpContent);               
                string jsonStringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                httpClient.Dispose();
                var payCallResult = JsonConvert.DeserializeObject<PayCallResult>(jsonStringResponse);
                _logger.Debug("SmsPayCallHandler - Send SMS - \n{@smsInfo}  \nResult - {jsonStringResponse}", smsInfo, jsonStringResponse);
                if (documentCollection != null)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                        if (payCallResult?.success?.ToLower() == "false")
                        {
                            await dependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed);
                            _logger.Error("SmsPayCallHandler - Failed to send SMS to {PhoneNumbers}, Result = [{jsonStringResponse}]", smsInfo.Phones, jsonStringResponse);
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
                            _logger.Debug("SmsPayCallHandler - Successfully sent SMS to {PhoneNumbers} using PayCall, Response [{jsonStringResponse}]", smsInfo.Phones, jsonStringResponse);
                        }
                    };
                }
            }
            );
        }       
    }


    public class SmsPayCallModel
    {
        public PayCallUser User { get; set; }
        public IEnumerable<PayCallSend> send { get; set; }
    }

    public class PayCallUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class PayCallSend
    {
        public PayCallContent Content { get; set; }
        public IEnumerable<string> Recipients { get; set; }
        public PayCallSettings Settings { get; set; }
    }

    public class PayCallContent
    {
        public string Message { get; set; }
        public string Type { get; set; }
    }

    public class PayCallSettings
    {
        public string Sender { get; set; }
        public int international { get; set; }

    }

    public class PayCallResult
    {
        public string success { get; set; }
        public string Message { get; set; }
        public int smsCount { get; set; }
    }

}
