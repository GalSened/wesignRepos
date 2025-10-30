using Common.Enums;
using Common.Enums.Users;
using Common.Interfaces.DB;
using Common.Models.Configurations;
using Common.Models.Documents;
using DAL.DAOs.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class ConfigurationConnector : IConfigurationConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private readonly string ConfigurationKey = "ConfigurationKey";
        private readonly string TabletKey = "TabletMemKey";
        private readonly int MaxTabletInMemory = 3000;

        public ConfigurationConnector(IWeSignEntities dbContext, IMemoryCache memoryCache, ILogger logger)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<Configuration> Read()
        {
            try
            {
                var configuration = _memoryCache.Get<Configuration>(ConfigurationKey); //_memoryCache.Set<Configuration>(userId, user, TimeSpan.FromMinutes(3));
                if (configuration != null)
                {
                    return configuration;
                }

                var result = await _dbContext.Configuration.ToListAsync();
                var smtpKeys = result.Where(c => c.Key.StartsWith("Smtp"));
                var smtpConfiguration = ToSmtpConfiguration(smtpKeys);
                var smsKeys = result.Where(c => c.Key.StartsWith("Sms"));
                var smsConfiguration = ToSmsConfiguration(smsKeys);
                var documentDeletionKeys = result.Where(c => c.Key.StartsWith("Delete"));
                var documentDeletionConfiguration = ToDocumentDeletionConfiguration(documentDeletionKeys);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "UseManagementOtpAuth")?.Value, out bool useManagementOtpAuth);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "EnableFreeTrailUsers")?.Value, out bool enableFreeTrailUsers);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "EnableTabletsSupport")?.Value, out bool enableTabletsSupport);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "EnableSigner1ExtraSigningTypes")?.Value, out bool enableSigner1ExtraSigningTypes);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "ShouldUseReCaptchaInRegistration")?.Value, out bool shouldUseReCaptchaInRegistration);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "ShouldUseSignerAuth")?.Value, out bool shouldUseSignerAuth);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "ShouldUseSignerAuthDefault")?.Value, out bool shouldUseSignerAuthDefault);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "SendWithOTPByDefault")?.Value, out bool shouldSendWithOTPByDefault);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "EnableVisualIdentityFlow")?.Value, out bool EnableVisualIdentityFlow);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "EnableShowSSOOnlyInUserUI")?.Value, out bool enableShowSSOOnlyInUserUI);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "EnableRenewalPayingUserLogic")?.Value, out bool enableRenewalPayingUserLogic);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "UseExternalGraphicSignature")?.Value, out bool useExternalGraphicSignature);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "ShouldReturnActivationLinkInAPIResponse")?.Value, out bool shouldReturnActivationLinkInAPIResponse);
                int.TryParse(result?.FirstOrDefault(c => c.Key == "RecentPasswordsAmount")?.Value, out int recentPasswordsAmount);
                bool.TryParse(result?.FirstOrDefault(x => x.Key == "IsTemplatesSyncedMandatoryFields")?.Value, out bool isTemplatesSyncedMandatoryFields);

                configuration = new Configuration()
                {
                    SmtpConfiguration = smtpConfiguration,
                    SmsConfiguration = smsConfiguration,
                    DocumentDeletionConfiguration = documentDeletionConfiguration,
                    MessageBefore = result.FirstOrDefault(c => c.Key == "MessageBefore")?.Value,
                    MessageAfter = result.FirstOrDefault(c => c.Key == "MessageAfter")?.Value,
                    MessageBeforeHebrew = result.FirstOrDefault(c => c.Key == "MessageBeforeHebrew")?.Value,
                    MessageAfterHebrew = result.FirstOrDefault(c => c.Key == "MessageAfterHebrew")?.Value,
                    LogArichveIntervalInDays = int.Parse(result.FirstOrDefault(c => c.Key == "LogArichveIntervalInDays")?.Value),
                    UseManagementOtpAuth = useManagementOtpAuth,
                    EnableFreeTrailUsers = enableFreeTrailUsers,
                    EnableTabletsSupport = enableTabletsSupport,
                    EnableShowSSOOnlyInUserUI = enableShowSSOOnlyInUserUI,
                    EnableSigner1ExtraSigningTypes = enableSigner1ExtraSigningTypes,
                    Signer1Configuration = new Signer1Configuration
                    {
                        Endpoint = result.FirstOrDefault(x => x.Key == "Signer1Endpoint")?.Value,
                        User = result.FirstOrDefault(x => x.Key == "Signer1User")?.Value,
                        Password = result.FirstOrDefault(x => x.Key == "Signer1Password")?.Value,
                    },
                    ShouldUseReCaptchaInRegistration = shouldUseReCaptchaInRegistration,
                    ShouldUseSignerAuth = shouldUseSignerAuth,
                    ShouldUseSignerAuthDefault = shouldUseSignerAuthDefault,
                    EnableVisualIdentityFlow = EnableVisualIdentityFlow,
                    SendWithOTPByDefault = shouldSendWithOTPByDefault,
                    EnableRenewalPayingUserLogic = enableRenewalPayingUserLogic,
                    VisualIdentityURL = result.FirstOrDefault(x => x.Key == "VisualIdentityURL")?.Value,
                    VisualIdentityPassword = result.FirstOrDefault(x => x.Key == "VisualIdentityPassword")?.Value,
                    VisualIdentityUser = result.FirstOrDefault(x => x.Key == "VisualIdentityUser")?.Value,
                    ExternalPdfServiceURL = result.FirstOrDefault(x => x.Key == "ExternalPdfServiceURL")?.Value,
                    ExternalPdfServiceAPIKey = result.FirstOrDefault(x => x.Key == "ExternalPdfServiceAPIKey")?.Value,
                    HistoryIntegratorServiceURL = result.FirstOrDefault(x => x.Key == "HistoryIntegratorServiceURL")?.Value,
                    HistoryIntegratorServiceAPIKey = result.FirstOrDefault(x => x.Key == "HistoryIntegratorServiceAPIKey")?.Value,
                    UseExternalGraphicSignature = useExternalGraphicSignature,
                    ExternalGraphicSignatureSigner1Url = result.FirstOrDefault(x => x.Key == "ExternalGraphicSignatureSigner1Url")?.Value,
                    ExternalGraphicSignatureCert = result.FirstOrDefault(x => x.Key == "ExternalGraphicSignatureCert")?.Value,
                    ExternalGraphicSignaturePin = result.FirstOrDefault(x => x.Key == "ExternalGraphicSignaturePin")?.Value,
                    ShouldReturnActivationLinkInAPIResponse = shouldReturnActivationLinkInAPIResponse,
                    RecentPasswordsAmount = recentPasswordsAmount,
                    IsTemplatesSyncedMandatoryFields = isTemplatesSyncedMandatoryFields
                };
                _memoryCache.Set<Configuration>(ConfigurationKey, configuration, TimeSpan.FromMinutes(10));
                return configuration;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ConfigurationConnector_Read = ");
                throw new InvalidOperationException("Failed to read configuration from DB", ex);
            }
        }

        public async Task Update(Configuration appConfiguration)
        {
            try
            {
                _memoryCache.Remove(ConfigurationKey);
                var result = await _dbContext.Configuration.ToListAsync();
                result.FirstOrDefault(x => x.Key == "UseManagementOtpAuth").Value = appConfiguration.UseManagementOtpAuth.ToString();
                result.FirstOrDefault(x => x.Key == "EnableFreeTrailUsers").Value = appConfiguration.EnableFreeTrailUsers.ToString();
                result.FirstOrDefault(x => x.Key == "EnableShowSSOOnlyInUserUI").Value = appConfiguration.EnableShowSSOOnlyInUserUI.ToString();
                result.FirstOrDefault(x => x.Key == "EnableTabletsSupport").Value = appConfiguration.EnableTabletsSupport.ToString();
                result.FirstOrDefault(x => x.Key == "EnableSigner1ExtraSigningTypes").Value = appConfiguration.EnableSigner1ExtraSigningTypes.ToString();
                result.FirstOrDefault(x => x.Key == "ShouldUseReCaptchaInRegistration").Value = appConfiguration.ShouldUseReCaptchaInRegistration.ToString();
                result.FirstOrDefault(x => x.Key == "ShouldUseSignerAuth").Value = appConfiguration.ShouldUseSignerAuth.ToString();
                result.FirstOrDefault(x => x.Key == "ShouldUseSignerAuthDefault").Value = appConfiguration.ShouldUseSignerAuthDefault.ToString();
                result.FirstOrDefault(x => x.Key == "LogArichveIntervalInDays").Value = appConfiguration.LogArichveIntervalInDays.ToString();
                result.FirstOrDefault(x => x.Key == "MessageBefore").Value = appConfiguration.MessageBefore;
                result.FirstOrDefault(x => x.Key == "MessageAfter").Value = appConfiguration.MessageAfter;
                result.FirstOrDefault(x => x.Key == "MessageBeforeHebrew").Value = appConfiguration.MessageBeforeHebrew;
                result.FirstOrDefault(x => x.Key == "MessageAfterHebrew").Value = appConfiguration.MessageAfterHebrew;
                result.FirstOrDefault(x => x.Key == "Signer1Endpoint").Value = appConfiguration.Signer1Configuration.Endpoint;
                result.FirstOrDefault(x => x.Key == "Signer1User").Value = appConfiguration.Signer1Configuration.User;
                result.FirstOrDefault(x => x.Key == "Signer1Password").Value = appConfiguration.Signer1Configuration.Password;
                result.FirstOrDefault(x => x.Key == "DeleteSignedDocumentAfterXDays").Value = appConfiguration.DocumentDeletionConfiguration.DeleteSignedDocumentAfterXDays.ToString();
                result.FirstOrDefault(x => x.Key == "DeleteUnsignedDocumentAfterXDays").Value = appConfiguration.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays.ToString();
                result.FirstOrDefault(x => x.Key == "SmsUser").Value = appConfiguration.SmsConfiguration.User;
                result.FirstOrDefault(x => x.Key == "SmsPassword").Value = appConfiguration.SmsConfiguration.Password;
                result.FirstOrDefault(x => x.Key == "SmsFrom").Value = appConfiguration.SmsConfiguration.From;
                result.FirstOrDefault(x => x.Key == "SmsProvider").Value = appConfiguration.SmsConfiguration.Provider.ToString();
                result.FirstOrDefault(x => x.Key == "SmsLanguage").Value = appConfiguration.SmsConfiguration.Language.ToString();
                result.FirstOrDefault(x => x.Key == "SmtpEnableSsl").Value = appConfiguration.SmtpConfiguration.EnableSsl.ToString();
                result.FirstOrDefault(x => x.Key == "SmtpFrom").Value = appConfiguration.SmtpConfiguration.From;
                result.FirstOrDefault(x => x.Key == "SmtpPassword").Value = appConfiguration.SmtpConfiguration.Password;
                result.FirstOrDefault(x => x.Key == "SmtpAttachmentMaxSize").Value = appConfiguration.SmtpConfiguration.AttachmentMaxSize.ToString();
                result.FirstOrDefault(x => x.Key == "SmtpPort").Value = appConfiguration.SmtpConfiguration.Port.ToString();
                result.FirstOrDefault(x => x.Key == "SmtpServer").Value = appConfiguration.SmtpConfiguration.Server;
                result.FirstOrDefault(x => x.Key == "SmtpUser").Value = appConfiguration.SmtpConfiguration.User;
                result.FirstOrDefault(x => x.Key == "SendWithOTPByDefault").Value = appConfiguration.SendWithOTPByDefault.ToString();
                result.FirstOrDefault(x => x.Key == "EnableVisualIdentityFlow").Value = appConfiguration.EnableVisualIdentityFlow.ToString();
                result.FirstOrDefault(x => x.Key == "VisualIdentityURL").Value = appConfiguration.VisualIdentityURL.ToString();
                result.FirstOrDefault(x => x.Key == "VisualIdentityPassword").Value = appConfiguration.VisualIdentityPassword.ToString();
                result.FirstOrDefault(x => x.Key == "VisualIdentityUser").Value = appConfiguration.VisualIdentityUser.ToString();
                result.FirstOrDefault(x => x.Key == "ExternalPdfServiceURL").Value = appConfiguration.ExternalPdfServiceURL.ToString();
                result.FirstOrDefault(x => x.Key == "ExternalPdfServiceAPIKey").Value = appConfiguration.ExternalPdfServiceAPIKey.ToString();
                result.FirstOrDefault(x => x.Key == "HistoryIntegratorServiceURL").Value = appConfiguration.HistoryIntegratorServiceURL.ToString();
                result.FirstOrDefault(x => x.Key == "HistoryIntegratorServiceAPIKey").Value = appConfiguration.HistoryIntegratorServiceAPIKey.ToString();
                result.FirstOrDefault(x => x.Key == "UseExternalGraphicSignature").Value = appConfiguration.UseExternalGraphicSignature.ToString();
                result.FirstOrDefault(x => x.Key == "ExternalGraphicSignatureSigner1Url").Value = appConfiguration.ExternalGraphicSignatureSigner1Url.ToString();
                result.FirstOrDefault(x => x.Key == "ExternalGraphicSignatureCert").Value = appConfiguration.ExternalGraphicSignatureCert.ToString();
                result.FirstOrDefault(x => x.Key == "ExternalGraphicSignaturePin").Value = appConfiguration.ExternalGraphicSignaturePin.ToString();
                result.FirstOrDefault(x => x.Key == "ShouldReturnActivationLinkInAPIResponse").Value = appConfiguration.ShouldReturnActivationLinkInAPIResponse.ToString();
                result.FirstOrDefault(x => x.Key == "RecentPasswordsAmount").Value = appConfiguration.RecentPasswordsAmount.ToString();
                result.FirstOrDefault(x => x.Key == "IsTemplatesSyncedMandatoryFields").Value = appConfiguration.IsTemplatesSyncedMandatoryFields.ToString();
                
                _dbContext.Configuration.UpdateRange(result);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ConfigurationConnector_Update = ");
                throw;
            }
        }

        public IEnumerable<Tablet> ReadTablets(string key, Guid companyId)
        {
            try
            {
                List<Tablet> tablets = _memoryCache.Get<List<Tablet>>($"{companyId}_{TabletKey}");
                if (tablets == null)
                {
                    tablets = _dbContext.Tablets.Where(x => x.CompanyId == companyId).Take(MaxTabletInMemory).Select(x => new Tablet { Id = x.Id, Name = x.Name, CompanyId = x.CompanyId }).ToList();
                    _memoryCache.Set($"{companyId}_{TabletKey}", tablets, TimeSpan.FromSeconds(40));
                }

                return tablets.Where(x => string.IsNullOrWhiteSpace(key) ? x.Id != Guid.Empty :
                x.Name.Contains(key, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ConfigurationConnector_ReadTablets = ");
                throw;
            }
        }

        public async Task<bool> IsTabletExist(Tablet tablet)
        {
            try
            {
                bool isExist = _memoryCache.Get<bool>($"IsTabletExist_{tablet.CompanyId}_{tablet.Name}");
                if (isExist)
                {
                    return isExist;
                }
                isExist = await _dbContext.Tablets.AnyAsync(x => x.Name == tablet.Name && x.CompanyId == tablet.CompanyId);
                _memoryCache.Set($"IsTabletExist_{tablet.CompanyId}_{tablet.Name}", isExist, TimeSpan.FromMinutes(3));
                return isExist;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ConfigurationConnector_IsTabletExist = ");
                throw;
            }
        }

        public async Task CreateTablet(Tablet tablet)
        {
            try
            {
                if (!await _dbContext.Tablets.AnyAsync(x => x.Name == tablet.Name && x.CompanyId == tablet.CompanyId))
                {
                    var tabletDAO = new TabletDAO(tablet);
                    await _dbContext.Tablets.AddAsync(tabletDAO);
                    await _dbContext.SaveChangesAsync();
                }
                _memoryCache.Remove($"IsTabletExist_{tablet.CompanyId}_{tablet.Name}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ConfigurationConnector_CreateTablet = ");
                throw;
            }
        }

        #region Private Functions

        private DocumentDeletionConfiguration ToDocumentDeletionConfiguration(IEnumerable<ConfigurationDAO> documentDeletionKeys)
        {
            return new DocumentDeletionConfiguration()
            {
                DeleteSignedDocumentAfterXDays = int.Parse(documentDeletionKeys.FirstOrDefault(x => x.Key == "DeleteSignedDocumentAfterXDays")?.Value),
                DeleteUnsignedDocumentAfterXDays = int.Parse(documentDeletionKeys.FirstOrDefault(x => x.Key == "DeleteUnsignedDocumentAfterXDays")?.Value)
            };
        }

        private SmsConfiguration ToSmsConfiguration(IEnumerable<ConfigurationDAO> smsKeys)
        {
            var result = new SmsConfiguration()
            {

                User = smsKeys?.FirstOrDefault(x => x.Key == "SmsUser")?.Value,
                //TODO set password encrypt and success to decrypt it (using pbkdf2 we cannot decrypt password)
                Password = smsKeys?.FirstOrDefault(x => x.Key == "SmsPassword")?.Value,
                From = smsKeys?.FirstOrDefault(x => x.Key == "SmsFrom")?.Value,
                Provider = (ProviderType)Enum.Parse(typeof(ProviderType), smsKeys?.FirstOrDefault(x => x.Key == "SmsProvider")?.Value),
                Language = (Language)Enum.Parse(typeof(Language), smsKeys?.FirstOrDefault(x => x.Key == "SmsLanguage")?.Value)
            };

            return result;
        }

        private SmtpConfiguration ToSmtpConfiguration(IEnumerable<ConfigurationDAO> smtpKeys)
        {
            int.TryParse(smtpKeys?.FirstOrDefault(x => x.Key == "SmtpAttachmentMaxSize")?.Value, out int attachmentSize);
            int.TryParse(smtpKeys?.FirstOrDefault(x => x.Key == "SmtpPort")?.Value, out int port);
            bool.TryParse(smtpKeys?.FirstOrDefault(x => x.Key == "SmtpEnableSsl")?.Value, out bool ssl);
            return new SmtpConfiguration()
            {
                EnableSsl = ssl,
                From = smtpKeys?.FirstOrDefault(x => x.Key == "SmtpFrom")?.Value,
                Password = smtpKeys?.FirstOrDefault(x => x.Key == "SmtpPassword")?.Value,
                Port = port,
                Server = smtpKeys?.FirstOrDefault(x => x.Key == "SmtpServer")?.Value,
                User = smtpKeys?.FirstOrDefault(x => x.Key == "SmtpUser")?.Value,
                AttachmentMaxSize = attachmentSize
            };
        }

        #endregion
    }
}
