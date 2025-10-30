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
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.SMS
{
    public class SmsCenterHandler : ISmsProvider
    {
        private readonly ILogger _logger;
        private readonly IEncryptor _encryptor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private string apiUrl = "https://www.smscenter.co.il/";
        public SmsCenterHandler(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor, 
            IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _encryptor = encryptor;
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
        }

        public void SendBatchAsync(List<Tuple<Sms, DocumentCollection>> smsInfo, SmsConfiguration configuration)
        {
            foreach (var pair in smsInfo)
            {
                SendAsync(pair.Item1, configuration, pair.Item2);
            }
        }

        public void SendAsync(Sms smsInfo, SmsConfiguration configuration, DocumentCollection documentCollection = null)
        {
            Task.Run(async () =>
            {
                try
                {
                    SmsCenterModel smsModel = GetSmsModel(smsInfo, configuration);
                    using (var httpClient = _httpClientFactory.CreateClient())
                    {
                        var body = BuildFormUrlEncodedString(smsModel);
                        var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                        var response = await httpClient.PostAsync($"{apiUrl}Web/WebServices/SendMessage.asmx/SendMessagesV2", content);
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                            if (response.IsSuccessStatusCode)
                            {
                                if (documentCollection != null)
                                {
                                    SendDocSuccessfully(smsInfo, documentCollection, dependencyService);
                                }
                                else
                                {
                                    string jsonStringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                    _logger.Warning("Send SMS via Center SMS first attempt failed trying again result : {jsonStringResponse}", jsonStringResponse);
                                    System.Threading.Thread.Sleep(3000);
                                    response = await httpClient.PostAsync($"{apiUrl}Web/WebServices/SendMessage.asmx/SendMessagesV2", content);
                                    if (documentCollection != null)
                                    {
                                        if (response.IsSuccessStatusCode)
                                        {
                                            SendDocSuccessfully(smsInfo, documentCollection, dependencyService);
                                        }
                                        else
                                        {
                                            jsonStringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                            _logger.Warning("Send SMS via Center SMS second attempt failed result : {jsonStringResponse}", jsonStringResponse);
                                            SendDocFailed(smsInfo, documentCollection, dependencyService);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error in SmsCenterHandler - {newLine}{ExceptionMessage", Environment.NewLine, ex.ToString());
                    throw;
                }
            });
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
            _logger.Debug("SmsCenterHandler - Successfully sent SMS to {PhoneNumbers}", smsInfo.Phones);
        }

        private void SendDocFailed(Sms smsInfo, DocumentCollection documentCollection, IDocumentCollectionConnector dependencyService)
        {
            dependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed).GetAwaiter().GetResult();
            _logger.Error("SmsCenterHandler - Failed to send SMS to {PhoneNumbers}", smsInfo.Phones);
        }

        private string BuildFormUrlEncodedString(SmsCenterModel request)
        {
            var sb = new StringBuilder();

            foreach (var prop in request.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var name = WebUtility.UrlEncode(prop.Name);
                var value = WebUtility.UrlEncode(prop.GetValue(request)?.ToString() ?? "");
                sb.Append($"{name}={value}&");
            }

            if (sb.Length > 0)
                sb.Length--; // Remove trailing '&'

            return sb.ToString();
        }


        private SmsCenterModel GetSmsModel(Sms smsInfo, SmsConfiguration configuration)
        {
            return new SmsCenterModel
            {
                UserName = configuration.User,
                Password = _encryptor.Decrypt(configuration.Password),
                SenderName = configuration.From,
                SendToPhoneNumbers = string.Join(",", smsInfo.Phones.Select(p => p.StartsWith("+972") ? p.Substring(4) : p)),
                CCToEmail = "",
                Message = smsInfo.Message,
                SMSOperation = SmsOperation.Push,
                DeliveryDelayInMinutes = 0,
                ExpirationDelayInMinutes = 0,
                MessageOption = MessageOption.Regular,
                Price = 0
            };
        }

        internal class SmsCenterModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string SenderName { get; set; }
            public string SendToPhoneNumbers { get; set; }
            public string CCToEmail { get; set; }
            public string Message { get; set; }
            public SmsOperation SMSOperation { get; set; }
            public int DeliveryDelayInMinutes { get; set; }
            public int ExpirationDelayInMinutes { get; set; }
            public MessageOption MessageOption { get; set; }
            public double Price { get; set; }
        }

        internal enum SmsOperation 
        { 
            Push,
            Pull,
            ReverseBilling
        }

        internal enum MessageOption 
        { 
            Regular,
            Concatenated,
            Reply
        }
    }
}
