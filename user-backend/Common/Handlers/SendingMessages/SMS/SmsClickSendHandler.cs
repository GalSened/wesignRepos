using Common.Enums.Documents;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.MessageSending.Sms;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Settings;
using Common.Models.Sms;
using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.ClickSend.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Handlers.SendingMessages.SMS
{
    public class SmsClickSendHandler : ISmsProvider
    {
        private readonly ILogger _logger;
        private readonly IEncryptor _encryptor;
        private readonly IServiceScopeFactory _scopeFactory;

        public SmsClickSendHandler(ILogger log, IOptions<GeneralSettings> generalSetting, IEncryptor encryptor, IServiceScopeFactory scopeFactory)
        {
            _logger = log;
            _encryptor = encryptor;
            _scopeFactory = scopeFactory;
        }


        public void SendBatchAsync(List<System.Tuple<Sms, DocumentCollection>> smsInfo, SmsConfiguration configuration)
        {

            // for now send one by one
            foreach (var pair in smsInfo)
            {
                SendAsync(pair.Item1, configuration, pair.Item2);
            }
        }

        public void SendAsync(Sms smsInfo, SmsConfiguration smsConfiguration, DocumentCollection documentCollection = null)
        {
            var configuration = new IO.ClickSend.Client.Configuration()
            {
                Username = smsConfiguration.User,
                //API_KEY
                Password = _encryptor.Decrypt(smsConfiguration.Password),

            };
            var smsApi = new SMSApi(configuration);
            var listOfSms = new List<SmsMessage>();
            smsInfo.Phones.ForEach(phone =>
            {
                listOfSms.Add(new SmsMessage(from: smsConfiguration.From, to: phone, body: smsInfo.Message));
            });
            Send(smsInfo, smsApi, listOfSms, documentCollection);
        }

     

      

        private void Send(Sms smsInfo, SMSApi smsApi, List<SmsMessage> listOfSms, DocumentCollection documentCollection)
        {
            var smsCollection = new SmsMessageCollection(listOfSms);
            var response = smsApi.SmsSendPost(smsCollection);
            dynamic jsonObj = JsonConvert.DeserializeObject(response);

            _logger.Debug("SmsClickSendHandler - Message to {PhoneNumbers} using ClickSend, Response [{response}]", smsInfo.Phones, response);
            if (documentCollection != null)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    IDocumentCollectionConnector documentCollectionConnectordependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                    
                    var dynamicStatus = jsonObj["data"]["messages"][0]["status"];                    
                    string status = Convert.ToString(dynamicStatus);
                    string result = Convert.ToString(jsonObj["response_code"]);
                    if (!result.ToUpper().Contains("SUCCESS") || (result.ToUpper().Contains("SUCCESS") && status.ToUpper() == "INSUFFICIENT_CREDIT"))
                    {
                        documentCollectionConnectordependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed).GetAwaiter().GetResult() ;
                        _logger.Error("SmsClickSendHandler - Failed to sent SMS to {PhoneNumbers} using ClickSend, Result = [{response}]", smsInfo.Phones, response);
                    }
                    else
                    {
                        documentCollectionConnectordependencyService.UpdateStatus(documentCollection,
                            documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : documentCollection.DocumentStatus == DocumentStatus.ExtraServerSigned ? DocumentStatus.ExtraServerSigned : DocumentStatus.Sent).GetAwaiter().GetResult();
                        if (documentCollection.DocumentStatus != DocumentStatus.Signed && documentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
                        {
                            var signer = documentCollection.Signers?.FirstOrDefault(x => (x.Contact?.Phone == smsInfo?.Phones.First()
                                || x.Contact?.PhoneExtension + x.Contact?.Phone == smsInfo?.Phones.First()) && x.SendingMethod == SendingMethod.SMS);
                            documentCollectionConnectordependencyService.UpdateSignerSendingTime(documentCollection, signer).GetAwaiter().GetResult();
                        }
                        _logger.Debug("SmsClickSendHandler - Successfully sent SMS to {PhoneNumbers} using ClickSend, Response [{response}]", smsInfo.Phones, response);
                    }
                }
            }

        }
    }
}
