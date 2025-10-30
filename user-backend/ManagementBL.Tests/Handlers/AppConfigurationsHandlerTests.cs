using Comda.License.DAL;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.MessageSending.Sms;
using Common.Models.Configurations;
using Common.Models.License;
using Common.Models.Sms;
using ManagementBL.Handlers;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ManagementBL.Tests.Handlers
{
    public class AppConfigurationsHandlerTests
    {

        private readonly Mock<IConfigurationConnector> _configurationConnectorMock;
        private readonly Mock<IEncryptor> _encryptorMock;
        private readonly Mock<IActiveDirectoryConfigConnector> _activeDirectoryConfigConnector;
        private readonly Mock<ILicense> _licenseMock;
        private readonly Mock<IFileSystem> _fileSystem;
        private readonly Mock<ISmsProviderHandler> _smsProvider;

        private readonly AppConfigurationsHandler _appConfigurationsHandler;

        public AppConfigurationsHandlerTests()
        {
            _configurationConnectorMock = new Mock<IConfigurationConnector>();
            _encryptorMock = new Mock<IEncryptor>();
            _activeDirectoryConfigConnector = new Mock<IActiveDirectoryConfigConnector>();
            _licenseMock = new Mock<ILicense>();
            _fileSystem = new Mock<IFileSystem>();
            _smsProvider = new Mock<ISmsProviderHandler>();
            _appConfigurationsHandler = new AppConfigurationsHandler(_configurationConnectorMock.Object, _encryptorMock.Object, _licenseMock.Object, _activeDirectoryConfigConnector.Object, _smsProvider.Object, null);
        }

        #region Read
        [Fact]
        public async Task Read_DBReadConfigurations_ThrowException()
        {
            string excErrorMsg = "Failed to read configuration from DB";
            _configurationConnectorMock.Setup(x => x.Read()).Throws(new InvalidOperationException(excErrorMsg));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _appConfigurationsHandler.Read());

            Assert.Equal(excErrorMsg, actual.Message);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Read_DBReadConfigurations_Succsess(bool useActiveDirectory)
        {
            SetupLicenseMocker(useActiveDirectory);
            _configurationConnectorMock.Setup(x => x.Read()).ReturnsAsync(
                new Configuration()
                {
                    MessageBefore = "this is my message Before"
                });

            var readResult = await _appConfigurationsHandler.Read();

            Assert.Equal("this is my message Before", readResult.MessageBefore);
        }
        #endregion

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task Update_UpdateSmsConfigurationPassword_Succsess(bool useActiveDirectory, bool setupConfig)
        {
            SetupLicenseMocker(useActiveDirectory);

            if (setupConfig)
            {
                _activeDirectoryConfigConnector.Setup(x => x.Read()).ReturnsAsync(new ActiveDirectoryConfiguration());
            }

            _encryptorMock.Setup(x => x.Encrypt(It.IsAny<string>())).Returns("Fixed");
            SmsConfiguration smsConfiguration = new SmsConfiguration()
            {
                Password = "123456"
            };
            _configurationConnectorMock.Setup(x => x.Read()).ReturnsAsync(
               new Configuration()
               {
                   SmsConfiguration = smsConfiguration
               });
            Configuration configuration = new Configuration();
            configuration.SmsConfiguration.Password = "122";


            await _appConfigurationsHandler.Update(configuration);

            Assert.Equal("Fixed", configuration.SmsConfiguration.Password);

        }


        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task Update_NotNeedToUpdateSmsConfigurationPassword_Succsess(bool useActiveDirectory, bool setupConfig)
        {
            SetupLicenseMocker(useActiveDirectory);

            if (setupConfig)
            {
                _activeDirectoryConfigConnector.Setup(x => x.Read()).ReturnsAsync(new ActiveDirectoryConfiguration());
            }

            _encryptorMock.Setup(x => x.Encrypt(It.IsAny<string>())).Returns("Fixed");
            SmsConfiguration smsConfiguration = new SmsConfiguration()
            {
                Password = "123456"
            };
            _configurationConnectorMock.Setup(x => x.Read()).ReturnsAsync(
               new Configuration()
               {
                   SmsConfiguration = smsConfiguration
               });

            Configuration configuration = new Configuration();
            configuration.SmsConfiguration.Password = "123456";


            await _appConfigurationsHandler.Update(configuration);

            Assert.Equal("123456", configuration.SmsConfiguration.Password);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task Update_UpdateSMTPConfigurationPassword_Succsess(bool useActiveDirectory, bool setupConfig)
        {
            SetupLicenseMocker(useActiveDirectory);
            if (setupConfig)
            {
                _activeDirectoryConfigConnector.Setup(x => x.Read()).ReturnsAsync(new ActiveDirectoryConfiguration());
            }
            _encryptorMock.Setup(x => x.Encrypt(It.IsAny<string>())).Returns("Fixed");
            SmtpConfiguration smtpConfiguration = new SmtpConfiguration()
            {
                Password = "123456"
            };
            _configurationConnectorMock.Setup(x => x.Read()).ReturnsAsync(
               new Configuration()
               {
                   SmtpConfiguration = smtpConfiguration
               });
            Configuration configuration = new Configuration();
            configuration.SmtpConfiguration.Password = "122";


            await _appConfigurationsHandler.Update(configuration);

            Assert.Equal("Fixed", configuration.SmtpConfiguration.Password);

        }

        [Theory]
        [InlineData(true, true,"text-1")]
        [InlineData(false, false,"")]
        [InlineData(true, false,"")]
        [InlineData(false, true, "text-1")]
        public async Task Update_NotNeedToUpdateSMTPConfigurationPassword_Succsess(bool useActiveDirenctory, bool setupConfig,
            string appConfigurationActiveDirectoryPassword)
        {
            SetupLicenseMocker(useActiveDirenctory);
            if (setupConfig)
            {
                _activeDirectoryConfigConnector.Setup(x => x.Read()).ReturnsAsync(new ActiveDirectoryConfiguration()
                {
                    Password = appConfigurationActiveDirectoryPassword
                });
            }
            _encryptorMock.Setup(x => x.Encrypt(It.IsAny<string>())).Returns("Fixed");
            SmtpConfiguration smtpConfiguration = new SmtpConfiguration()
            {
                Password = "123456"
            };
            _configurationConnectorMock.Setup(x => x.Read()).ReturnsAsync(
               new Configuration()
               {
                   SmtpConfiguration = smtpConfiguration
               });

            Configuration configuration = new Configuration();
            configuration.SmtpConfiguration.Password = "123456";


            await _appConfigurationsHandler.Update(configuration);

            Assert.Equal("123456", configuration.SmtpConfiguration.Password);
        }


        [Fact]
        public async Task Update_SendNullInput_ThrowException()
        {
            var actual =await Assert.ThrowsAsync<Exception>(() => _appConfigurationsHandler.Update(null));

            Assert.Equal("Null input - app configuration is null", actual.Message);
        }


        private void SetupLicenseMocker(bool useActiveDirectory)
        {
            LicenseCounters zero = null ;
            var properties = new List<LicensePropertyDB>();
            _fileSystem.Setup(f => f.File.ReadAllText(It.IsAny<String>(), Encoding.UTF8)).Returns("");
            _fileSystem.Setup(f => f.Path.GetDirectoryName(It.IsAny<String>())).Verifiable();
            _fileSystem.Setup(f => f.Path.Combine(It.IsAny<String>())).Verifiable();

            _licenseMock.Setup(x => x.GetLicenseInformationAndUsing( It.IsAny<bool>())).ReturnsAsync(
                (new WeSignLicense(properties, _fileSystem.Object) { LicenseCounters = new LicenseCounters { UseActiveDirectory = useActiveDirectory } }, zero));
        }

    }
}
