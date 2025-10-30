using Common.Enums;
using Common.Enums.Users;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.Emails;
using Common.Models.Sms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WeSignManagement.Models.Configurations;

namespace WeSignManagement.Controllers
{
#if DEBUG
    [Route("managementapi/v3/configuration")]
#else
    [Route("v3/configuration")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class ConfigurationController : ControllerBase
    {

        private readonly IManagementBL _bl;

        public ConfigurationController(IManagementBL bl)
        {
            _bl = bl;
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update(AppConfigurationDTO input)
        {
            var appConfiguration = new Configuration
            {
                MessageBefore = input.MessageBefore,
                MessageAfter = input.MessageAfter,
                MessageBeforeHebrew = input.MessageBeforeHebrew,
                MessageAfterHebrew = input.MessageAfterHebrew,
                LogArichveIntervalInDays = input.LogArichveIntervalInDays,
                ShouldReturnActivationLinkInAPIResponse = input.ShouldReturnActivationLinkInAPIResponse,
                DocumentDeletionConfiguration = new DocumentDeletionConfiguration
                {
                    DeleteSignedDocumentAfterXDays = input.DeleteSignedDocumentAfterXDays,
                    DeleteUnsignedDocumentAfterXDays = input.DeleteUnsignedDocumentAfterXDays,
                },
                SmsConfiguration = new SmsConfiguration
                {
                    From = input.SmsFrom,
                    Language = (Language)input.SmsLanguage,
                    Password = input.SmsPassword,
                    Provider = (ProviderType)input.SmsProvider,
                    User = input.SmsUser
                },
                SmtpConfiguration = new SmtpConfiguration
                {
                    Server = input.SmtpServer,
                    Port = input.SmtpPort,
                    From = input.SmtpFrom,
                    EnableSsl = input.SmtpEnableSsl,
                    AttachmentMaxSize = input.SmtpAttachmentMaxSize,
                    User = input.SmtpUser,
                    Password = input.SmtpPassword
                },
                UseManagementOtpAuth = input.UseManagementOtpAuth,
                EnableFreeTrailUsers = input.EnableFreeTrailUsers,
                EnableShowSSOOnlyInUserUI = input.EnableShowSSOOnlyInUserUI,
                EnableTabletsSupport = input.EnableTabletsSupport,
                EnableSigner1ExtraSigningTypes = input.EnableSigner1ExtraSigningTypes,
                ShouldUseReCaptchaInRegistration = input.ShouldUseReCaptchaInRegistration,
                ShouldUseSignerAuth = input.ShouldUseSignerAuth,
                ShouldUseSignerAuthDefault = input.ShouldUseSignerAuthDefault,
                EnableVisualIdentityFlow = input.EnableVisualIdentityFlow,
                SendWithOTPByDefault = input.ShouldSendWithOTPByDefault,
                VisualIdentityUser = input.VisualIdentityUser,
                VisualIdentityURL = input.VisualIdentityURL,
                VisualIdentityPassword = input.VisualIdentityPassword,
                ExternalPdfServiceAPIKey  = input.ExternalPdfServiceAPIKey,
                ExternalPdfServiceURL = input.ExternalPdfServiceURL,
                HistoryIntegratorServiceAPIKey = input.HistoryIntegratorServiceAPIKey,
                HistoryIntegratorServiceURL = input.HistoryIntegratorServiceURL,
                UseExternalGraphicSignature = input.UseExternalGraphicSignature,
                ExternalGraphicSignatureSigner1Url = input.ExternalGraphicSignatureSigner1Url,
                ExternalGraphicSignatureCert = input.ExternalGraphicSignatureCert,
                ExternalGraphicSignaturePin = input.ExternalGraphicSignaturePin,
                RecentPasswordsAmount = input.RecentPasswordsAmount,
                Signer1Configuration = new Signer1Configuration
                {
                    Endpoint = input.Signer1Endpoint.Replace('\\', '/').Trim(),
                    User = input.Signer1User,
                    Password = input.Signer1Password
                },
                ActiveDirectoryConfiguration = new ActiveDirectoryConfiguration()
                {
                    Container = input?.ActiveDirecrotyConfiguration?.Container,
                    Domain = input?.ActiveDirecrotyConfiguration?.Domain,
                    Host= input?.ActiveDirecrotyConfiguration?.Host,
                    Password = input?.ActiveDirecrotyConfiguration?.Password,
                    Port = input?.ActiveDirecrotyConfiguration?.Port ?? 0,
                    User = input?.ActiveDirecrotyConfiguration?.User
                }
            };
            await _bl.AppConfigurations.Update(appConfiguration);

            return Ok();
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AppConfigurationDTO))]
        public async Task<IActionResult> Read()
        {
            Configuration config = await _bl.AppConfigurations.Read();
            
            return Ok(new AppConfigurationDTO(config));
        }

        [HttpGet]
        [Route("init")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(InitManagementConfigurationDTO))]
        public async Task<IActionResult> ReadInitConfiguration()
        {
            Configuration config = await _bl.AppConfigurations.Read();

            return Ok(new InitManagementConfigurationDTO { UseManagementOtpAuth = config.UseManagementOtpAuth});
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        [Route("sms/message")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public IActionResult SendSmsTestMessage(SmsDetailsDTO input)
        {
            var smsConfiguration = new SmsConfiguration
            {
                From = input.From,
                Password = input.Password,
                Provider = input.Provider,
                User = input.User
            };
            var smsInfo = new Sms()
            {
                Phones = new List<string> { input.PhoneNumber },
                Message = input.Message
            };
            _bl.AppConfigurations.SendSmsTestMessage(smsConfiguration, smsInfo);

            return Ok();
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        [Route("smtp/message")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendEmailTestMessage(SmtpDetailsDTO input)
        {
            var smtpConfiguration = new SmtpConfiguration
            {
                EnableSsl = input.EnableSsl,
                From = input.From,
                Server = input.Server,
                Port = input.Port,
                User = input.User,
                Password = input.Password                
            };
            Email email = new Email
            {
                To = input.Email,
                Subject = "Test", 
                HtmlBody = new HtmlBody { TemplateText = input.Message}
            };
            await _bl.AppConfigurations.SendSmtpTestMessage(smtpConfiguration, email);

            return Ok();
        }
    }
}
