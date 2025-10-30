
namespace Common.Handlers.SendingMessages.SMS
{
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
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using static System.Formats.Asn1.AsnWriter;

    public class SmsGoldmanHandler : ISmsProvider
    {
        
        private readonly ILogger _logger;
        private readonly GeneralSettings _generalSetting;
        private readonly IEncryptor _encryptor;
     
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private string apiUrl = "https://api.icrm.co.il/";

        public SmsGoldmanHandler(ILogger logger, IOptions<GeneralSettings> generalSettings,
            IEncryptor encryptor, IServiceScopeFactory scopeFactory,IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _generalSetting = generalSettings.Value;
            _encryptor = encryptor;
       
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
        }



        public void SendBatchAsync(List<Tuple<Sms, DocumentCollection>> smsInfo, SmsConfiguration configuration)
        {
            
            foreach(var tuple in smsInfo)
            {
                SendAsync(tuple.Item1, configuration, tuple.Item2);
            }
        }

        public void SendAsync(Sms smsInfo, SmsConfiguration configuration, DocumentCollection documentCollection = null)
        {
           
            Task.Run(async () =>
            {
                try
                {
                    SmsGoldmanModel smsModel = GetSMSMpdel(smsInfo, configuration);
                    using (var httpClient = _httpClientFactory.CreateClient())
                    {
                        string json = JsonConvert.SerializeObject(smsModel);
                        StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync($"{apiUrl}BasicSMS/SendSMS", httpContent);
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                            if (response.IsSuccessStatusCode)
                            {
                                if (documentCollection != null)
                                {
                                    SendDocSuccessfully(smsInfo, documentCollection, dependencyService);
                                }
                            }
                            else
                            {
                                string jsonStringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                _logger.Warning("Send SMS via Goldman SMS first attempt failed trying again result : {jsonStringResponse}", jsonStringResponse);
                                System.Threading.Thread.Sleep(3000);
                                response = await httpClient.PostAsync($"{apiUrl}BasicSMS/SendSMS", httpContent);
                                if (documentCollection != null)
                                {
                                    if (response.IsSuccessStatusCode)
                                    {
                                        SendDocSuccessfully(smsInfo, documentCollection, dependencyService);
                                    }
                                    else
                                    {
                                        jsonStringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                        _logger.Warning("Send SMS via Goldman SMS second attempt failed result : {jsonStringResponse}", jsonStringResponse);
                                        SendDocFailed(smsInfo, documentCollection, dependencyService);
                                    }
                                }



                            }
                        }

                    }

                    
                }
                catch (Exception ex)
                {
                    _logger.Error("Error in SmsGoldmanHandler - {newLine}{ExceptionMessage", Environment.NewLine, ex.ToString());
                    throw;
                }
            });
        }

        private SmsGoldmanModel GetSMSMpdel(Sms smsInfo, SmsConfiguration configuration)
        {
            return new SmsGoldmanModel
            {
                customer_transaction_id = "",
                customer_transaction_reference = "",
                sender = configuration.From,
                dlr_url = "",
                end_flash_sms = false,
                msg_lang = "UNICODE",
                password = _encryptor.Decrypt(configuration.Password),
                username = configuration.User,
                send_on_date = "",
                sms_msg = smsInfo.Message,
                mobiles = smsInfo.Phones.Select(x => x.Replace("+972", "")).ToArray()
            };
        }

        private void SendDocFailed(Sms smsInfo, DocumentCollection documentCollection, IDocumentCollectionConnector dependencyService)
        {
            dependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed).GetAwaiter().GetResult();
            _logger.Error("SmsGoldmanHandler - Failed to send SMS to {PhoneNumbers}", smsInfo.Phones);
        }

        private void SendDocSuccessfully(Sms smsInfo, DocumentCollection documentCollection, IDocumentCollectionConnector dependencyService)
        {
            dependencyService.UpdateStatus(documentCollection,
            documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : documentCollection.DocumentStatus == DocumentStatus.ExtraServerSigned ? DocumentStatus.ExtraServerSigned : DocumentStatus.Sent).GetAwaiter().GetResult();
            if (documentCollection.DocumentStatus != DocumentStatus.Signed && documentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
            {
                var signer = documentCollection.Signers?.FirstOrDefault(x => (x.Contact?.Phone == smsInfo?.Phones.First()
                || x.Contact?.PhoneExtension + x.Contact?.Phone == smsInfo?.Phones.First()) && x.SendingMethod == SendingMethod.SMS);
                dependencyService.UpdateSignerSendingTime(documentCollection, signer).GetAwaiter().GetResult();
            }
            _logger.Debug("SmsGoldmanHandler -  Successfully sent SMS to {PhoneNumbers}", smsInfo.Phones);

        }

        
     

      
    }

       

        internal class SmsGoldmanModel
        {
            public string username { get; set; }
            public string password { get; set; }
            public string sender { get; set; }
            public string msg_lang { get; set; } = "UNICODE";
            public string sms_msg { get; set; }
            public bool end_flash_sms { get; set; } = false;
            public string dlr_url { get; set; } 
            public string send_on_date { get; set; }
            public string customer_transaction_id { get; set; }
            public string customer_transaction_reference { get; set; }
            public string[] mobiles { get; set; }
        }
}
