using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers.SendingMessages.SMS;
using Common.Interfaces;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;

using System.Threading.Tasks;

namespace BL.Handlers
{
    public class ExternalPDFServiceHandler : IExternalPDFService
    {
        
        private IHttpClientFactory _httpClientFactory;
        private ILogger _logger;
        private IEncryptor _encryptor;
        private readonly IConfiguration _configuration;

        public ExternalPDFServiceHandler(IHttpClientFactory httpClientFactory, ILogger logger, IEncryptor encryptor,
            IConfiguration configuration)
        {
            
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _encryptor = encryptor;
            _configuration = configuration;
        }
        public async Task<string> Merge(List<string> templatesContents)
        {

            var configuration = (await _configuration.ReadAppConfiguration());

            if(string.IsNullOrWhiteSpace(configuration.ExternalPdfServiceURL) || string.IsNullOrWhiteSpace(configuration.ExternalPdfServiceAPIKey))
            {
                _logger.Warning("Missing configuration for external PDF Service");
                throw new InvalidOperationException(ResultCode.MissingSettingsForPDFExternalSettings.GetNumericString());
                    
            }

            var model = new FileMerge
            {
                APIKey = _encryptor.Decrypt(configuration.ExternalPdfServiceAPIKey),
                Base64Files = templatesContents
            };

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                string json = JsonConvert.SerializeObject(model);
                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");


                var response = await httpClient.PostAsync($"{configuration.ExternalPdfServiceURL}/operations/mergefiles", httpContent);
                _logger.Information("Response from external PDF service: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var resultFile = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<MergeResultDTO>(resultFile);
                    return result.Document;// Convert.ToBase64String(resultFile);
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.Error("Error in external conversion {Result}", @result);
                    if (result.Contains("Invalid credential"))
                    {
                        throw new InvalidOperationException(ResultCode.FaildToMergeFileErrorFromExternalServiceInvalidCredential.GetNumericString());
                    }
                    else if (result.Contains("File size limit to"))
                    {
                        throw new InvalidOperationException(ResultCode.FileToMergeFileErrorFromExternalServiceFileSizeExceedingLimit.GetNumericString());
                    }
                    // need to fix message;
                    throw new InvalidOperationException(ResultCode.FaildToMergeFileErrorFromExternalService.GetNumericString());
                }
            }

        }
        private sealed class FileMerge
        {
            public List<string> Base64Files { get; set; }
            public string APIKey { get; internal set; }
        }
    }

    public class MergeResultDTO
    {
        public string Document { get; set; }
    }

}
