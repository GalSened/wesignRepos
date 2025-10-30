using BL.Handlers;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Dashboard;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Dashboard;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class DashboardHandlerTests : IDisposable
    {
        private readonly IDashboard _dashboardHandler;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IUsers> _usersMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IDashboardConnector> _dashboardConnectorMock;

        public DashboardHandlerTests()
        {
            _usersMock = new Mock<IUsers>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _dashboardConnectorMock = new Mock<IDashboardConnector>();

            var serviceCollection = new ServiceCollection();

            // Add any DI stuff here:
            serviceCollection.AddSingleton(_dashboardConnectorMock.Object);

            // Create the service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // ServiceScopeMock will contain my ServiceProvider
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(_ => _.ServiceProvider)
                .Returns(serviceProvider);

            // ScopeFactoryMock will contain my ServiceScopeMock
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeFactoryMock.Setup(_ => _.CreateScope())
                .Returns(serviceScopeMock.Object);

            _dashboardHandler = new DashboardHandler(_scopeFactoryMock.Object, _usersMock.Object, _memoryCacheMock.Object);
        }

        public void Dispose()
        {
            _scopeFactoryMock.Invocations.Clear();
            _usersMock.Invocations.Clear();
            _memoryCacheMock.Invocations.Clear();
        }

        #region GetDashboardView

        [Fact]
        public async Task GetDashboardView_UserNotExist_ThrowException()
        {
            // Arrange
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((null, null));
            
            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _dashboardHandler.GetDashboardView());

            // Assert
            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        #endregion
    }
}
