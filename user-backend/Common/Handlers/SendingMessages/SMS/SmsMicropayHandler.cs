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
using System.Web;

namespace Common.Handlers.SendingMessages.SMS
{
    class SmsMicropayHandler : ISmsProvider
    {
        private const string SMS = "SMS";

        private readonly ILogger _logger;
        private readonly IEncryptor _encryptor;
        private readonly GeneralSettings _generalSetting;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _micropayAPIUrl = @"https://www.micropay.co.il/extApi/scheduleSms.php";

        public SmsMicropayHandler(ILogger logger, IOptions<GeneralSettings> generalSetting, IEncryptor encryptor, 
            IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _encryptor = encryptor;
            _generalSetting = generalSetting.Value;
            _httpClientFactory = httpClientFactory;
            _scopeFactory = scopeFactory;
        }

        public void SendAsync(Sms smsInfo, SmsConfiguration configuration, DocumentCollection documentCollection = null)
        {
            try
            {
                var micropayRequest = GetMicropayRequest(smsInfo, configuration);



                Task.Run(async () =>
                {

                    try
                    {
                        using (var httpClient = _httpClientFactory.CreateClient())
                        {



                            var result = await httpClient.GetAsync($"{micropayRequest}");
                            var response = await result.Content.ReadAsStringAsync();



                            _logger.Debug("SmsIMicropayHandler - Send SMS -request {Request}\n{@smsInfo}\nResult - {@response}", micropayRequest, smsInfo, response);


                            if (documentCollection != null)
                            {
                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                                    if (!response.ToLower().Contains("error"))
                                    {
                                        await dependencyService.UpdateStatus(documentCollection,
                                            documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : documentCollection.DocumentStatus == DocumentStatus.ExtraServerSigned ? DocumentStatus.ExtraServerSigned : DocumentStatus.Sent);
                                        if (documentCollection.DocumentStatus != DocumentStatus.Signed && documentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
                                        {
                                            var signer = documentCollection.Signers?.FirstOrDefault(x => (x.Contact?.Phone == smsInfo?.Phones.First()
                                                || x.Contact?.PhoneExtension + x.Contact?.Phone == smsInfo?.Phones.First()) && x.SendingMethod == SendingMethod.SMS);
                                            await dependencyService.UpdateSignerSendingTime(documentCollection, signer);
                                        }

                                        _logger.Debug("SmsInforUMobileHandler - Successfully sent SMS to {PhoneNumbers}", smsInfo.Phones);
                                    }
                                    else
                                    {
                                        await dependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed);
                                        _logger.Error("SmsInforUMobileHandler - Failed to sent SMS to {PhoneNumbers}, Result = [{@result}]", smsInfo.Phones, result);
                                    }
                                };
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failure in sending SMS process using SNS++ ");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in InforUSMSHandler :");
                throw;
            }
        }

        private string GetMicropayRequest(Sms smsInfo, SmsConfiguration configuration)
        {

            IEnumerable<string> recipients = smsInfo.Phones.Select(p => p.Replace("+972", ""));

            string phonesList = "";

            foreach (var phone in recipients)
            {
                if (phonesList != "")
                {
                    phonesList += ",";
                }
                phonesList += phone;

            }

            return $"{_micropayAPIUrl}?get=1&token={configuration?.User}{_encryptor.Decrypt(configuration?.Password)}&msg={HttpUtility.UrlEncode(smsInfo.Message, Encoding.UTF8)}&list={phonesList}&from={configuration.From}";
        }

        public void SendBatchAsync(List<Tuple<Sms, DocumentCollection>> smsInfo, SmsConfiguration configuration)
        {
            // for now send one by one
            foreach (var pair in smsInfo)
            {
                SendAsync(pair.Item1, configuration, pair.Item2);
            }
        }
    }
}
