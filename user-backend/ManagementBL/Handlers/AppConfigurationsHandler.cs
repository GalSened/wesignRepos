using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.MessageSending.Sms;
using Common.Models.Configurations;
using Common.Models.Emails;
using Common.Models.License;
using Common.Models.Settings;
using Common.Models.Sms;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class AppConfigurationsHandler : IAppConfigurations
    {
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IEncryptor _encryptor;
        private readonly ILicense _license;
        private readonly IActiveDirectoryConfigConnector _activeDirectoryConfigConnector;
        private readonly ISmsProviderHandler _smsProvider;
        private readonly IEmailProvider _emailProvider;

        public AppConfigurationsHandler(IConfigurationConnector configurationConnector, IEncryptor encryptor, ILicense license,
            IActiveDirectoryConfigConnector activeDirectoryConfigConnector, ISmsProviderHandler smsProvider, IEmailProvider emailProvider)
        {
            _configurationConnector = configurationConnector;
            _encryptor = encryptor;
            _license = license;
            _activeDirectoryConfigConnector = activeDirectoryConfigConnector;
            _smsProvider = smsProvider;
            _emailProvider = emailProvider;
        }

        public async Task<Configuration> Read()
        {
            var configuration = await _configurationConnector.Read();
            (var licenseInfo,var _) =await  _license.GetLicenseInformationAndUsing( false);
            if(licenseInfo.LicenseCounters.UseActiveDirectory)
            {
                configuration.ActiveDirectoryConfiguration = await _activeDirectoryConfigConnector.Read();
            }
            return configuration;
        }

        public async Task Update(Configuration appConfiguration)
        {
            if (appConfiguration == null)
            {
                throw new Exception($"Null input - app configuration is null");
            }

            var dbConfig = await _configurationConnector.Read();
            appConfiguration.SmsConfiguration.Password = IsPasswordChanged(appConfiguration.SmsConfiguration.Password, dbConfig.SmsConfiguration.Password)? 
                                                        _encryptor.Encrypt(appConfiguration.SmsConfiguration.Password):
                                                        appConfiguration.SmsConfiguration.Password;
            appConfiguration.SmtpConfiguration.Password = IsPasswordChanged(appConfiguration.SmtpConfiguration.Password, dbConfig.SmtpConfiguration.Password) ?
                                                        _encryptor.Encrypt(appConfiguration.SmtpConfiguration.Password) :
                                                        appConfiguration.SmtpConfiguration.Password;
            appConfiguration.Signer1Configuration.Password = IsPasswordChanged(appConfiguration.Signer1Configuration.Password, dbConfig.Signer1Configuration.Password) ?
                                                                    _encryptor.Encrypt(appConfiguration.Signer1Configuration.Password) :
                                                                    appConfiguration.Signer1Configuration.Password;
            
            appConfiguration.VisualIdentityPassword = IsPasswordChanged(appConfiguration.VisualIdentityPassword, dbConfig.VisualIdentityPassword) ?
                                                                    _encryptor.Encrypt(appConfiguration.VisualIdentityPassword) :
                                                                    appConfiguration.VisualIdentityPassword;


            appConfiguration.ExternalPdfServiceAPIKey = IsPasswordChanged(appConfiguration.ExternalPdfServiceAPIKey, dbConfig.ExternalPdfServiceAPIKey) ?
                                                                    _encryptor.Encrypt(appConfiguration.ExternalPdfServiceAPIKey) :
                                                                    appConfiguration.ExternalPdfServiceAPIKey;

            appConfiguration.HistoryIntegratorServiceAPIKey = IsPasswordChanged(appConfiguration.HistoryIntegratorServiceAPIKey, dbConfig.HistoryIntegratorServiceAPIKey) ?
                                                                    _encryptor.Encrypt(appConfiguration.HistoryIntegratorServiceAPIKey) :
                                                                    appConfiguration.HistoryIntegratorServiceAPIKey;


            appConfiguration.ExternalGraphicSignaturePin = IsPasswordChanged(appConfiguration.ExternalGraphicSignaturePin, dbConfig.ExternalGraphicSignaturePin) ?
                                                                   _encryptor.Encrypt(appConfiguration.ExternalGraphicSignaturePin) :
                                                                   appConfiguration.ExternalGraphicSignaturePin;


            await _configurationConnector.Update(appConfiguration);          
                        
           ( var licenseInfo, var _ )= await _license.GetLicenseInformationAndUsing(false);
            if (appConfiguration.ActiveDirectoryConfiguration != null && licenseInfo.LicenseCounters.UseActiveDirectory)
            {
                var adconfig = await _activeDirectoryConfigConnector.Read();
                if(adconfig == null)
                {
                    await CreateActiveDirectorySettings(appConfiguration.ActiveDirectoryConfiguration);
                }
                else
                {
                   await  UpdateActiveDirectorySettings(adconfig, appConfiguration.ActiveDirectoryConfiguration);
                }

            }
        }

        public void SendSmsTestMessage(SmsConfiguration smsConfiguration, Sms smsInfo)
        {
            var smsProvider = _smsProvider.ExecuteCreation(smsConfiguration.Provider);
            smsProvider.SendAsync(smsInfo, smsConfiguration);
        }

        public  Task SendSmtpTestMessage(SmtpConfiguration smtpConfiguration, Email email)
        {
            return _emailProvider.Send(email, smtpConfiguration);
        }

        #region Private Functions

        private  Task UpdateActiveDirectorySettings(ActiveDirectoryConfiguration existingConfigurationRecord, ActiveDirectoryConfiguration newConfigurationData)
        {
            newConfigurationData.Password =  IsPasswordChanged(existingConfigurationRecord.Password, newConfigurationData.Password) ?
                                                       _encryptor.Encrypt(newConfigurationData.Password) :
                                                       existingConfigurationRecord.Password;

            return _activeDirectoryConfigConnector.Update(newConfigurationData);

        }

        private  Task CreateActiveDirectorySettings(ActiveDirectoryConfiguration appConfiguration)
        {
            if (!string.IsNullOrWhiteSpace(appConfiguration.Password))
            {
                appConfiguration.Password = _encryptor.Encrypt(appConfiguration.Password);
            }
            return _activeDirectoryConfigConnector.Create(appConfiguration);
        }

        private bool IsPasswordChanged(string password1, string password2)
        {
            return password1 != password2;
        }

        #endregion
    }
}
