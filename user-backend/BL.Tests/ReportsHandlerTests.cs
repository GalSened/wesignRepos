using BL.Handlers;
using Common.Enums.Documents;
using Common.Enums.Reports;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Reports;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Reports;
using Common.Models.Settings;
using Common.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class ReportsHandlerTests : IDisposable
    {
        private readonly IReports _reportsHandler;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IUserPeriodicReportConnector> _userPeriodicReportConnector;
        private readonly Mock<IDocumentCollectionConnector> _documentsCollectionConnector;
        private readonly Mock<IGroupConnector> _groupConnectorMock;
        private readonly Mock<IDater> _dater;
        private readonly Mock<IUsers> _users;
        private readonly Mock<IWeSignHistoryReports> _reportsHttpClientWrapper;
        private readonly IOptions<GeneralSettings> _generalSettings;
        private readonly IOptions<FolderSettings> _folderSettings;


        public void Dispose()
        {
            _serviceScopeFactoryMock.Invocations.Clear();
            _userPeriodicReportConnector.Invocations.Clear();
            _documentsCollectionConnector.Invocations.Clear();
            _groupConnectorMock.Invocations.Clear();
            _dater.Invocations.Clear();
            _users.Invocations.Clear();
            _reportsHttpClientWrapper.Invocations.Clear();
        }

        public ReportsHandlerTests()
        {
            _userPeriodicReportConnector = new Mock<IUserPeriodicReportConnector>();
            _documentsCollectionConnector = new Mock<IDocumentCollectionConnector>();
            _groupConnectorMock = new Mock<IGroupConnector>();
            _dater = new Mock<IDater>();
            _users = new Mock<IUsers>();
            _reportsHttpClientWrapper = new Mock<IWeSignHistoryReports>();
            _generalSettings = Options.Create(new GeneralSettings());
            _folderSettings = Options.Create(new FolderSettings());

            var serviceCollection = new ServiceCollection();

            // Add any DI stuff here:
            serviceCollection.AddSingleton(_userPeriodicReportConnector.Object);
            serviceCollection.AddSingleton(_documentsCollectionConnector.Object);
            serviceCollection.AddSingleton(_groupConnectorMock.Object);
            // Create the ServiceProvider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // serviceScopeMock will contain my ServiceProvider
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider)
                .Returns(serviceProvider);

            // serviceScopeFactoryMock will contain my serviceScopeMock
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeFactoryMock.Setup(s => s.CreateScope())
                .Returns(serviceScopeMock.Object);

            _reportsHandler = new ReportsHandler(_serviceScopeFactoryMock.Object, _dater.Object, _users.Object, _reportsHttpClientWrapper.Object, _generalSettings, _folderSettings);
        }

        #region CreateFrequencyReport
        [Fact]
        public async Task CreateFrequencyReport_UserNotExist_ThrowException()
        {
            // Arrange
            ReportFrequency reportFrequency = ReportFrequency.None;
            ReportType reportType = ReportType.UsageData;
            _users.Setup(_ => _.GetUser()).ReturnsAsync((default(User), default(CompanySigner1Details)));

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _reportsHandler.CreateFrequencyReport(reportFrequency, reportType));

            // Assert
            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public void CreateFrequencyReport_Valid_ShouldSuccess()
        {
            // Arrange
            User user = new User();
            ReportFrequency reportFrequency = ReportFrequency.None;
            ReportType reportType = ReportType.UsageData;
            _users.Setup(_ => _.GetUser()).ReturnsAsync((user, default(CompanySigner1Details)));
            _userPeriodicReportConnector.Setup(_ => _.Create(It.IsAny<UserPeriodicReport>()));

            // Action
            _reportsHandler.CreateFrequencyReport(reportFrequency, reportType);

            // Assert
            _userPeriodicReportConnector.Verify(_ => _.Create(It.IsAny<UserPeriodicReport>()), Times.Once);
        }
        #endregion

        #region ReadDocumentsReports

        [Fact]
        public async Task ReadDocumentsReports_InvalidUser_ThrowException()
        {
            // Arrange
            _users.Setup(_ => _.GetUser()).ReturnsAsync((default(User), default(CompanySigner1Details)));

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _reportsHandler.ReadUsageData(null, null, null, null, true, 0, 0));

            // Assert
            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadDocumentsReports_InvalidOffset_ThrowException()
        {
            // Arrange
            User user = new User();
            _users.Setup(_ => _.GetUser()).ReturnsAsync((user, default(CompanySigner1Details)));
            _dater.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _reportsHandler.ReadUsageData(null, null, null, null, true, -1, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadDocumentsReports_InvalidLimit_ThrowException()
        {
            // Arrange
            User user = new User();
            _users.Setup(_ => _.GetUser()).ReturnsAsync((user, default(CompanySigner1Details)));
            _dater.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _reportsHandler.ReadUsageData(null, null, null, null, true, 0, -1));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadDocumentsReports_GroupNotBelongToUser_ThrowException()
        {
            // Arrange
            User user = new User();
            var groupIds = new List<Guid>() { Guid.NewGuid() };
            List<Group> emptyList = new List<Group>();
            _users.Setup(_ => _.GetUser()).ReturnsAsync((user, default(CompanySigner1Details)));
            _dater.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            _users.Setup(_ => _.GetUserGroups()).ReturnsAsync(emptyList);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _reportsHandler.ReadUsageData(null, null, null, groupIds, true, 0, 0));

            // Assert
            Assert.Equal(ResultCode.GroupNotBelongToUser.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadDocumentsReports_Valid_ShouldSuccess()
        {
            var groupId = Guid.NewGuid();
            var groupName = "test";
            Dictionary<Guid, string> groups = new Dictionary<Guid, string>()
            {
                { groupId, groupName }
            };
            // Arrange
            var reports = new List<UsageDataReport>() { new UsageDataReport() {
            GroupId = groupId,
            GroupName = "test"
            } };
            User user = new User();
            List<Group> emptyList = new List<Group>();
            _users.Setup(_ => _.GetUser()).ReturnsAsync((user, default(CompanySigner1Details)));
            _dater.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            _users.Setup(_ => _.GetUserGroups()).ReturnsAsync(emptyList);
            _documentsCollectionConnector.Setup(_ => _.ReadUserUsageDataReports(It.IsAny<Guid>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<DocumentStatus>>(),
                It.IsAny<List<Guid>>(), It.IsAny<bool>())).Returns(reports);
            _reportsHttpClientWrapper.Setup(_ => _.ReadUsageDataReports(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),  
                It.IsAny<IEnumerable<DocumentStatus>>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()));
            _groupConnectorMock.Setup(_ => _.GetGroupIdNameDictionary(It.IsAny<List<Guid>>())).Returns(groups);

            // Action
            var response = await _reportsHandler.ReadUsageData(null, null, null, null, true, 0, 20);

            // Assert
            Assert.Equal(reports.Count, response.Count());
        }
        #endregion

    }
}
