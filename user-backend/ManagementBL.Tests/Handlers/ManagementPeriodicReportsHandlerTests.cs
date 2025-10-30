using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers.SendingMessages.Mail;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.MessageSending;
using Common.Interfaces.Reports;
using Common.Models;
using Common.Models.ManagementApp.Reports;
using Common.Models.Settings;
using ManagementBL.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Jwt;
using Xunit;

namespace ManagementBL.Tests.Handlers
{
    public class ManagementPeriodicReportsHandlerTests : IDisposable
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactory;
        private readonly Mock<IReport> _reports;
        private readonly Mock<IMessageSender> _messageSender;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IDater> _dater;
        private readonly Mock<IServiceScope> _scope;
        private readonly Mock<IUserConnector> _userConnector;
        private readonly Mock<ICompanyConnector> _companyConnector;
        private readonly Mock<IPeriodicReportFileConnector> _persiodicReportFileConnector;
        private readonly IOptions<FolderSettings> _folderSettings;
        private readonly IOptions<GeneralSettings> _generalSettings;
        private readonly IManagementPeriodicReports _managementPeriodicReports;

        public ManagementPeriodicReportsHandlerTests()
        {
            _scopeFactory = new Mock<IServiceScopeFactory>();
            _reports = new Mock<IReport>();
            _messageSender = new Mock<IMessageSender>();
            _logger = new Mock<ILogger>();
            _dater = new Mock<IDater>();
            _scope = new Mock<IServiceScope>();
            _userConnector = new Mock<IUserConnector>();
            _companyConnector = new Mock<ICompanyConnector>();
            _persiodicReportFileConnector = new Mock<IPeriodicReportFileConnector>();
            _folderSettings = Options.Create(new FolderSettings());
            _generalSettings = Options.Create(new GeneralSettings());
            _scopeFactory.Setup(_ => _.CreateScope()).Returns(_scope.Object);
            _scope.Setup(_ => _.ServiceProvider.GetService(typeof(IUserConnector))).Returns(_userConnector.Object);
            _scope.Setup(_ => _.ServiceProvider.GetService(typeof(ICompanyConnector))).Returns(_companyConnector.Object);
            _scope.Setup(_ => _.ServiceProvider.GetService(typeof(IPeriodicReportFileConnector))).Returns(_persiodicReportFileConnector.Object);
            _managementPeriodicReports = new ManagementPeriodicReportsHandler(_scopeFactory.Object, _reports.Object, _messageSender.Object, _logger.Object, _dater.Object, _folderSettings, _generalSettings);
        }

        public void Dispose()
        {
            _scopeFactory.Invocations.Clear();
            _reports.Invocations.Clear();
            _messageSender.Invocations.Clear();
            _logger.Invocations.Clear();
            _dater.Invocations.Clear();
            _userConnector.Invocations.Clear();
            _companyConnector.Invocations.Clear();
            _persiodicReportFileConnector.Invocations.Clear();  
        }

        #region SendManagementReportToUsers
        [Fact]
        public async void SendManagementReportToUsers_UserNotExist_ThrowException()
        {
            // Arrange
            User user = null;
            var report = new ManagementPeriodicReport();
            _userConnector.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(user);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _managementPeriodicReports.SendManagementReportToUsers(null, report, null));

            // Assert
            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async void SendManagementReportToUsers_CompanyNotExist_ThrowException()
        {
            // Arrange
            User user = new User();
            Company company = null;
            var report = new ManagementPeriodicReport();
            _userConnector.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companyConnector.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _managementPeriodicReports.SendManagementReportToUsers(null, report, null));

            // Assert
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        #endregion
    }
}
