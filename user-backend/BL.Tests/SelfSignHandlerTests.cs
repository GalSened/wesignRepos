using Common.Interfaces.DB;
using Common.Interfaces.PDF;
using Common.Interfaces;
using Common.Models.Settings;
using PdfHandler.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using Serilog;
using Moq;
using BL.Handlers;
using Microsoft.Extensions.Options;
using Xunit;
using Common.Models;
using Common.Models.Configurations;
using Common.Enums.Results;
using Common.Extensions;
using Common.Models.FileGateScanner;
using Common.Enums;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Interfaces.Oauth;
using System.Threading.Tasks;

namespace BL.Tests
{
    public class SelfSignHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";
        private const string GUID2 = "D14E4B8B-5970-4B50-A1F1-090B8F99D3B2";

        private readonly Mock<IValidator> _validatorMock;
        
        private readonly Mock<IDocumentPdf> _documentPdfMock;
        private readonly Mock<ICertificate> _certificateMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IUsers> _usersMock;
        private readonly Mock<IDataUriScheme> _dataUriSchemeMock;
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<ITemplatePdf> _templatePdfMock;
        private readonly Mock<IJWT> _jwtMock;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IOptions<GeneralSettings> _generalSetting;
        private readonly Mock<IDocumentCollections> _documentCollectionsMock;
        private readonly Mock<ITemplates> _templatesMock;
        private readonly Mock<IPdfConverter> _pdfConverterMock;
        private readonly Mock<IEncryptor> _encryptorMock;
        private readonly Mock<ISignConnector> _signConnectorMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ISmartCardSigningProcess> _smartCardSigningProcess;
        private readonly ISelfSign _selpSignHandler;


        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnectorMock;
        private readonly Mock<ISignerTokenMappingConnector> _signerTokenMappingConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IContactConnector> _contactConnectorMock;
        private readonly Mock<ITemplateConnector> _templateConnectorMock;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<IOauth> _oauthMock;
        
        public void Dispose()
        {
            
            _validatorMock.Invocations.Clear();
            
            _documentPdfMock.Invocations.Clear();
            _certificateMock.Invocations.Clear();
            _fileSystemMock.Invocations.Clear();
            _usersMock.Invocations.Clear();
            _dataUriSchemeMock.Invocations.Clear();
            _daterMock.Invocations.Clear();
            _templatePdfMock.Invocations.Clear();
            _jwtMock.Invocations.Clear();
            _documentCollectionsMock.Invocations.Clear();
            _templatesMock.Invocations.Clear();
            _pdfConverterMock.Invocations.Clear();
            _encryptorMock.Invocations.Clear();
            _signConnectorMock.Invocations.Clear();
            _loggerMock.Invocations.Clear();
            _smartCardSigningProcess.Invocations.Clear();
            _documentCollectionConnectorMock.Invocations.Clear();
            _programUtilizationConnectorMock.Invocations.Clear();
            _signerTokenMappingConnectorMock.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
            _contactConnectorMock.Invocations.Clear();
            _templateConnectorMock.Invocations.Clear();
            _programConnectorMock.Invocations.Clear();
            _oauthMock.Invocations.Clear();
        }

        public SelfSignHandlerTests()
        {
            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();
            _programUtilizationConnectorMock = new Mock<IProgramUtilizationConnector>();
            _signerTokenMappingConnectorMock = new Mock<ISignerTokenMappingConnector>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _contactConnectorMock = new Mock<IContactConnector>();
            _templateConnectorMock = new Mock<ITemplateConnector>();
            _programConnectorMock = new Mock<IProgramConnector>();
            _validatorMock = new Mock<IValidator>();
            
            _documentPdfMock = new Mock<IDocumentPdf>();
            _certificateMock = new Mock<ICertificate>();
            _fileSystemMock = new Mock<IFileSystem>();
            _usersMock = new Mock<IUsers>();
            _dataUriSchemeMock = new Mock<IDataUriScheme>();
            _daterMock = new Mock<IDater>();
            _templatePdfMock = new Mock<ITemplatePdf>();
            _jwtMock = new Mock<IJWT>();
            _documentCollectionsMock = new Mock<IDocumentCollections>();
            _templatesMock = new Mock<ITemplates>();
            _pdfConverterMock = new Mock<IPdfConverter>();
            _encryptorMock = new Mock<IEncryptor>();
            _signConnectorMock = new Mock<ISignConnector>();
            _loggerMock = new Mock<ILogger>();
            _oauthMock = new Mock<IOauth>();
            _jwtSettings = Options.Create(new JwtSettings());
            _generalSetting = Options.Create(new GeneralSettings());
            _smartCardSigningProcess = new Mock<ISmartCardSigningProcess>();
            _selpSignHandler = new SelfSignHandler(_validatorMock.Object,_documentPdfMock.Object, _documentCollectionConnectorMock.Object,_companyConnectorMock.Object,
                _templateConnectorMock.Object, _programConnectorMock.Object, _contactConnectorMock.Object, _programUtilizationConnectorMock.Object, _signerTokenMappingConnectorMock.Object,
                _certificateMock.Object,_jwtSettings,_usersMock.Object,_dataUriSchemeMock.Object,
                _daterMock.Object,_templatePdfMock.Object,_jwtMock.Object,_documentCollectionsMock.Object,_encryptorMock.Object,
                _loggerMock.Object,_templatesMock.Object,_pdfConverterMock.Object,_signConnectorMock.Object,
                _generalSetting,  _oauthMock.Object,
                _smartCardSigningProcess.Object);
        }

        #region Create

        [Fact]
        public async Task Create_UserProgramExpired_ShouldThrowExceptions()
        {
            User user = new User();
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _selpSignHandler.Create(new Template(),""));

            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task Create_InvalidFileType_ShouldThrowExceptions()
        {
            User user = new User();
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            Common.Enums.FileType fileType;

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);

            _dataUriSchemeMock.Setup(x => x.IsOctetStreamIsValidWord(It.IsAny<string>(), out fileType)).Returns(false);
            _dataUriSchemeMock.Setup(x => x.IsValidFileType(It.IsAny<string>(), out fileType));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _selpSignHandler.Create(new Template(), ""));

            Assert.Equal(ResultCode.InvalidFileType.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task Create_InvalidTemplateId_ShouldThrowExceptions()
        {
            User user = new User()
            {
                GroupId = Guid.NewGuid()
            };
            Common.Enums.FileType fileType;
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            FileGateScanResult fileGateScanResult = new FileGateScanResult();
            Template template = new Template();

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);

            _dataUriSchemeMock.Setup(x => x.IsOctetStreamIsValidWord(It.IsAny<string>(), out fileType)).Returns(true);
            _dataUriSchemeMock.Setup(x => x.IsValidFileType(It.IsAny<string>(), out fileType));

            _validatorMock.Setup(x => x.ValidateIsCleanFile(It.IsAny<string>())).ReturnsAsync(fileGateScanResult);
            _certificateMock.Setup(x => x.Create(It.IsAny<User>(), It.IsAny<CompanyConfiguration>()));
            _templateConnectorMock.Setup(x=>x.Read(It.IsAny<Template>())).ReturnsAsync(template);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _selpSignHandler.Create(new Template()
            {
                Id = Guid.NewGuid()
            }, ""));

            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);

        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_InvalidDocumentCollectionId_ShouldThrowException()
        {
            DocumentCollection documentCollection = null;

            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _selpSignHandler.Update(new DocumentCollection(), Common.Enums.Documents.DocumentOperation.Close));

            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_DocumentNotBelongToDocumentCollection_ShouldThrowException()
        {
            DocumentCollection documentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID2),
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        Id = Guid.Parse(GUID2),
                    }
                }
            };
            DocumentCollection dbDocumentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID),
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        Id = Guid.Parse(GUID),
                    }
                }
            };
            User user = new User()
            {
                Id = Guid.Parse(GUID)
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();

            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(dbDocumentCollection);
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _selpSignHandler.Update(documentCollection, Common.Enums.Documents.DocumentOperation.Close));

            Assert.Equal(ResultCode.DocumentNotBelongToDocumentCollection.GetNumericString(), actual.Message);
        }
        #endregion

        #region Delete
        
        [Fact]
        public async Task Delete_InvalidOperationException_ShouldThrowException()
        {
            User user = new User()
            {
                Id= Guid.Parse(GUID),
                GroupId = Guid.Parse(GUID2)
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            
            DocumentCollection documentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID2),
            };
            DocumentCollection dbDocumentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(dbDocumentCollection);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _selpSignHandler.Delete(documentCollection));

            Assert.Equal(ResultCode.DocumentNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        #endregion

        #region VerifySigner1Credential

        [Fact]
        public void VerifySigner1Credential_()
        {
            _documentPdfMock.Setup(x => x.VerifySigner1Credential(It.IsAny<SignerAuthentication>(), It.IsAny<CompanySigner1Details>()));
        }

        #endregion
    }
}
