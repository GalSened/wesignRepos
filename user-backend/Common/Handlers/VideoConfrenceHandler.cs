using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.Links;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common.Models.Configurations;
using Common.Enums.Results;
using Newtonsoft.Json;
using System.Net.Http;
using Common.Extensions;
using Serilog;
using Common.Models;
using System.Net.Http.Headers;
namespace Common.Handlers
{
    public class VideoConfrenceHandler : IVideoConfrence
    {
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IEncryptor _encryptor;
        private readonly ILogger _logger;

        public VideoConfrenceHandler(ILogger logger, IConfigurationConnector configurationConnector, IHttpClientFactory clientFactory,
            IEncryptor encryptor)
        {
            _configurationConnector = configurationConnector;
            _clientFactory = clientFactory;
            _encryptor = encryptor;
        }
        public async Task<ExternalVideoConfrenceResult> CreateVideoConference()
        {
            Configuration configuration = await ValidateServiceSettings();
            string token = await GetToken(configuration);
            using (var httpClient = _clientFactory.CreateClient())
            {
                GetExternalServiceSettings(configuration, out string url, out string user, out string password);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await httpClient.GetAsync($"{url}LiveMeeting/CreateZoomMetting");

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string jsonStringResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ExternalVideoConfrenceResult>(jsonStringResponse);
                    return result;
                }
                else
                {
                    _logger.Warning("Failed to create a  Video Confrence from {Url}, code {StatusCode} , {ResponseContent}", url, response.StatusCode, response.Content);
                    throw new InvalidOperationException(ResultCode.FailedToCreateVideoConfrence.GetNumericString());
                }

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


        private void GetExternalServiceSettings(Configuration configuration, out string url, out string user, out string password)
        {
            url = configuration.VisualIdentityURL.EndsWith('/') ? configuration.VisualIdentityURL : $"{configuration.VisualIdentityURL}/";
            user = configuration.VisualIdentityUser;
            password = _encryptor.Decrypt(configuration.VisualIdentityPassword);
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(ResultCode.VisualIdentityMissingSettings.GetNumericString());
            }
        }


        private async Task<string> GetToken(Configuration configuration)
        {
            using (var httpClient = _clientFactory.CreateClient())
            {

                GetExternalServiceSettings(configuration, out string url, out string user, out string password);
                ExternalServiceLoginModel externalServiceLoginModel = new ExternalServiceLoginModel
                {
                    Password = password,
                    UserName = user
                };
                string json = JsonConvert.SerializeObject(externalServiceLoginModel);
                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{url}users/login", httpContent);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string jsonStringResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ExtrnalServiceLoginResult>(jsonStringResponse);
                    return result.Token;
                }
                else
                {
                    _logger.Warning("Failed to get token from {Url}, code {StatusCode} , {ResponseContent}", url, response.StatusCode, response.Content);
                    throw new InvalidOperationException(ResultCode.VisualIdentityCantReadTokenFromService.GetNumericString());
                }
            }
        }

        private sealed class ExtrnalServiceLoginResult
        {
            public string Token { get; set; }

        }
        private sealed class ExternalServiceLoginModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }

        }
    }
    
}
