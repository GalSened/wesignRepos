using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Oauth;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SignerBL.Handlers
{
    public class VisualIdentityHandler : IVisualIdentity
    {
        private readonly ILogger _logger;
        private readonly GeneralSettings _generalSettings;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IEncryptor _encryptor;
        public VisualIdentityHandler(ILogger logger, IOptions<GeneralSettings> generalSettings, IHttpClientFactory clientFactory, IConfigurationConnector configurationConnector,
                   IEncryptor encryptor)
        {
            _logger = logger;
            _generalSettings = generalSettings.Value;
            _clientFactory = clientFactory;
            _configurationConnector = configurationConnector;
            _encryptor = encryptor;
        }
        public async Task<IdentityFlowResult> ReadVisualIdentityReqults(IdentityFlow identityFlow)
        {
            Configuration configuration =  await ValidateServiceSettings();
            string token = await GetToken(configuration);
            var identityFlowResult = await ReadFlowResult(token , configuration, identityFlow);
            return identityFlowResult;
        }

        public async Task<IdentityCreateFlowResult> StartVisualIdentityFlow(IdentityFlow identityFlow)
        {
            Configuration configuration = await ValidateServiceSettings();

            string token = await GetToken(configuration);
            string url = await CreateAuthFlow(token, configuration, identityFlow);
            return new IdentityCreateFlowResult
            {
                Url = url
            };
        }




        private async Task<IdentityFlowResult> ReadFlowResult(string token, Configuration configuration, IdentityFlow identityFlow)
        {
            using (var httpClient = _clientFactory.CreateClient())
            {
                GetIdentitySettings(configuration, out string url, out string user, out string password);         
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{url}Identification/AuthResults/{identityFlow.SignerToken}?externalId={identityFlow.SignerToken}&sessionToken={identityFlow.Code}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string jsonStringResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<IdentityFlowResult>(jsonStringResponse);                    
                    return result;
                }
                else
                {
                    _logger.Warning("Failed to get token from {Url}, code {StatusCode} , {ResponseContent}", url, response.StatusCode, response.Content);
                    throw new InvalidOperationException(ResultCode.VisualIdentityCantReadTokenFromService.GetNumericString());
                }

            }
        }


        private async Task<string> CreateAuthFlow(string token, Configuration configuration, IdentityFlow identityFlow)
        {
            using (var httpClient = _clientFactory.CreateClient())
            {
                GetIdentitySettings(configuration, out string url, out string user, out string password);
                ValidateServiceCreateFlowModel validateServiceCreateFlowModel = new ValidateServiceCreateFlowModel
                {
                    Id = identityFlow.SignerToken.ToString(),
                    SuccessRedirectUrl = $"{_generalSettings.SignerFronendApplicationRoute}/identityflowdone/{identityFlow.SignerToken}",
                    ErrorRedirectUrl = $"{_generalSettings.SignerFronendApplicationRoute}/identityflowdone/{identityFlow.SignerToken}"
                };
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string json = JsonConvert.SerializeObject(validateServiceCreateFlowModel);
                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{url}Identification", httpContent);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string jsonStringResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ValidateServiceCreateFlowResult>(jsonStringResponse);
                    return result.Url;
                }
                else
                {
                    _logger.Warning("Failed to get token from {Url}, code {StatusCode} , {ResponseContent}", url, response.StatusCode, response.Content );
                    throw new InvalidOperationException(ResultCode.VisualIdentityCantReadTokenFromService.GetNumericString());
                }

            }
        }

        private async Task<string> GetToken(Configuration configuration)
        {
            using (var httpClient = _clientFactory.CreateClient())
            {
                
                GetIdentitySettings(configuration, out string url, out string user, out string password);
                ValidateServiceLoginModel validateServiceLoginModel = new ValidateServiceLoginModel
                {
                    Password = password,
                    UserName = user
                };
                string json = JsonConvert.SerializeObject(validateServiceLoginModel);
                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{url}users/login", httpContent);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string jsonStringResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ValidateServiceLoginResult>(jsonStringResponse);
                    return result.Token;
                }
                else
                {
                    _logger.Warning("Failed to get token from {Url}, code {StatusCode} , {ResponseContent}", url, response.StatusCode, response.Content);
                    throw new InvalidOperationException(ResultCode.VisualIdentityCantReadTokenFromService.GetNumericString());
                }
            }
        }

        private void GetIdentitySettings(Configuration configuration, out string url, out string user, out string password)
        {
            url = configuration.VisualIdentityURL.EndsWith('/') ? configuration.VisualIdentityURL : $"{configuration.VisualIdentityURL}/";
            user = configuration.VisualIdentityUser;
            password = _encryptor.Decrypt(configuration.VisualIdentityPassword);
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(ResultCode.VisualIdentityMissingSettings.GetNumericString());
            }
        }

        private async Task<Configuration> ValidateServiceSettings()
        {
            Configuration configuration = await _configurationConnector.Read();

            if (string.IsNullOrWhiteSpace(configuration.VisualIdentityURL) || string.IsNullOrWhiteSpace(configuration.VisualIdentityPassword) || string.IsNullOrWhiteSpace(configuration.VisualIdentityUser))
            {
                throw new InvalidOperationException(ResultCode.VisualIdentityMissingSettings.GetNumericString());
            }
            return configuration;
           
        }

        private sealed class ValidateServiceCreateFlowModel
        {
            public string Id { get; set; }
            public string ErrorRedirectUrl { get; set; }
            public string SuccessRedirectUrl { get; set; }

        }


        private sealed class IdentityFlowAPIResult
        {
            public string FirstName { get;  set; }
            public string Id { get;  set; }
            public string LastName { get;  set; }
            public string PersonalId { get;  set; }
            public string ProcessResult { get;  set; }
        }
        private sealed class ValidateServiceCreateFlowResult
        {
            public string Url { get; set; }      

        }
        private sealed class ValidateServiceLoginModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }

        }
        private sealed class ValidateServiceLoginResult
        {
            public string Token { get; set; }            

        }
    }
    
}
