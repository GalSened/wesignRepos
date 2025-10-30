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
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace Common.Handlers.SendingMessages.SMS
{
    class SmsTwilioHandler : ISmsProvider
    {

        private ILogger _logger;
        private IOptions<GeneralSettings> _generalSetting;
        private IEncryptor _encryptor;
      
        private IServiceScopeFactory _scopeFactory;

        public SmsTwilioHandler(ILogger logger, IOptions<GeneralSettings> generalSetting, IEncryptor encryptor, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _generalSetting = generalSetting;
            _encryptor = encryptor;
           
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

            string accountSid = configuration.User;
            string authToken = _encryptor.Decrypt(configuration.Password);

            TwilioClient.Init(accountSid.Trim(), authToken.Trim());

            foreach (var phone in smsInfo.Phones)
            {
                var message = MessageResource.Create(
                    body: smsInfo.Message,
                    messagingServiceSid: configuration.From.Trim(),
                    //   from: new Twilio.Types.PhoneNumber(configuration.From),  //"+15017122661" // 
                    to: new Twilio.Types.PhoneNumber(phone)//"+15558675310")
                );
                _logger.Debug("Message to {Phone} using Twilio Sid = [{Sid}], Status= [{MessageStatus}]", phone, message.Sid, message.Status);
                if (message.Status.ToString().ToLower() != "failed" && message.ErrorCode == null && string.IsNullOrWhiteSpace(message.ErrorMessage) )
                {
                    if (documentCollection != null)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                            {
                                dependencyService.UpdateStatus(documentCollection,
                                    documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : documentCollection.DocumentStatus == DocumentStatus.ExtraServerSigned ? DocumentStatus.ExtraServerSigned : DocumentStatus.Sent).GetAwaiter().GetResult();
                                if (documentCollection.DocumentStatus != DocumentStatus.Signed && documentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
                                {
                                    var signer = documentCollection.Signers?.FirstOrDefault(x => x.Contact?.Phone != null && phone.Contains(x.Contact?.Phone) && x.SendingMethod == SendingMethod.SMS);
                                    dependencyService.UpdateSignerSendingTime(documentCollection, signer).GetAwaiter().GetResult();
                                }
                     
                            }
                        }
                    }
                    _logger.Debug("SmsTwilioHandler - Successfully sent SMS to {PhoneNumbers}", smsInfo.Phones);
                }
                else if (message.ErrorCode != null && !string.IsNullOrWhiteSpace(message.ErrorMessage) && documentCollection != null)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        if (documentCollection != null)
                        {
                            IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                            dependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed).GetAwaiter().GetResult();
                        }

                        _logger.Error("SmsTwilioHandler - Failed to sent SMS to {PhoneNumbers}, ErrorCode - [{MessageErrorCode}], ErrorMessage - [{ErrorMessage}]", smsInfo.Phones, message.ErrorCode, message.ErrorMessage);
                    }
                }
            }

        }
    }


}
