using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Models.License;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers
{
    public class LMSWrapperConnectorServiceHandler : ILMSWrapperConnectorService
    {
        private LMSWraperSettings _lmsSettings;
        private IHttpClientFactory _clientFactory;
        private IEncryptor _encryptor;
        private ILogger _logger;
        protected const string MEDIA_TYPE = "application/json";

        public LMSWrapperConnectorServiceHandler(IOptions<LMSWraperSettings> lmsSettings, IHttpClientFactory clientFactory, IEncryptor encryptor,
              ILogger logger)
        {
            _lmsSettings = lmsSettings.Value;
            _clientFactory = clientFactory;
            _encryptor = encryptor;
            _logger = logger;
        }



        public async Task<string> GetURLForChangePaymentRule(LmsUserAction lmsUserAction)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(_lmsSettings.URL))
                {
                    throw new InvalidOperationException(ResultCode.PaymentServiceIsNotActive.GetNumericString());
                }
                string token = await GetToken();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new Exception();
                }

                using (var httpClient = _clientFactory.CreateClient())
                {

                    string content = JsonConvert.SerializeObject(lmsUserAction).ToString();
                    StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage serverResponse = await httpClient.PostAsync($"{_lmsSettings.URL}/WSE/ChangePaymentRule", stringContent);
                    string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                    if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                        serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        _logger.Error("failed to get url form lms for change payment rule.");
                    }
                    else
                    {
                        var successResponse = JsonConvert.DeserializeObject<ChangePaymentRuleResponse>(resposnsString);
                      
                        return successResponse.Url;
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.Error(ex, "failed to check user in LMS service to check if payment is still active");
            }
            return "";

        }


        public async Task<bool> CheckUser(LmsUserAction unsubscribeUser)
        {
            try
            {
                

                if (string.IsNullOrWhiteSpace( _lmsSettings.URL))
                {

                    return true;
                }

                string token = await GetToken();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new Exception();
                }

                using (var httpClient = _clientFactory.CreateClient())
                {

                    string content = JsonConvert.SerializeObject(unsubscribeUser).ToString();
                    StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage serverResponse = await httpClient.PostAsync($"{_lmsSettings.URL}/WSE/CheckUser", stringContent);
                    string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                    if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                        serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        _logger.Error("failed to check user {LMSUserId} in LMS.", unsubscribeUser.UserID);
                    }
                    else
                    {
                        _logger.Debug("user Is OK and can work with WSE if he have license ");
                        return true;
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.Error(ex, "failed to check user in LMS service to check if payment is still active");
            }
            return false;
        }
        public async Task<bool> UnsubscribeUser(LmsUserAction unsubscribeUser)
        {
            string token = await GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception();
            }

            using (var httpClient = _clientFactory.CreateClient())
            {

                string content = JsonConvert.SerializeObject(unsubscribeUser).ToString();
                StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage serverResponse = await httpClient.PostAsync($"{_lmsSettings.URL}/WSE/UnsubscribeUser", stringContent);
                string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                    serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                {
                    _logger.Error("failed to unsubscribe payment process {Response}", resposnsString);
                }
                else
                {
                    _logger.Debug("user {UnsubcribeUserId} unsubscribe successfully ", unsubscribeUser.UserID);
                    return true;
                }
            }

            return false;
        }

        private async Task<string> GetToken()
        {
            string jwt = "";
            using (var httpClient = _clientFactory.CreateClient())
            {
                var loginRequest = new WrapprtServiceLoginModel
                {
                    User = _encryptor.Decrypt(_lmsSettings.Key1),
                    Password = _encryptor.Decrypt(_lmsSettings.Key2)

                };
                string content = JsonConvert.SerializeObject(loginRequest).ToString();
                StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);
                HttpResponseMessage serverResponse = await httpClient.PostAsync($"{_lmsSettings.URL}/auth", stringContent);
                string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                    serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                {
                    
                    _logger.Error("failed to login to LMSWrapper {ResponseString} in url {LmsUrl}", resposnsString, _lmsSettings.URL);

                    
                }
                if (serverResponse.StatusCode == HttpStatusCode.OK)
                {
                    _logger.Debug("in LMS Wrapper get token done successfully");
                    var successResponse = JsonConvert.DeserializeObject<WrapprtServiceTokensResponse>(resposnsString);
                    jwt = successResponse.Token;
                }

            }
            return jwt;
        }

        private sealed class WrapprtServiceLoginModel
        {
            public string User { get; set; }
            public string Password { get; set; }

        }

        private sealed class WrapprtServiceTokensResponse
        {
            public string Token { get; set; }

        }

        private sealed class ChangePaymentRuleResponse
        {
            public string Url { get; set; }
        }


    }

    
}
