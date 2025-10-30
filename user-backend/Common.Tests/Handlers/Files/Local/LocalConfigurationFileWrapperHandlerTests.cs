using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using Certificate.Interfaces;

using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers.Files.Local;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Settings;
using Microsoft.Extensions.Options;

using Moq;
using Serilog;
using Xunit;


namespace Common.Tests.Handlers.Files.Local
{
    public class LocalConfigurationFileWrapperHandlerTests
    {
        private readonly Guid ID = new Guid("C32BCF3A-C273-4F98-B002-A724DE1479FE");
        private const string DEFAULT_EMAIL_TEMPLATE = "Resources/EmailBody.html";
        private const string DEFAULT_LOGO = "Resources/Logo.png";
        private readonly IFileSystem _fileSystemMock;
        private IOptions<FolderSettings> _folderSettingsMock;
        private readonly Mock<IDataUriScheme> _dataUriSchemeMock;
        private IConfigurationFileWrapper _ConfigurationFileWrapper;
        public LocalConfigurationFileWrapperHandlerTests()
        {
            _fileSystemMock = new MockFileSystem();
            _folderSettingsMock = Options.Create(new FolderSettings()
            {
                EmailTemplates = "c:\\comda\\wesign\\emailTemplates",
                CompaniesLogo = "c:\\comda\\wesign\\CompaniesLogo",
            });
            _dataUriSchemeMock = new Mock<IDataUriScheme>();
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

        }
        [Fact]
        public void DeleteCompanyEmailHtml_UserIsNull_ThrowException()
        {



            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _ConfigurationFileWrapper.DeleteCompanyEmailHtml(null));

            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);
        }
        [Fact]
        public void DeleteCompanyEmailHtml_FileNotExist_DoNothing()
        {



            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.DeleteCompanyEmailHtml(new User());

        }
        [Fact]
        public void DeleteCompanyEmailHtml_FileNotExist_Success()
        {
            User user = new User()
            {
                CompanyId = ID
            };

            string path = Path.Combine(_folderSettingsMock.Value.EmailTemplates, $"{ID}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(path, new MockFileData("file for delete"));
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.DeleteCompanyEmailHtml(user);
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(path));

        }

        [Fact]
        public void DeleteCompanyLogo_UserIsNull_ThrowException()
        {



            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _ConfigurationFileWrapper.DeleteCompanyLogo(null));

            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);
        }
        [Fact]
        public void DeleteCompanyLogo_FileNotExist_DoNothing()
        {

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.DeleteCompanyLogo(new User());

        }
        [Fact]
        public void DeleteCompanyLogo_FileNotExist_Success()
        {
            User user = new User()
            {
                CompanyId = ID
            };

            string path = Path.Combine(_folderSettingsMock.Value.CompaniesLogo, $"{ID}.png");
            ((MockFileSystem)_fileSystemMock).AddFile(path, new MockFileData("file for delete"));
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.DeleteCompanyLogo(user);
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(path));

        }

        [Fact]
        public void DeleteCompanyResorces_UserIsNull_ThrowException()
        {



            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _ConfigurationFileWrapper.DeleteCompanyResorces(null));

            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);
        }
        [Fact]
        public void DeleteCompanyResorces_FileNotExist_DoNothing()
        {

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.DeleteCompanyResorces(new Company());

        }
        [Fact]
        public void DeleteCompanyResorces_FileNotExist_Success()
        {
            Company company = new Company()
            {
                Id = ID
            };

            string companylogoPath = Path.Combine(_folderSettingsMock.Value.CompaniesLogo, $"{company.Id}.png");
            ((MockFileSystem)_fileSystemMock).AddFile(companylogoPath, new MockFileData("file for delete"));
            string beforeSigningPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "BeforeSigning", $"{company.Id}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(companylogoPath, new MockFileData("file for delete"));

            string afterSigningPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "AfterSigning", $"{company.Id}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(companylogoPath, new MockFileData("file for delete"));
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.DeleteCompanyResorces(company);
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(companylogoPath));
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(beforeSigningPath));
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(afterSigningPath));

        }


        [Fact]
        public void GetCompanyEmailTemplate_CompanyIsNull_ThrowException()
        {

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _ConfigurationFileWrapper.GetCompanyEmailTemplate(null, MessageType.AfterSigning));

            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);
        }
        [Fact]
        public void GetCompanyEmailTemplate_MessageTypeNotHhandled_emptyString()
        {

            Company company = new Company()
            {
                Id = ID
            };
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var result = _ConfigurationFileWrapper.GetCompanyEmailTemplate(company, MessageType.AllSignersSignedNotification);

            Assert.Empty(result);
        }
        [Fact]
        public void GetCompanyEmailTemplate_FileNotExist_emptyString()
        {

            Company company = new Company()
            {
                Id = ID
            };
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var result = _ConfigurationFileWrapper.GetCompanyEmailTemplate(company, MessageType.AfterSigning);

            Assert.Empty(result);
        }
        [Fact]
        public void GetCompanyEmailTemplate_FileExist_Success()
        {



            Company company = new Company()
            {
                Id = ID
            };
            string afterSigningPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "AfterSigning", $"{company.Id}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(afterSigningPath, new MockFileData("file"));
            string beforeSigningPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "BeforeSigning", $"{company.Id}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(beforeSigningPath, new MockFileData("file aaa"));
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var resultAfterSigning = _ConfigurationFileWrapper.GetCompanyEmailTemplate(company, MessageType.AfterSigning);
            var resultBeforeSigning = _ConfigurationFileWrapper.GetCompanyEmailTemplate(company, MessageType.BeforeSigning);

            Assert.Contains("data:text/html;base64,", resultAfterSigning);
            Assert.Contains("data:text/html;base64,", resultBeforeSigning);
            Assert.NotEqual(resultBeforeSigning, resultAfterSigning);
        }


        [Fact]
        public void GetCompanyLogo_FileNotExist_emptyString()
        {


            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var result = _ConfigurationFileWrapper.GetCompanyLogo(ID);

            Assert.Empty(result);
        }

        [Fact]
        public void GetCompanyLogo_FileExist_Success()
        {




            string logo = Path.Combine(_folderSettingsMock.Value.CompaniesLogo, $"{ID}.png");
            ((MockFileSystem)_fileSystemMock).AddFile(logo, new MockFileData("file"));

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var resultlogo = _ConfigurationFileWrapper.GetCompanyLogo(ID);


            Assert.Contains("data:image/png;base64", resultlogo);

        }

        [Fact]
        public void GetDefaultEmailTemplate_FileExist_Success()
        {
            var templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), DEFAULT_EMAIL_TEMPLATE);
            ((MockFileSystem)_fileSystemMock).AddFile(templatePath, new MockFileData("file "));
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var htmlResult = _ConfigurationFileWrapper.GetDefaultEmailTemplate();

            Assert.NotEmpty(htmlResult);
        }

        [Fact]
        public void GetDefaultEmailTemplate_FileNotExist_ThrowException()
        {


            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<Exception>(() => _ConfigurationFileWrapper.GetDefaultEmailTemplate());

            Assert.Contains("Email body template not exist in system", actual.Message);
        }



        [Fact]
        public void GetDefaultLogo_FileExist_Success()
        {
            var templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), DEFAULT_LOGO);
            ((MockFileSystem)_fileSystemMock).AddFile(templatePath, new MockFileData("file"));
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var htmlResult = _ConfigurationFileWrapper.GetDefaultLogo();

            Assert.NotEmpty(htmlResult);
        }

        [Fact]
        public void GetDefaultLogo_FileNotExist_ThrowException()
        {


            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<Exception>(() => _ConfigurationFileWrapper.GetDefaultLogo());

            Assert.Contains("Default logo not exist in system ", actual.Message);
        }


        [Fact]
        public void GetEmailTemplate_UserNull_ThrowException()
        {


            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _ConfigurationFileWrapper.GetEmailTemplate(null, MessageType.BeforeSigning));

            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);
        }
        [Fact]
        public void GetEmailTemplate_FileNotExist_returnDefulat()
        {
            var user = new User()
            {
                CompanyId = ID
            };
            var templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), DEFAULT_EMAIL_TEMPLATE);
            ((MockFileSystem)_fileSystemMock).AddFile(templatePath, new MockFileData("file "));

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var emailTemplate = _ConfigurationFileWrapper.GetEmailTemplate(user, MessageType.BeforeSigning);
            var emailTemplate1 = _ConfigurationFileWrapper.GetEmailTemplate(user, MessageType.Decline);

            Assert.NotEmpty(emailTemplate);
            Assert.NotEmpty(emailTemplate1);
            Assert.Equal(emailTemplate, emailTemplate1);
        }


        [Fact]
        public void GetEmailTemplate_FileExist_Success()
        {
            var user = new User()
            {
                CompanyId = ID
            };

            var user1 = new User()
            {
                CompanyId = Guid.NewGuid()
            };


            var templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), DEFAULT_EMAIL_TEMPLATE);
            ((MockFileSystem)_fileSystemMock).AddFile(templatePath, new MockFileData("file "));

            string beforeSigningPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "BeforeSigning", $"{user.CompanyId}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(beforeSigningPath, new MockFileData("file For Company"));

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var htmlResult = _ConfigurationFileWrapper.GetEmailTemplate(user, MessageType.BeforeSigning);
            var htmlResult1 = _ConfigurationFileWrapper.GetEmailTemplate(user1, MessageType.BeforeSigning);

            Assert.NotEmpty(htmlResult);
            Assert.NotEmpty(htmlResult1);
            Assert.NotEqual(htmlResult1, htmlResult);
        }





        [Fact]
        public void GetLogo_UserNull_ThrowException()
        {


            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<Exception>(() => _ConfigurationFileWrapper.GetLogo(null));

            Assert.Equal("Null input - user is null", actual.Message);
        }
        [Fact]
        public void GetLogo_FileNotExist_returnDefulat()
        {
            var user = new User()
            {
                CompanyId = ID
            };
            var templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), DEFAULT_LOGO);
            ((MockFileSystem)_fileSystemMock).AddFile(templatePath, new MockFileData("file "));

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var emailTemplate = _ConfigurationFileWrapper.GetLogo(user);

            Assert.NotEmpty(emailTemplate);

        }


        [Fact]
        public void GetLogo_FileExist_Success()
        {
            var user = new User()
            {
                CompanyId = ID
            };

            var user1 = new User()
            {
                CompanyId = Guid.NewGuid()
            };


            var templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), DEFAULT_LOGO);
            ((MockFileSystem)_fileSystemMock).AddFile(templatePath, new MockFileData("file "));

            string beforeSigningPath = Path.Combine(_folderSettingsMock.Value.CompaniesLogo, $"{user.CompanyId}.png");
            ((MockFileSystem)_fileSystemMock).AddFile(beforeSigningPath, new MockFileData("file For Company"));

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var htmlResult = _ConfigurationFileWrapper.GetLogo(user);
            var htmlResult1 = _ConfigurationFileWrapper.GetLogo(user1);

            Assert.NotEmpty(htmlResult);
            Assert.NotEmpty(htmlResult1);
            Assert.NotEqual(htmlResult1, htmlResult);
        }







        [Fact]
        public void ReadEmailsResource_FileNotExist_ThrowException()
        {


            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _ConfigurationFileWrapper.ReadEmailsResource(Enums.Users.Language.en));

            Assert.Contains("Email Json not exist in system ", actual.Message);
        }


        [Fact]
        public void ReadEmailsResource_FileExist_Success()
        {
            var enPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"Resources/Emails.{Enums.Users.Language.en}.json");
            ((MockFileSystem)_fileSystemMock).AddFile(enPath, new MockFileData("{" +
                "ActivationLinkText:\"ENG\"" +
                "} "));

            var hebPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"Resources/Emails.{Enums.Users.Language.he}.json");
            ((MockFileSystem)_fileSystemMock).AddFile(hebPath, new MockFileData("{" +
                "ActivationLinkText:\"HEB\"" +
                "} "));


            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var engResource = _ConfigurationFileWrapper.ReadEmailsResource(Enums.Users.Language.en);
            var hebResource = _ConfigurationFileWrapper.ReadEmailsResource(Enums.Users.Language.he);

            Assert.Equal("ENG", engResource.ActivationLinkText);
            Assert.Equal("HEB", hebResource.ActivationLinkText);
        }






        [Fact]
        public void SaveCompanyLogo_UserNull_ThrowException()
        {


            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _ConfigurationFileWrapper.SaveCompanyLogo(null, ""));

            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);
        }


        [Fact]
        public void SaveCompanyLogo_validInput_Success()
        {

            var user = new User()
            {
                CompanyId = ID
            };
            byte[] dataUriSchemeResult = Convert.FromBase64String("YQ==");
            _dataUriSchemeMock.Setup(x => x.GetBytes(It.IsAny<string>())).Returns(dataUriSchemeResult);
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.SaveCompanyLogo(user, "a");

            Assert.True(((MockFileSystem)_fileSystemMock).File.Exists(Path.Combine(_folderSettingsMock.Value.CompaniesLogo, $"{user.CompanyId}.png")));
        }



        [Fact]
        public void UpdateCompanyEmailHtml_UserNull_ThrowException()
        {

            var user = new User()
            {
                CompanyId = ID
            };

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _ConfigurationFileWrapper.UpdateCompanyEmailHtml(null, null));

            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);
        }

        [Fact]
        public void UpdateCompanyEmailHtml_EmptyEmailsHtmlBody_DeleteExistsFiles()
        {

            var user = new User()
            {
                CompanyId = ID
            };
            var beforeHTMLPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "BeforeSigning", $"{user.CompanyId}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(beforeHTMLPath, new MockFileData("data1"));
            var afterHTMLPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "AfterSigning", $"{user.CompanyId}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(afterHTMLPath, new MockFileData("data2"));

            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.UpdateCompanyEmailHtml(user, null);
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(beforeHTMLPath));
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(afterHTMLPath));

            beforeHTMLPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "BeforeSigning", $"{user.CompanyId}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(beforeHTMLPath, new MockFileData("data1"));
            afterHTMLPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "AfterSigning", $"{user.CompanyId}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(afterHTMLPath, new MockFileData("data2"));
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.UpdateCompanyEmailHtml(user, new EmailHtmlBodyTemplates { AfterSigningBase64String = "", BeforeSigningBase64String = "" });
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(beforeHTMLPath));
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(afterHTMLPath));
        }


        [Fact]
        public void UpdateCompanyEmailHtml_UpdateHtmlBody_Success()
        {

            var user = new User()
            {
                CompanyId = ID
            };
            var beforeHTMLPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "BeforeSigning", $"{user.CompanyId}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(beforeHTMLPath, new MockFileData("data1"));
            byte[] dataUriSchemeResult = Convert.FromBase64String("YQ==");
            _dataUriSchemeMock.Setup(x => x.GetBytes(It.IsAny<string>())).Returns(dataUriSchemeResult);
            var afterHTMLPath = Path.Combine(_folderSettingsMock.Value.EmailTemplates, "AfterSigning", $"{user.CompanyId}.html");
            Assert.False(((MockFileSystem)_fileSystemMock).File.Exists(afterHTMLPath));
            _ConfigurationFileWrapper = new LocalConfigurationFileWrapperHandler(_fileSystemMock, _folderSettingsMock, _dataUriSchemeMock.Object);

            _ConfigurationFileWrapper.UpdateCompanyEmailHtml(user, new EmailHtmlBodyTemplates { AfterSigningBase64String = "YQ==", BeforeSigningBase64String = "YQ==" } );
            Assert.True(((MockFileSystem)_fileSystemMock).File.Exists(beforeHTMLPath));
            Assert.True(((MockFileSystem)_fileSystemMock).File.Exists(afterHTMLPath));

        }


    }
}
