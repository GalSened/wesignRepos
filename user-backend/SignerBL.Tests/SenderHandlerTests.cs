using Common.Enums;
using Common.Enums.Documents;
using Common.Handlers.SendingMessages;
using Common.Interfaces;
using Common.Interfaces.MessageSending;
using Common.Interfaces.MessageSending.Mail;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Moq;
using SignerBL.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SignerBL.Tests
{
    public class SenderHandlerTests : IDisposable
    {
        private readonly Mock<ISendingMessageHandler> _sendingMessageHandlerMock;
        private readonly Mock<IGenerateLinkHandler> _generateLinkHandlerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IEmailTypeHandler> _emailTypeHandlerMock;
        private readonly Mock<IMessageSender> _messageSender;
        private ISender _senderHandler;

        public SenderHandlerTests()
        {
            _emailTypeHandlerMock = new Mock<IEmailTypeHandler>();
            _sendingMessageHandlerMock = new Mock<ISendingMessageHandler>();
            _generateLinkHandlerMock = new Mock<IGenerateLinkHandler>();
            _configurationMock = new Mock<IConfiguration>();
            _messageSender = new Mock<IMessageSender>();
            _senderHandler = new SenderHandler(_configurationMock.Object, _sendingMessageHandlerMock.Object, _generateLinkHandlerMock.Object);
        }

        public void Dispose()
        {
            _sendingMessageHandlerMock.Invocations.Clear();
            _generateLinkHandlerMock.Invocations.Clear();
            _configurationMock.Invocations.Clear();
        }

        #region SendSigningLinkToNextSigner

        [Fact]
        public async Task SendSigningLinkToNextSigner_WhenSignerSendingMethodIsEmail_ShouldReplaceMessageLinkPlaceholderWithEmptyString()
        {
            // Arrange

            User user = new User();
            DocumentCollection documentCollection = new DocumentCollection() { User = user, Name = "name" };
            Configuration configuration = new Configuration();
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Signer signer = new Signer() { SendingMethod = Common.Enums.Documents.SendingMethod.Email };

            _configurationMock.Setup(x => x.GetBeforeMessage(documentCollection.User, configuration, companyConfiguration)).Returns($"message[LINK]");
            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(signer.SendingMethod)).Returns(new EmailHandler(_configurationMock.Object, _emailTypeHandlerMock.Object));
            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(signer.SendingMethod)).Returns(_messageSender.Object);

            SignerLink signerLink = new SignerLink() { SignerId = Guid.Empty, Link = "" };
            List<SignerLink> signerLinks = new List<SignerLink>() { signerLink };
            _generateLinkHandlerMock.Setup(x => x.GenerateSigningLink(documentCollection, documentCollection.User, companyConfiguration, true)).ReturnsAsync(signerLinks);

            List<MessageInfo> result = new List<MessageInfo>();
            _messageSender.Setup(x => x.Send(configuration, companyConfiguration, It.IsAny<MessageInfo>()))
           .Callback<Configuration, CompanyConfiguration, MessageInfo>((configuration, companyComfiguration, messageInfo) => result.Add(messageInfo));

            // Action
            await _senderHandler.SendSigningLinkToNextSigner(documentCollection, configuration, companyConfiguration, signer);

            // Assert
            Assert.Equal("message", result[0].MessageContent);
        }
        [Fact]
        public async Task SendSigningLinkToNextSigner_WhenSignerSendingMethodIsSMS_ShouldReplaceMessageLinkPlaceholderWithSignerLink()
        {
            // Arrange
            Guid signerSampleId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            User user = new User();
            DocumentCollection documentCollection = new DocumentCollection() { User = user, Name = "name" };
            Configuration configuration = new Configuration();
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Signer signer = new Signer() { SendingMethod = Common.Enums.Documents.SendingMethod.SMS, Id = signerSampleId };

            _configurationMock.Setup(x => x.GetBeforeMessage(documentCollection.User, configuration, companyConfiguration)).Returns($"message[LINK]");
            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(signer.SendingMethod)).Returns(new EmailHandler(_configurationMock.Object, _emailTypeHandlerMock.Object));
            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(signer.SendingMethod)).Returns(_messageSender.Object);

            SignerLink signerLink = new SignerLink() { SignerId = signerSampleId, Link = "changed" };
            List<SignerLink> signerLinks = new List<SignerLink>() { signerLink };
            _generateLinkHandlerMock.Setup(x => x.GenerateSigningLink(documentCollection, documentCollection.User, companyConfiguration, true)).ReturnsAsync(signerLinks);

            List<MessageInfo> result = new List<MessageInfo>();
            _messageSender.Setup(x => x.Send(configuration, companyConfiguration, It.IsAny<MessageInfo>()))
          .Callback<Configuration, CompanyConfiguration, MessageInfo>((configuration, companyComfiguration, messageInfo) => result.Add(messageInfo));

            // Action
            await _senderHandler.SendSigningLinkToNextSigner(documentCollection, configuration, companyConfiguration, signer);

            // Assert
            Assert.Equal("messagechanged", result[0].MessageContent);
        }
        [Fact]
        public async Task SendSigningLinkToNextSigner_WhenSignerIsEmptyOrSignerSendingMethodIsNotSmsOrEmail_ShouldSendMessageInfoWithEmptyMessageContent()
        {
            // Arrange


            User user = new User();
            DocumentCollection documentCollection = new DocumentCollection() { User = user, Name = "name" };
            Configuration configuration = new Configuration();
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Signer signer = new Signer();

            _configurationMock.Setup(x => x.GetBeforeMessage(documentCollection.User, configuration, companyConfiguration)).Returns($"message[LINK]");
            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(signer.SendingMethod)).Returns(new EmailHandler(_configurationMock.Object, _emailTypeHandlerMock.Object));
            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(signer.SendingMethod)).Returns(_messageSender.Object);

            SignerLink signerLink = new SignerLink() { SignerId = Guid.Empty, Link = "" };
            List<SignerLink> signerLinks = new List<SignerLink>() { signerLink };
            _generateLinkHandlerMock.Setup(x => x.GenerateSigningLink(documentCollection, documentCollection.User, companyConfiguration, true)).ReturnsAsync(signerLinks);

            List<MessageInfo> result = new List<MessageInfo>();

            _messageSender.Setup(x => x.Send(configuration, companyConfiguration, It.IsAny<MessageInfo>()))
                .Callback<Configuration, CompanyConfiguration, MessageInfo>((configuration, companyComfiguration, messageInfo) => result.Add(messageInfo));

            // Action
            await _senderHandler.SendSigningLinkToNextSigner(documentCollection, configuration, companyConfiguration, signer);

            // Assert
            Assert.Equal("", result[0].MessageContent);
        }



        #endregion

        #region SendEmailNotification

        [Fact]
        public async Task SendEmailNotification_WhenInputsAreValid_ShouldCallSendMethod()
        {
            // Arrange 

            MessageType messageType = MessageType.Decline;
            DocumentCollection documentCollection = new DocumentCollection();
            Configuration configuration = new Configuration();
            Signer signer = new Signer();
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();

            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(_messageSender.Object);

            // Action 
            await _senderHandler.SendEmailNotification(messageType, documentCollection, configuration, signer, companyConfiguration);

            // Assert
            _messageSender.Verify(x => x.Send(It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>(), It.IsAny<MessageInfo>()));
        }

        #endregion

        #region SendSignedDocument

        [Fact]
        public async Task SendSignedDocument_ShouldReturnEmptyString_WhenConfigurationShouldSendSignedDocumentIsFalse()
        {
            // Arrange
            DocumentCollection documentCollection = new DocumentCollection();
            Configuration configuration = new Configuration();
            Signer dbSigner = new Signer();
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();

            _configurationMock.Setup(x => x.ShouldSendSignedDocument(It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<DocumentNotifications>())).Returns(false);

            // Action
            var action =await _senderHandler.SendSignedDocument(documentCollection, configuration, dbSigner, companyConfiguration);

            // Assert
            Assert.Equal("", action);

        }

        [Fact]
        public async Task SendingSignedDocument_ShouldReturnSignerLink_WhenConfigurationShouldSendSignedDocumentsIsTrue()
        {
            // Arrange
            User user = new User();

            DocumentCollection documentCollection = new DocumentCollection() { User = user };
            Configuration configuration = new Configuration();
            Signer dbSigner = new Signer() { SendingMethod = Common.Enums.Documents.SendingMethod.Tablet };
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();

            _configurationMock.Setup(x => x.ShouldSendSignedDocument(It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<DocumentNotifications>()))
                .Returns(true);
            _configurationMock.Setup(x => x.GetAfterMessage(It.IsAny<User>(), It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>()))
                .Returns("messageContent");
            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(_messageSender.Object);
            _generateLinkHandlerMock.Setup(x => x.GenerateDocumentDownloadLink(It.IsAny<DocumentCollection>(), It.IsAny<Signer>(), It.IsAny<User>(), It.IsAny<CompanyConfiguration>()))
                .ReturnsAsync(new SignerLink() { Link = "link" });

            // Action
            var action = await _senderHandler.SendSignedDocument(documentCollection, configuration, dbSigner, companyConfiguration);

            // Assert
            Assert.Equal("link", action);

        }

        #endregion

        #region SendOtpCode

        [Fact]
        public async Task SendOtpCode_WhenSignerContactIsntNullAndSignerHasContactPhone_ShouldReturnContactPhone()
        {
            // Arrange
            string otpMessage = "Your validation code is [OTP_CODE] and valid for the next 5 minutes";
            User user = new User()
            {
                UserConfiguration = new UserConfiguration()
                {
                    Language = Common.Enums.Users.Language.en
                }
            };

            Configuration appConfiguration = new Configuration();
            Contact contact = new Contact() { Phone = "0525050505" };
            Signer dbSigner = new Signer() { SendingMethod = Common.Enums.Documents.SendingMethod.SMS, Contact = contact };
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company company = new Company() { CompanyConfiguration = companyConfiguration };

            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(_messageSender.Object);
            _configurationMock.Setup(x => x.GetOtpMessgae(user, It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>())).Returns(otpMessage);

            // Action

            var action = await _senderHandler.SendOtpCode(appConfiguration, dbSigner, user, company);

            // Assert
            Assert.Equal(dbSigner.Contact.Phone, action);
        }
        [Fact]
        public async Task SendOtpCode_WhenSignerContactIsntNullAndSignerSendingMethodIsEmail_ShouldReturnContactEmail()
        {
            string otpMessage = "Your validation code is [OTP_CODE] and valid for the next 5 minutes";
            User user = new User()
            {
                UserConfiguration = new UserConfiguration()
                {
                    Language = Common.Enums.Users.Language.en
                }
            };
            Signer signer = new Signer()
            {
                SignerAuthentication = new SignerAuthentication()
                {
                    OtpDetails = new OtpDetails()
                    {
                        Code = "111111"
                    },
                },
                Contact = new Contact()
                {
                    DefaultSendingMethod = SendingMethod.Email,
                    Email = "aaa@comda.co.il"
                }
            };
            Company company = new Company()
            {
                CompanyConfiguration = new CompanyConfiguration() { }
            };

            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(_messageSender.Object);
            _configurationMock.Setup(x => x.GetOtpMessgae(user, null, null)).Returns(otpMessage);

            var actual = await _senderHandler.SendOtpCode(null, signer, user, company);
            string expected = "aaa@comda.co.il";
            Assert.Equal(expected, actual);

        }
        [Fact]
        public async Task SendOtpCode_WhenSignerContactIsntNullAndSignerMethodIsTablet_ShouldReturnContactEmail()
        {
            // Arrange
            User user = new User()
            {
                UserConfiguration = new UserConfiguration()
                {
                    Language = Common.Enums.Users.Language.en
                }
            };

            Configuration appConfiguration = new Configuration();
            Contact contact = new Contact() { Email = "" };
            Signer dbSigner = new Signer()
            {
                SendingMethod = Common.Enums.Documents.SendingMethod.Tablet,
                Contact = contact,
                SignerAuthentication = new SignerAuthentication()
                {
                    OtpDetails = new OtpDetails()
                    {
                        Code = ""
                    }
                }
            };
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company company = new Company() { CompanyConfiguration = companyConfiguration };

            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(_messageSender.Object);
            _configurationMock.Setup(x => x.GetOtpMessgae(user, It.IsAny<Configuration>(), null)).Returns("");

            //   Action

            var action =await  _senderHandler.SendOtpCode(appConfiguration, dbSigner, user, company);

            //   Assert
            Assert.Equal(dbSigner.Contact.Email, action);

        }
        [Fact]
        public async Task SendOtpCode_WhenSignerHasNoContact_ShouldReturnNull()
        {

            //  Arrange
            User user = new User()
            {
            };

            Configuration appConfiguration = new Configuration();

            Signer dbSigner = new Signer() { SendingMethod = Common.Enums.Documents.SendingMethod.Tablet };
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company company = new Company() { CompanyConfiguration = companyConfiguration };

            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(_messageSender.Object);
            _configurationMock.Setup(x => x.GetOtpMessgae(user, It.IsAny<Configuration>(), null)).Returns("");

            //   Action

            var action =await _senderHandler.SendOtpCode(appConfiguration, dbSigner, user, company);

            //Assert
            Assert.Null(action);

        }

        #endregion

        #region SendDocumentDecline

        [Fact]
        public async Task SendDocumentDecline_WhenInputsAreValid_ShouldCallSendMethod()
        {
            // Arrange 
            DocumentCollection documentCollection = new DocumentCollection();
            Configuration configuration = new Configuration();
            Signer signer = new Signer();
            User user = new User();

            _sendingMessageHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(_messageSender.Object);

            // Action 
            await _senderHandler.SendDocumentDecline(documentCollection, configuration, signer, user);

            // Assert
            _messageSender.Verify(x => x.Send(It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>(), It.IsAny<MessageInfo>()));
        }

        #endregion

    }







}

