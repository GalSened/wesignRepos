using BL.Handlers;
using Common.Enums;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
using Common.Handlers.SendingMessages;
using Common.Handlers.SendingMessages.Mail;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.MessageSending;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class SignersHandlerTests : IDisposable
    {
        private readonly Mock<IUsers> _usersMock;
        private readonly Mock<IValidator> _validatorsMock;
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<IConfigurationConnector> _configurationConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IGenerateLinkHandler> _generateLinkHandlerMock;
        private readonly Mock<IDocumentCollectionOperations> _documentCollectionOperationsMock;
        private readonly Mock<ISendingMessageHandler> _sendingMessageHandlerMock;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnectorMock;
        private readonly Mock<IContacts> _contactsMock;
        private readonly ISigners _signers;

        public SignersHandlerTests()
        {
            _usersMock = new Mock<IUsers>();
            _validatorsMock = new Mock<IValidator>();
            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();
            _daterMock = new Mock<IDater>();
            _configurationConnectorMock = new Mock<IConfigurationConnector>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _configurationMock = new Mock<IConfiguration>();
            _generateLinkHandlerMock = new Mock<IGenerateLinkHandler>();
            _documentCollectionOperationsMock = new Mock<IDocumentCollectionOperations>();
            _sendingMessageHandlerMock = new Mock<ISendingMessageHandler>();
            _programConnectorMock = new Mock<IProgramConnector>();
            _programUtilizationConnectorMock = new Mock<IProgramUtilizationConnector>();
            _contactsMock = new Mock<IContacts>();
            _signers = new SignersHandler(_usersMock.Object, _validatorsMock.Object, _documentCollectionConnectorMock.Object,
                _daterMock.Object, _configurationConnectorMock.Object, _companyConnectorMock.Object,
                _configurationMock.Object, _generateLinkHandlerMock.Object,
                _documentCollectionOperationsMock.Object, _sendingMessageHandlerMock.Object,
                _programConnectorMock.Object, _programUtilizationConnectorMock.Object, _contactsMock.Object);
        }

        public void Dispose()
        {
            _usersMock.Invocations.Clear();
            _validatorsMock.Invocations.Clear();
            _documentCollectionConnectorMock.Invocations.Clear();
            _daterMock.Invocations.Clear();
            _configurationConnectorMock.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
            _configurationMock.Invocations.Clear();
            _generateLinkHandlerMock.Invocations.Clear();
            _documentCollectionOperationsMock.Invocations.Clear();
            _sendingMessageHandlerMock.Invocations.Clear();
            _programConnectorMock.Invocations.Clear();
            _programUtilizationConnectorMock.Invocations.Clear();
            _contactsMock.Invocations.Clear();
        }

        #region ReplaceSigner

        [Fact]
        public async Task ReplaceSigner_InvalidDocumentCollectionId_ThrowsException()
        {
            // Arrange
            DocumentCollection documentCollection = null;
            _usersMock.Setup(_ => _.GetUser());
            _validatorsMock.Setup(_ => _.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(_ => _.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _signers.ReplaceSigner(Guid.Empty, Guid.Empty, null, null, null, 
                OtpMode.None, null, AuthMode.None));

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReplaceSigner_DocumentNotBelongToUserGroup_ThrowsException()
        {
            // Arrange
            DocumentCollection documentCollection = new DocumentCollection();
            User user = new User();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, null));
            _validatorsMock.Setup(_ => _.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(_ => _.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _signers.ReplaceSigner(Guid.Empty, Guid.Empty, null, null, null,
                OtpMode.None, null, AuthMode.None));

            // Assert
            Assert.Equal(ResultCode.DocumentNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReplaceSigner_InvalidSignerId_ThrowsException()
        {
            // Arrange
            Guid groupId = Guid.NewGuid();
            DocumentCollection documentCollection = new DocumentCollection() { GroupId = groupId };
            User user = new User() { GroupId = groupId };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, null));
            _validatorsMock.Setup(_ => _.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(_ => _.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _signers.ReplaceSigner(Guid.Empty, Guid.Empty, null, null, null,
                OtpMode.None, null, AuthMode.None));

            // Assert
            Assert.Equal(ResultCode.InvalidSignerId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReplaceSigner_DocumentAlreadySignedBySigner_ThrowsException()
        {
            // Arrange
            Guid groupId = Guid.NewGuid();
            Guid signerId = Guid.NewGuid();
            var signer = new Signer() { Id = signerId, Status = SignerStatus.Sent };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                GroupId = groupId,
                Signers = new List<Signer>() { signer },
                DocumentStatus = DocumentStatus.Signed
            };
            User user = new User() { GroupId = groupId };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, null));
            _validatorsMock.Setup(_ => _.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(_ => _.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _signers.ReplaceSigner(Guid.Empty, signerId, null, null, null,
                OtpMode.None, null, AuthMode.None));

            // Assert
            Assert.Equal(ResultCode.DocumentAlreadySignedBySigner.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReplaceSigner_VisualIdentificationsExceedLicenseLimit_ThrowsException()
        {
            // Arrange
            Guid groupId = Guid.NewGuid();
            Guid signerId = Guid.NewGuid();
            var signer = new Signer() 
            {
                Id = signerId, 
                Status = SignerStatus.Sent,
                SignerAuthentication = new SignerAuthentication()
                {
                    AuthenticationMode = AuthMode.None
                }
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                GroupId = groupId,
                Signers = new List<Signer>() { signer },
                DocumentStatus = DocumentStatus.Sent
            };
            User user = new User() { GroupId = groupId };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, null));
            _validatorsMock.Setup(_ => _.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(_ => _.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _programConnectorMock.Setup(_ => _.CanAddVisualIdentifications(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(false);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _signers.ReplaceSigner(Guid.Empty, signerId, null, null, null,
                OtpMode.None, null, AuthMode.ComsignVisualIDP));

            // Assert
            Assert.Equal(ResultCode.VisualIdentificationsExceedLicenseLimit.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReplaceSigner_Valid_ShouldSuccess()
        {
            // Arrange
            Guid groupId = Guid.NewGuid();
            Guid signerId = Guid.NewGuid();
            var prevSigner = new Signer()
            {
                Status = SignerStatus.Sent
            };
            var signer = new Signer()
            {
                Id = signerId,
                Status = SignerStatus.Sent
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                GroupId = groupId,
                Signers = new List<Signer>() { prevSigner, signer },
                DocumentStatus = DocumentStatus.Sent,
                Mode = DocumentMode.OrderedGroupSign
            };
            User user = new User() { GroupId = groupId };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, null));
            _validatorsMock.Setup(_ => _.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(_ => _.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _programConnectorMock.Setup(_ => _.CanAddVisualIdentifications(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(false);

            // Action
            await _signers.ReplaceSigner(Guid.Empty, signerId, null, null, null,
                OtpMode.None, null, AuthMode.None);

            // Assert
            _documentCollectionConnectorMock.Verify(_ => _.Update(It.IsAny<DocumentCollection>()), Times.Once);
        }

        #endregion
    }
}
