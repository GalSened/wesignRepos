namespace BL.Tests
{
    using BL.Handlers;
    using Common.Interfaces.DB;
    using Common.Interfaces.Emails;
    using Common.Models.Settings;
    using Microsoft.Extensions.Options;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Reflection;

    public class EmailHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";

        private IFileSystem _fileSystemMock;
        private IOptions<FolderSettings> _folderSettingsMock;
        private IOptions<GeneralSettings> _generalSettingsMock;
        private Mock<IEmailProvider> _emailProviderMock;
        private Mock<IDbConnector> _dbConnectorMock;


       // private EmailMessageHandler _emailHandler;

        //public EmailHandlerTests()
        //{
        //    _folderSettingsMock = Options.Create(new FolderSettings() { });
        //    _generalSettingsMock = Options.Create(new GeneralSettings() { });
        //    _emailProviderMock = new Mock<IEmailProvider>();
        //    _dbConnectorMock = new Mock<IDbConnector>();
        //    var context = new DefaultHttpContext();
        //    context.Request.Path = "/v3/api/users";
        //    context.Request.Scheme = "https";
        //    context.Request.Host = new HostString("localhost", 44300);
        //}

        public void Dispose()
        {
            _emailProviderMock.Invocations.Clear();

        }

        //#region Activation

        //[Fact]
        //public void Activation_NoFiles_ReturnException()
        //{
        //    _fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData> { });
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object, _dbConnectorMock.Object);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.Activation(user));

        //    Assert.Equal("Failed to send activation link mail, Email body template or logo or json not exist in system", actual.Message);
        //}

        //[Fact]
        //public void Activation_FilesExistsSmtpConfigMissing_ReturnException()
        //{
        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object,  _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Throws(new InvalidOperationException("Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)"));
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.Activation(user));

        //    Assert.Equal("Failed to send activation link mail, Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)", actual.Message);
        //}

        //[Fact]
        //public void ResetPassword_NoFiles_ReturnException()
        //{
        //    _fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData> { });
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object, _dbConnectorMock.Object);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    string token = "token";

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.ResetPassword(user, token));

        //    Assert.Equal("Failed to send resetPassword mail, Email body template or logo or json not exist in system", actual.Message);
        //}

        //[Fact]
        //public void ResetPassword_FilesExistsSmtpConfigMissing_ReturnException()
        //{
        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object,  _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Throws(new InvalidOperationException("Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)"));
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    string token = "token";

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.ResetPassword(user, token));

        //    Assert.Equal("Failed to send resetPassword mail, Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)", actual.Message);
        //}

        //[Fact]
        //public void ResetPassword_ValidUser_Success()
        //{
        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object, _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Verifiable();
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    string token = "token";

        //    _emailHandler.ResetPassword(user, token);
        //}

        //[Fact]
        //public void ClientSigning_NoFiles_ReturnException()
        //{
        //    _fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData> { });
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object,  _dbConnectorMock.Object);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.ClientSigning(user));

        //    Assert.Equal("Failed to send ClientSigning mail, Email body template or logo or json not exist in system", actual.Message);
        //}

        //[Fact]
        //public void ClientSigning_FilesExistsSmtpConfigMissing_ReturnException()
        //{
        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object, _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Throws(new InvalidOperationException("Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)"));
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.ClientSigning(user));

        //    Assert.Equal("Failed to send ClientSigning mail, Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)", actual.Message);
        //}

        //[Fact]
        //public void ClientSigning_ValidUser_Success()
        //{
        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object,  _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Verifiable();
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    _emailHandler.ClientSigning(user);
        //}

        //[Fact]
        //public void SignedDocs_NoFiles_ReturnException()
        //{
        //    _fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData> { });
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object,  _dbConnectorMock.Object);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.SignedDocs(user));

        //    Assert.Equal("Failed to send SignedDocs mail, Email body template or logo or json not exist in system", actual.Message);
        //}

        //[Fact]
        //public void SignedDocs_FilesExistsSmtpConfigMissing_ReturnException()
        //{

        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object,  _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Throws(new InvalidOperationException("Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)"));
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.SignedDocs(user));

        //    Assert.Equal("Failed to send SignedDocs mail, Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)", actual.Message);
        //}

        //[Fact]
        //public void SignedDocs_ValidUser_Success()
        //{
        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmsConfiguration = new Common.Models.Sms.SmsConfiguration() });
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object,  _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Verifiable();
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    _emailHandler.SignedDocs(user);
        //}

        //[Fact]
        //public void AllParticipantesSignedNotification_NoFiles_ReturnException()
        //{
        //    _fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData> { });
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object, _dbConnectorMock.Object);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.AllParticipantesSignedNotification(user));

        //    Assert.Equal("Failed to send AllParticipantesSignedNotification mail, Email body template or logo or json not exist in system", actual.Message);
        //}

        //[Fact]
        //public void AllParticipantesSignedNotification_FilesExistsSmtpConfigMissing_ReturnException()
        //{

        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object, _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Throws(new InvalidOperationException("Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)"));
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.AllParticipantesSignedNotification(user));

        //    Assert.Equal("Failed to send AllParticipantesSignedNotification mail, Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)", actual.Message);
        //}

        //[Fact]
        //public void ClientSignedNotification_ValidUser_Success()
        //{
        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmsConfiguration = new Common.Models.Sms.SmsConfiguration() });
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object,  _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Verifiable();
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    _emailHandler.ClientSignedNotification(user);
        //}

        //[Fact]
        //public void ClientSignedNotification_NoFiles_ReturnException()
        //{
        //    _fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData> { });
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object,  _dbConnectorMock.Object);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.ClientSignedNotification(user));

        //    Assert.Equal("Failed to send ClientSignedNotification mail, Email body template or logo or json not exist in system", actual.Message);
        //}

        //[Fact]
        //public void ClientSignedNotification_FilesExistsSmtpConfigMissing_ReturnException()
        //{

        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object, _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Throws(new InvalidOperationException("Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)"));
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    var actual = Assert.Throws<InvalidOperationException>(() => _emailHandler.ClientSignedNotification(user));

        //    Assert.Equal("Failed to send ClientSignedNotification mail, Failed to send mail using SMTP client, Smtp configuration missing (server, port or from address)", actual.Message);
        //}

        //[Fact]
        //public void AllParticipantesSignedNotification_ValidUser_Success()
        //{
        //    _fileSystemMock = CreateValidMockFileSystem();
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmsConfiguration = new Common.Models.Sms.SmsConfiguration() });
        //    _emailHandler = new EmailHandler(_folderSettingsMock, _generalSettingsMock, _fileSystemMock, _emailProviderMock.Object, _dbConnectorMock.Object);
        //    string currecntfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    var user = new User()
        //    {
        //        Id = new Guid(GUID),
        //        Name = "User Name",
        //        Email = "userName@comda.co.il",
        //        UserConfiguration = new UserConfiguration()
        //        {
        //            Language = UserLanguage.en
        //        }
        //    };
        //    var smtpConfiguration = new SmtpConfiguration();
        //    _emailProviderMock.Setup(x => x.Send(It.IsAny<Email>(), It.IsAny<SmtpConfiguration>())).Verifiable();
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration() { SmtpConfiguration = new SmtpConfiguration() });

        //    _emailHandler.AllParticipantesSignedNotification(user);
        //}
//#endregion








        #region Private Function

        private MockFileSystem CreateValidMockFileSystem()
        {
            string currentFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return new MockFileSystem(new Dictionary<string, MockFileData> {
                {Path.Combine(currentFolder,"Resources/Emails.en.json"), new MockFileData( @"{
                          ""ActivationLinkText"": ""VERIFY"",
                          ""ActivationSubject"": ""Activate your Comsign Trust Online account"",
                          ""ActivationText"": ""Before you send your first document to be signed,<BR />please take a moment to verify your email address"",  
                          ""Copyright"": ""&copy; Copyright 2018 - Digital Signature and eSignature software solutions -"",
                          ""Digital"": "" Advanced digital signature solutions and electronic approved signature"",  
                          ""Hello"": ""Hello, "",  
                          ""ForSigning"": ""for signing "",  
                          ""SentYou"": ""has sent you a document "",  
                          ""Visit"": ""visit our group&apos;s companies:""
                       }") },
                {Path.Combine(currentFolder,"Resources/EmailBody.html"), new MockFileData("<a abcd.pdf a>")},
                {Path.Combine(currentFolder,"Resources/Logo.png"), new MockFileData("") }
            });
        }

        #endregion
    }
}
