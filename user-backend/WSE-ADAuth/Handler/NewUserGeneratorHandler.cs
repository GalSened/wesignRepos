using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Extensions;
using WSE_ADAuth.Models;
using WSE_ADAuth.Models.Management;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using System.Net.Http.Headers;

namespace WSE_ADAuth.Handler
{
    public class NewUserGeneratorHandler : INewUserGenerator
    {
        private GeneralSettings _generalSettings;
        private IUserConnector _userConnector;
    
        private ILogger _logger;
 
        private AutoUserCreatingSettings _autoUserCreatingSettings;
        private IHttpClientFactory _clientFactory;
        private IEncryptor _encryptor;
        private const string MEDIA_TYPE = "application/json";
        public NewUserGeneratorHandler(IOptions<GeneralSettings> GeneralSettings,IUserConnector userConnector, ILogger logger,            
             IOptions<AutoUserCreatingSettings> autoUserCreatingSettings,  IEncryptor encryptor,
              IHttpClientFactory clientFactory)
        {
            _generalSettings = GeneralSettings.Value;
            _userConnector = userConnector;
         
            _logger = logger;
          
            _autoUserCreatingSettings = autoUserCreatingSettings.Value;
            _clientFactory = clientFactory;
            _encryptor = encryptor;


        }
        public async Task<User> CreateNewUser(LoginToClient loginToClient)
        {
          
            
            if (_autoUserCreatingSettings == null || !_autoUserCreatingSettings.Active)
            {
                _logger.Debug("Create new user in login is not active");
                return null;
            }
           
            string token = Login();
            if(string.IsNullOrWhiteSpace(token))
            {
                return null;
            }
            
            if(!CreateCompany(loginToClient, token))
            {
                return null;
            }

            return await _userConnector.Read(new User
            {
                Email = loginToClient.UserEmail
            });
            
        }

        private bool CreateCompany(LoginToClient loginToClient, string token)
        {
            using (var client = _clientFactory.CreateClient())
            {
                var createCompanyRequest = new WSE_ADAuth.Models.Management.Company
                {
                    CompanyName = $"{loginToClient.UserName}_Comp_{Guid.NewGuid().ToString("N").Substring(0, 5)}",
                    ExpirationTime = DateTime.Now.AddMonths(_autoUserCreatingSettings.ValidTillInMonths),
                    Language = Language.en,
                    ProgramId = _autoUserCreatingSettings.PlanID,
                    User = new BaseUser
                    {
                        Email = loginToClient.UserEmail,
                        GroupName = "Main",
                        UserName = loginToClient.UserName
                    }

                };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string content = JsonConvert.SerializeObject(createCompanyRequest).ToString();
                StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);
                HttpResponseMessage serverResponse = client.PostAsync($"{_autoUserCreatingSettings.ManagementAPIURL}/companies", stringContent).GetAwaiter().GetResult();
                if (serverResponse.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }

                return false;

            }
            
        }

        private string Login()
        {
            using (var client = _clientFactory.CreateClient())
            {
                var loginDTO = new LoginRequestDTO
                {
                    Email = _encryptor.Decrypt(_autoUserCreatingSettings.Key1),
                    Password = _encryptor.Decrypt(_autoUserCreatingSettings.Key2) 
                };
                string content = JsonConvert.SerializeObject(loginDTO).ToString();
                StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);
                HttpResponseMessage serverResponse =  client.PostAsync($"{_autoUserCreatingSettings.ManagementAPIURL}/users/login", stringContent).GetAwaiter().GetResult();
                string resposnsString =  serverResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (serverResponse.StatusCode == HttpStatusCode.OK)
                {
                    var successResponse = JsonConvert.DeserializeObject<TokensManagementDTO>(resposnsString);
                    return successResponse.Token;
                }
                else
                {
                    _logger.Warning("Failed To login to Managment System in auto creating user");
                }
              
            }
            return null;
        }

    }
}

