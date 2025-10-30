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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace Common.Handlers.SendingMessages.SMS
{
    public class SmsInforUMobileHandler : ISmsProvider
    {
        private readonly ILogger _logger;
        private readonly IEncryptor _encryptor;
        
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GeneralSettings _generalSetting;
        private readonly string _inforUAPIUrl = @"https://api.inforu.co.il/SendMessageXml.ashx";

        public SmsInforUMobileHandler(ILogger logger, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _generalSetting = generalSettings.Value;
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
            try
            {
                var inforURequest = GetInforuRequest(smsInfo, configuration);
                var result = SendRequest(inforURequest);
                
                _logger.Debug("SmsInforUMobileHandler - Send SMS - \n{@smsInfo}  \nResult - {@result}", smsInfo, result);
                if (documentCollection != null)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                        if (result?.Status == 1)
                        {
                            dependencyService.UpdateStatus(documentCollection,
                                documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : documentCollection.DocumentStatus == DocumentStatus.ExtraServerSigned ? DocumentStatus.ExtraServerSigned : DocumentStatus.Sent).GetAwaiter().GetResult();
                            if (documentCollection.DocumentStatus != DocumentStatus.Signed && documentCollection.DocumentStatus != DocumentStatus.ExtraServerSigned)
                            {
                                var signer = documentCollection.Signers?.FirstOrDefault(x => (x.Contact?.Phone == smsInfo?.Phones.First()
                                    || x.Contact?.PhoneExtension + x.Contact?.Phone == smsInfo?.Phones.First()) && x.SendingMethod == SendingMethod.SMS);
                                dependencyService.UpdateSignerSendingTime(documentCollection, signer).GetAwaiter().GetResult();
                            }
                            _logger.Debug("SmsInforUMobileHandler - Successfully sent SMS to [{PhoneNumbers}]", smsInfo.Phones);
                        }
                        else
                        {
                            dependencyService.UpdateStatus(documentCollection, documentCollection.DocumentStatus == DocumentStatus.Signed ? DocumentStatus.Signed : DocumentStatus.SendingFailed).GetAwaiter().GetResult();
                            _logger.Error("SmsInforUMobileHandler - Failed to sent SMS to {PhoneNumbers}, Result = [@result]", smsInfo.Phones, result);
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in InforUSMSHandler :");
                throw;
            }
        }

        private InfroResult SendRequest(string inforURequest)
        {

            string szResult = string.Empty;
            WebRequest Request = WebRequest.Create(_inforUAPIUrl);
            Request.Timeout = 30000;
            Request.Method = "POST";
            Request.ContentType = "application/x-www-form-urlencoded";

            inforURequest = inforURequest.Replace(" ", "+");
            var PostBuffer = Encoding.UTF8.GetBytes(inforURequest);
            Request.ContentLength = PostBuffer.Length;

            using (Stream RequestStream = Request.GetRequestStream())
            {
                //Write the POST data
                RequestStream.Write(PostBuffer, 0, PostBuffer.Length);
                RequestStream.Close();
                var Response = Request.GetResponse();
                using (StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.UTF8))
                {
                    szResult = sr.ReadToEnd();

                }
            }
            XmlSerializer serializer = new XmlSerializer(typeof(InfroResult), new XmlRootAttribute("Result"));
            using StringReader stringReader = new StringReader(szResult);
            return (InfroResult)serializer.Deserialize(stringReader); 
        }

        private string GetInforuRequest(Sms smsInfo, SmsConfiguration configuration)
        {
            string phonesList = "";

            foreach (var phone in smsInfo.Phones)
            {
                if (phonesList != "")
                {
                    phonesList += ";";
                }
                phonesList += phone;

            }
            var sbXml = new StringBuilder();
            sbXml.Append("<Inforu>");
            sbXml.Append("<User>");
            sbXml.Append("<Username>" + configuration.User + "</Username>");
            sbXml.Append("<ApiToken>" + _encryptor.Decrypt(configuration?.Password) + "</ApiToken>");
            sbXml.Append("</User>");
            sbXml.Append("<Content Type=\"sms\">");
            sbXml.Append("<Message>" + System.Security.SecurityElement.Escape(smsInfo.Message) + "</Message>");
            sbXml.Append("</Content>");
            sbXml.Append("<Recipients>");
            sbXml.Append("<PhoneNumber>" + phonesList + "</PhoneNumber>");
            sbXml.Append("</Recipients>");
            sbXml.Append("<Settings>");
            sbXml.Append("<Sender>" + configuration.From + "</Sender>");
            sbXml.Append("<MessageInterval>0</MessageInterval>");
            sbXml.Append("<TimeToSend></TimeToSend>");
            sbXml.Append("</Settings>");
            sbXml.Append("</Inforu >");
            return "InforuXML=" + HttpUtility.UrlEncode(sbXml.ToString(), System.Text.Encoding.UTF8);
        }

        
    }

    public class InfroResult
    {
        public int Status { get; set; }
        public string Description { get; set; }
        public string NumberOfRecipients { get; set; }
    }
}


