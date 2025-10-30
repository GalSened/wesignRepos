using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models.Configurations;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Common.Tests.Handlers
{

    public class ActiveDirectoryHandlerTests : IDisposable
    {
        
        private readonly Mock<IGroupConnector> _groupConnectorMock;
        private readonly Mock<IActiveDirectoryConfigConnector> _activeDirectoryConfigConnectorMock;
        private readonly Mock<IActiveDirectoryGroupsConnector> _activeDirectoryGroupsConnectorMock;
        private readonly Mock<ILicense> _license;
        private readonly Mock<IEncryptor> _encryptor;
        private Mock<ILogger> _logger;
        private IActiveDirectory _activeDirectoryHandler;

        public ActiveDirectoryHandlerTests()
        {
            _groupConnectorMock = new Mock<IGroupConnector>();
            _activeDirectoryConfigConnectorMock = new Mock<IActiveDirectoryConfigConnector>();
            _activeDirectoryGroupsConnectorMock = new Mock<IActiveDirectoryGroupsConnector>();
                _license = new Mock<ILicense>();
            _encryptor = new Mock<IEncryptor>();
            _logger = new Mock<ILogger>();

            _activeDirectoryHandler = new ActiveDirectoryHandler(_activeDirectoryGroupsConnectorMock.Object,_groupConnectorMock.Object,
                _activeDirectoryConfigConnectorMock.Object, _license.Object, _logger.Object, _encryptor.Object);
        }

        public void Dispose()
        {
            _groupConnectorMock.Invocations.Clear();
            _activeDirectoryConfigConnectorMock.Invocations.Clear();
            _activeDirectoryGroupsConnectorMock.Invocations.Clear();
            _logger.Invocations.Clear();
            _license.Invocations.Clear();
            _encryptor.Invocations.Clear();
        }

        #region ReadADGroups

        [Fact]
        public async Task ReadADGroups_NullInput_ThrowArgumentException()
        {
            ActiveDirectoryConfiguration activeDirectoryConfiguration = null;

            var actual =await  Assert.ThrowsAsync<ArgumentException>(() => _activeDirectoryHandler.ReadADGroups(activeDirectoryConfiguration ));

            Assert.Equal(" (Parameter 'activeDirectoryConfiguration')", actual.Message);
        }

        [Fact]
        public void ReadADGroups_EmptyInput_ThrowArgumentException()
        {
            //ActiveDirectoryConfiguration activeDirectoryConfiguration = new ActiveDirectoryConfiguration();

            //var actual = Assert.Throws<ArgumentException>(() => _activeDirectoryHandler.ReadADGroups(activeDirectoryConfiguration));

            //Assert.Equal(" (Parameter 'activeDirectoryConfiguration')", actual.Message);
        }

        #endregion
    }
}
