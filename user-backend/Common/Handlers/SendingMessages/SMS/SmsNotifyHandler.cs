using Common.Interfaces.DB;
using Common.Interfaces;
using Common.Interfaces.MessageSending.Sms;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Settings;
using Common.Models.Sms;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using Microsoft.Extensions.Options;
using NotifySms;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Common.Enums.Documents;
using System.Data.Common;
using Common.Enums.Results;
using Common.Extensions;

namespace Common.Handlers.SendingMessages.SMS
    
{
    public class SmsNotifyHandler : ISmsProvider
    {
        private readonly ILogger _logger;
        private readonly GeneralSettings _generalSettings;
        private readonly IEncryptor _encryptor;
    
        private readonly IServiceScopeFactory _scopeFactory;

        public SmsNotifyHandler(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor,  IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _generalSettings = generalSettings.Value;
            _encryptor = encryptor;
    
            _scopeFactory = scopeFactory;
        }
        public void SendAsync(Sms smsInfo, SmsConfiguration configuration, DocumentCollection documentCollection = null)
        {
            NotifyApiServiceClient client = CreateNotifyClient();
            AbstractSingleSmsRequest request = getSMSRequest(smsInfo, configuration);
            Task.Run(async () =>
            {
                try
                {
                    SingleSmsResponse result = await client.SendSingleSmsMessageAsync(request);
                    _logger.Debug("NotifySMSHandler- Send SMS - \n{@smsInfo} \n Result - {@result}", smsInfo, result);

                    if (result.Status != MessageStatus.SmsSent && result.Status != MessageStatus.SentByParentQuota)
                    {
                        _logger.Warning("Send doc via NotifySMSHandler first attempt failed trying again \nresult : {result}", result);
                        System.Threading.Thread.Sleep(3000);
                        result = await client.SendSingleSmsMessageAsync(request);
                    }
                    if (documentCollection != null)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                            if (result.Status == MessageStatus.SmsSent || result.Status == MessageStatus.SentByParentQuota)
                            {
                                await SendDocSuccessfully(smsInfo, documentCollection, dependencyService);
                            }
                            else
                            {

                                await SendDocFailed(smsInfo, documentCollection, dependencyService);
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
              
                    _logger.Error("Error in NotifySMSHandler - {newLine}{ExceptionString}", Environment.NewLine, ex.ToString());
                    throw;
                }
            });
        }

        public void SendBatchAsync(List<Tuple<Sms, DocumentCollection>> smsInfo, SmsConfiguration configuration)
        {
            // for now send one by one
            foreach (var pair in smsInfo)
            {
                SendAsync(pair.Item1, configuration, pair.Item2);
            }
        }

        private AbstractSingleSmsRequest getSMSRequest(Sms smsInfo, SmsConfiguration configuration)
        {
            Guid appId = Guid.NewGuid();
            if (!Guid.TryParse(configuration.User, out appId))
            {
                throw new InvalidOperationException(ResultCode.InvalidSMSUserNameOrPassword.GetNumericString());
            }
            
            AbstractSingleSmsRequest singleSmsRequest = new SingleSmsRequest()
            {
                ApplicationID = appId,
                PhoneNumber = smsInfo.Phones.FirstOrDefault().Replace("+972", ""),
                SmsMessageText = smsInfo.Message,
                SmsSenderName = configuration.From,
            };
            return singleSmsRequest;
        }

        private async Task SendDocFailed(Sms smsInfo, DocumentCollection documentCollection, IDocumentCollectionConnector dependencyService)
        {
            await dependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed);
            _logger.Error("NotifySMSHandler - Failed to send SMS to {PhoneNumbers}", smsInfo.Phones);
        }

        private async Task SendDocSuccessfully(Sms smsInfo, DocumentCollection documentCollection, IDocumentCollectionConnector dependencyService)
        {
         await   dependencyService.UpdateStatus(documentCollection,
            documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : documentCollection.DocumentStatus == DocumentStatus.ExtraServerSigned ? DocumentStatus.ExtraServerSigned : DocumentStatus.Sent);
            if (documentCollection.DocumentStatus != DocumentStatus.Signed && documentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
            {
                var signer = documentCollection.Signers?.FirstOrDefault(x => (x.Contact?.Phone == smsInfo?.Phones.First()
                || x.Contact?.PhoneExtension + x.Contact?.Phone == smsInfo?.Phones.First()) && x.SendingMethod == SendingMethod.SMS);
                await dependencyService.UpdateSignerSendingTime(documentCollection, signer);
            }
            _logger.Debug("NotifySMSHandler - Successfully sent SMS to {PhoneNumbers}", smsInfo.Phones);
        }

        private NotifyApiServiceClient CreateNotifyClient()
        {
            var notifyClient = new NotifyApiServiceClient(NotifyApiServiceClient.EndpointConfiguration.BasicHttpBinding_INotifyApiService, _generalSettings.NotifySmsApiEndpoint);
            return notifyClient;
        }
    }
}
