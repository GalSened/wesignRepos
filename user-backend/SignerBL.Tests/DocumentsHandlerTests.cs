using Common.Enums.PDF;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.PDF;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using Common.Models.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using SignerBL.HandlersdocumentCollection;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using Xunit;

using System.Reflection;

using Common.Models.Configurations;
using Common.Models.FileGateScanner;
using SignerBL.Handlers.Actions;
using Common.Interfaces.Oauth;
using Common.Interfaces.Files;
using Common.Handlers.Files;
using BL.Tests;
using BL.Tests.Services;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace SignerBL.Tests
{
    public class DocumentsHandlerTests : IDisposable
    {

        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";
        private const string GUID2 = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";
        private Guid GUID3 = Guid.NewGuid();

        private readonly IOptions<FolderSettings> _folderOptions;
        private readonly IOptions<GeneralSettings> _generalOptions;

        private readonly Mock<Common.Interfaces.SignerApp.ISignerValidator> _validatorMock;
        
        private readonly Mock<IJWT> _jwtMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IDataUriScheme> _dataUriSchemeMock;
        private readonly Mock<IDocumentPdf> _documentPdfMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<ITemplatePdf> _templatePdfMock;
        private readonly Mock<ICertificate> _certificateMock;
        private readonly Mock<IDocumentModeHandler> _documentModeHandlerMock;
        private readonly Mock<IAppendices> _appendicesMock;
        private readonly Mock<ISender> _senderMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IHttpClientFactory> _clientFactoryMock;

        private readonly Mock<IOTP> _optMock;
        private readonly Mock<IOauth> _oauthMock;
        private readonly Mock<IVisualIdentity> _visualIdentityMock;
        private readonly Mock<IDocumentCollectionOperationsNotifier> _documentCollectionOperationsNotifierMock;
        private readonly Mock<ISmartCardSigningProcess> _smartCardSigningProcess;
        
        private readonly IDocumentsHandler _documentsHandler;
        private readonly IFilesWrapper _fileWrapperMock;

        private readonly Mock<IDocumentFileWrapper> _documentFileWrapper;
        private readonly Mock<IEncryptor> _encryptor;
        private readonly Mock<IContactFileWrapper> _contactFileWrapper;
        private readonly Mock<IUserFileWrapper> _userFileWrapper;
        private readonly Mock<ISignerFileWrapper> _signerFileWrapper;
        private readonly Mock<IConfigurationFileWrapper> _configurationFileWrapper;

        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;
        private readonly Mock<ISignerTokenMappingConnector> _signerTokenMappingConnectorMock;
        private readonly Mock<IConfigurationConnector> _configurationConnectorMock;
        private readonly Mock<ISignersConnector> _signersConnectorMock;
        private readonly Mock<ITemplateConnector> _tempateConnectorMock;
        private readonly Mock<IGroupConnector> _groupConnectorMock;
        private readonly Mock<IDocumentConnector> _documentConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IOcrService> _ocrServiceMock;
        
        public void Dispose()
        {
            _validatorMock.Invocations.Clear();
            _serviceScopeFactoryMock.Invocations.Clear();
            _loggerMock.Invocations.Clear();
            _optMock.Invocations.Clear();
            _oauthMock.Invocations.Clear();
            _visualIdentityMock.Invocations.Clear();
            _documentCollectionOperationsNotifierMock.Invocations.Clear();
            _smartCardSigningProcess.Invocations.Clear();
            _encryptor.Invocations.Clear();
            _documentCollectionConnectorMock.Invocations.Clear();
                _signerTokenMappingConnectorMock.Invocations.Clear();
            _configurationConnectorMock.Invocations.Clear();
            _signersConnectorMock.Invocations.Clear();
            _tempateConnectorMock.Invocations.Clear();
            _groupConnectorMock.Invocations.Clear();
            _documentConnectorMock.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
            _ocrServiceMock.Invocations.Clear();
        }

        public DocumentsHandlerTests()
        {
            _validatorMock = new Mock<Common.Interfaces.SignerApp.ISignerValidator>();

            _jwtMock = new Mock<IJWT>();
            _fileSystemMock = new Mock<IFileSystem>();
            _dataUriSchemeMock = new Mock<IDataUriScheme>();
            _documentPdfMock = new Mock<IDocumentPdf>();
            _loggerMock = new Mock<ILogger>();
            _daterMock = new Mock<IDater>();
            _templatePdfMock = new Mock<ITemplatePdf>();
            _certificateMock = new Mock<ICertificate>();
            _documentModeHandlerMock = new Mock<IDocumentModeHandler>();
            _appendicesMock = new Mock<IAppendices>();
            _senderMock = new Mock<ISender>();
            _configurationMock = new Mock<IConfiguration>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _clientFactoryMock = new Mock<IHttpClientFactory>();
            _optMock = new Mock<IOTP>();
            _oauthMock = new Mock<IOauth>();
            _visualIdentityMock = new Mock<IVisualIdentity>();
            _documentCollectionOperationsNotifierMock = new Mock<IDocumentCollectionOperationsNotifier>();
            _smartCardSigningProcess = new Mock<ISmartCardSigningProcess>();

            _documentFileWrapper = new Mock<IDocumentFileWrapper>();
            _contactFileWrapper = new Mock<IContactFileWrapper>();
            _userFileWrapper = new Mock<IUserFileWrapper>();
            _signerFileWrapper = new Mock<ISignerFileWrapper>();
            _configurationFileWrapper = new Mock<IConfigurationFileWrapper>();
            _encryptor = new Mock<IEncryptor>();
            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();
            _signerTokenMappingConnectorMock = new Mock<ISignerTokenMappingConnector>();
            _configurationConnectorMock = new Mock<IConfigurationConnector>();
            _signersConnectorMock = new Mock<ISignersConnector>();
            _tempateConnectorMock = new Mock<ITemplateConnector>();
            _groupConnectorMock = new Mock<IGroupConnector>();
            _documentConnectorMock = new Mock<IDocumentConnector>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _fileWrapperMock = new FileWrapperStub(_documentFileWrapper.Object, _contactFileWrapper.Object, _userFileWrapper.Object, _signerFileWrapper.Object, _configurationFileWrapper.Object);
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _ocrServiceMock = new Mock<IOcrService>();

            _folderOptions = Options.Create(new FolderSettings()

            {
                ContactSignatureImages = @"c:\",
            });
            _generalOptions = Options.Create(new GeneralSettings()
            {
            });

            _documentsHandler = new DocumentsHandler(_documentCollectionConnectorMock.Object,_signerTokenMappingConnectorMock.Object,_companyConnectorMock.Object, _configurationConnectorMock.Object,
                _groupConnectorMock.Object,_documentConnectorMock.Object,_tempateConnectorMock.Object,_signersConnectorMock.Object, _generalOptions,_jwtMock.Object,
                _documentPdfMock.Object, _validatorMock.Object,  _certificateMock.Object, _documentModeHandlerMock.Object,
                _loggerMock.Object, _daterMock.Object, _templatePdfMock.Object,  _appendicesMock.Object,
                _senderMock.Object,  _memoryCacheMock.Object, _optMock.Object,    _oauthMock.Object,        
                _documentCollectionOperationsNotifierMock.Object,
                _fileWrapperMock, _encryptor.Object, _serviceScopeFactoryMock.Object, _ocrServiceMock.Object);
        }


        #region GetPagesInfoByDocumentId

        [Fact]
        public async Task GetPagesInfoByDocumentId_SignerIsNull_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = null;

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            string code = "";
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<Guid>())).Returns(Mock.Of<ICacheEntry>());

            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.GetPagesInfoByDocumentId(signerToken,
                GUID3, 2, 5, code));


            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetPagesInfoByDocumentId_DocumentCollectionIsNull_ShouldThrowException()
        {
            // Arrange
            DocumentCollection documentCollection = null;
            SignerTokenMapping signerToken = new SignerTokenMapping() { GuidToken = Guid.Parse(GUID)};
            Guid guid = Guid.Empty;
            Signer signer = new Signer();

            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            string code = "";
            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.
             GetPagesInfoByDocumentId(signerToken, Guid.NewGuid(), offset: 2, limit: 5, code));

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Theory]
        [InlineData(GUID)]
        public async Task GetPagesInfoByDocumentId_DocumentIsNull_ShouldThrowException(string documentId)
        {
            // Arrange
            _memoryCacheMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Guid guid = Guid.Parse(GUID);

            var signer = new Signer()
            {
                Id = Guid.Parse(GUID)
            };

            var documentsList = new List<Document>()
            {
                new Document()
                {
                    //Id = guid
                },
                new Document(),
                new Document(),
            };
            var signersList = new List<Signer>()
            {
             signer
            };

            DocumentCollection documentCollection = new DocumentCollection()
            {
                Documents = documentsList,
                Signers = signersList,
            };
            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));


            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            //documentCollection.Documents.FirstOrDefault(d=>d.Id == signer.Id);
            string code = "";
            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.
            GetPagesInfoByDocumentId(signerToken, Guid.Parse(documentId), 2,   5, code));

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentId.GetNumericString(), actual.Message);
        }

        [Theory]
        [InlineData(GUID, 2, 5)]
        [InlineData(GUID, 1, 3)]
        [InlineData(GUID, 1, 4)]
        public async Task GetPagesInfoByDocumentId_Success(string documentId, int offset, int limit)
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping();
            Guid guid = Guid.Parse(GUID);

            var signer = new Signer()
            {
                Id = Guid.Parse(GUID)
            };

            var documentsList = new List<Document>()
            {
                new Document()
                {
                    Id = guid,
                    Fields = new PDFFields()
                    {
                        CheckBoxFields =new List<CheckBoxField>()
                        {
                            new CheckBoxField(new BaseField() ),
                            new CheckBoxField(new BaseField() ),
                        },
                        TextFields = new List<TextField>()
                    }
                },
                new Document(),
                new Document(),
            };
            var signersList = new List<Signer>()
            {
             signer
            };

            DocumentCollection documentCollection = new DocumentCollection()
            {
                Documents = documentsList,
                Signers = signersList,
            };
            DocumentMemoryCache documentMemoryCache = null;


            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>());
            _documentPdfMock.Setup(x => x.Load(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(true);
            _documentPdfMock.Setup(x => x.GetAllFields(It.IsAny<int>(), It.IsAny<int>(),It.IsAny<bool>())).Returns(documentsList[0].Fields);
            _documentConnectorMock.Setup(x => x.ReadSignatures(It.IsAny<Document>())).Returns(new List<DocumentSignatureField>());

            // Act
            MethodInfo method = typeof(DocumentsHandler).GetMethod("ExtractSignerFields", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { offset, limit, signer, documentCollection.Documents.First(), documentMemoryCache };
            object result = method.Invoke(_documentsHandler, parameters);
            string code = "";
            (var document,var _, PDFFields fields) = await _documentsHandler.GetPagesInfoByDocumentId(signerToken, Guid.Parse(documentId), offset, limit, code);
            // Assert
            Assert.NotNull(document);
            Assert.IsType<Document>(document);
            Assert.Equal(Guid.Parse(GUID), document.Id);
        }

        [Theory]
        [InlineData(2, 5)]
        public async Task GetPagesInfoByDocumentId_PrivateMethod_ExtractSignerFields_ShouldReturnEditedDocument_Success(int offset, int limit)
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping();
            Guid guid = Guid.Parse(GUID);

            var signer = new Signer()
            {
                Id = Guid.Parse(GUID),
                SignerFields = new List<SignerField>()
                {
                    new SignerField()
                    {
                        FieldValue = "field1",
                        DocumentId = Guid.Parse(GUID),
                        FieldName = "Name1"
                    } ,
                    new SignerField()
                    {
                        FieldValue = "field2",
                        DocumentId = Guid.Parse(GUID2),
                        FieldName = "Asdcw2"
                    }
                },
            };

            var documentsList = new List<Document>()
            {
                new Document()
                {
                    Id = guid,
                    Fields = new PDFFields()
                    {
                        CheckBoxFields =new List<CheckBoxField>()
                        {
                            new CheckBoxField(new BaseField() ),
                            new CheckBoxField(new BaseField() ),
                        },
                        TextFields = new List<TextField>()
                    }
                },
                new Document(),
                new Document(),
            };
            var signersList = new List<Signer>()
            {
             signer
            };

            DocumentCollection documentCollection = new DocumentCollection()
            {
                Documents = documentsList,
                Signers = signersList,
            };
            

            PDFFields pdfFields = new PDFFields()
            {
                TextFields = new List<TextField>()
                {
                    new TextField()
                    {
                        Description = "Desc1",
                        Name="Name1",
                        TextFieldType = Common.Enums.PDF.TextFieldType.Custom
                    },
                    new TextField()
                    {
                        Description = "Desc2",
                        Name="Name2"
                    },
                }
            };

            Template template = new Template()
            {
                Id = Guid.Parse(GUID),
                Fields = new PDFFields()
                {
                    TextFields = new List<TextField>()
                    {
                        new TextField()
                        {
                            Name="name1",
                            TextFieldType=Common.Enums.PDF.TextFieldType.Text
                        },
                        new TextField()
                        {
                            Name="name2",
                            TextFieldType=Common.Enums.PDF.TextFieldType.Phone
                        }
                    },
                },

            };

            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>());
            _documentPdfMock.Setup(x => x.Load(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(true);
            _documentPdfMock.Setup(x => x.GetAllFields(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(pdfFields);
            _tempateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _documentConnectorMock.Setup(x => x.ReadSignatures(It.IsAny<Document>())).Returns(new List<DocumentSignatureField>());
            string code = "";
            // Act

            //MethodInfo method = typeof(DocumentsHandler).GetMethod("ExtractSignerFields", BindingFlags.NonPublic | BindingFlags.Instance);
            //object[] parameters = { offset, limit, signer, documentCollection.Documents.First(), documentMemoryCache };
            //object result = method.Invoke(_documentsHandler, parameters);

            (var document, var _, var _ ) = await _documentsHandler.GetPagesInfoByDocumentId(signerToken, Guid.Parse(GUID), offset, limit, code);


            // Assert
            Assert.NotNull(document);
            Assert.IsType<Document>(document);
            //Assert.Equal(Guid.Parse(GUID), actual.Id);
        }

        [Theory]
        [InlineData(2, 5)]
        public async Task GetPagesInfoByDocumentId_PrivateMethod_GetImages_WhenDocumentIdNotEmpty_ShouldReturnDocumentWithImages_Success(int offset, int limit)
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping();
            Guid guid = Guid.Parse(GUID);

            var signer = new Signer()
            {
                Id = Guid.Parse(GUID),
                SignerFields = new List<SignerField>()
                {
                    new SignerField()
                    {
                        FieldValue = "field1",
                        DocumentId = Guid.Parse(GUID),
                        FieldName = "Name1"
                    } ,
                    new SignerField()
                    {
                        FieldValue = "field2",
                        DocumentId = Guid.Parse(GUID2),
                        FieldName = "Asdcw2"
                    }
                },
            };

            var documentsList = new List<Document>()
            {
                new Document()
                {
                    Id = guid,
                    TemplateId=Guid.Parse(GUID),
                    Fields = new PDFFields()
                    {
                        CheckBoxFields =new List<CheckBoxField>()
                        {
                            new CheckBoxField(new BaseField() ),
                            new CheckBoxField(new BaseField() ),
                        },
                        TextFields = new List<TextField>()
                    }
                },
                new Document(),
                new Document(),
            };
            var signersList = new List<Signer>()
            {
             signer
            };

            DocumentCollection documentCollection = new DocumentCollection()
            {
                Documents = documentsList,
                Signers = signersList,
            };
            

            PDFFields pdfFields = new PDFFields()
            {
                TextFields = new List<TextField>()
                {
                    new TextField()
                    {
                        Description = "Desc1",
                        Name="Name1",
                        TextFieldType = Common.Enums.PDF.TextFieldType.Custom
                    },
                    new TextField()
                    {
                        Description = "Desc2",
                        Name="Name2"
                    },
                }
            };

            Template template = new Template()
            {
                Id = Guid.Parse(GUID),
                Fields = new PDFFields()
                {
                    TextFields = new List<TextField>()
                    {
                        new TextField()
                        {
                            Name="name1",
                            TextFieldType=Common.Enums.PDF.TextFieldType.Text
                        },
                        new TextField()
                        {
                            Name="name2",
                            TextFieldType=Common.Enums.PDF.TextFieldType.Phone
                        }
                    },
                },
            };

            IList<PdfImage> pdfImages = new List<PdfImage>()
            {
                new PdfImage(),
                new PdfImage(),
                new PdfImage(),
            };

            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>());
            _documentPdfMock.Setup(x => x.Load(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(true);
            _documentPdfMock.Setup(x => x.GetAllFields(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(pdfFields);
            _tempateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _templatePdfMock.Setup(x => x.GetPdfImages(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).Returns(pdfImages);
            _documentConnectorMock.Setup(x => x.ReadSignatures(It.IsAny<Document>())).Returns(new List<DocumentSignatureField>());
            string code = "";
            // Act

            (var document, var _ , var _) =await _documentsHandler.GetPagesInfoByDocumentId(signerToken, Guid.Parse(GUID), offset, limit, code);


            // Assert
            Assert.NotNull(document);
            Assert.IsType<Document>(document);
            Assert.Equal(pdfImages.Count, document.Images.Count);
        }

        [Theory]
        [InlineData(2, 5)]
        public async Task GetPagesInfoByDocumentId_PrivateMethod_GetImages_WhenDocumentIdEmpty_ShouldReturnDocumentWithImages_Success(int offset, int limit)
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping();
            Guid guid = Guid.Parse(GUID);

            var signer = new Signer()
            {
                Id = Guid.Parse(GUID),
                SignerFields = new List<SignerField>()
                {
                    new SignerField()
                    {
                        FieldValue = "field1",
                        DocumentId = Guid.Parse(GUID),
                        FieldName = "Name1"
                    } ,
                    new SignerField()
                    {
                        FieldValue = "field2",
                        DocumentId = Guid.Parse(GUID2),
                        FieldName = "Asdcw2"
                    }
                },
            };

            var documentsList = new List<Document>()
            {
                new Document()
                {
                    Id = guid,
                    TemplateId=Guid.Empty,
                    Fields = new PDFFields()
                    {
                        CheckBoxFields =new List<CheckBoxField>()
                        {
                            new CheckBoxField(new BaseField() ),
                            new CheckBoxField(new BaseField() ),
                        },
                        TextFields = new List<TextField>()
                    }
                },
                new Document(),
                new Document(),
            };
            var signersList = new List<Signer>()
            {
             signer
            };

            DocumentCollection documentCollection = new DocumentCollection()
            {
                Documents = documentsList,
                Signers = signersList,
            };
            

            PDFFields pdfFields = new PDFFields()
            {
                TextFields = new List<TextField>()
                {
                    new TextField()
                    {
                        Description = "Desc1",
                        Name="Name1",
                        TextFieldType = Common.Enums.PDF.TextFieldType.Custom
                    },
                    new TextField()
                    {
                        Description = "Desc2",
                        Name="Name2"
                    },
                }
            };

            Template template = new Template()
            {
                Id = Guid.Parse(GUID),
                Fields = new PDFFields()
                {
                    TextFields = new List<TextField>()
                    {
                        new TextField()
                        {
                            Name="name1",
                            TextFieldType=Common.Enums.PDF.TextFieldType.Text
                        },
                        new TextField()
                        {
                            Name="name2",
                            TextFieldType=Common.Enums.PDF.TextFieldType.Phone
                        }
                    },
                },
            };

            IList<PdfImage> pdfImages = new List<PdfImage>()
            {
                new PdfImage(),
                new PdfImage(),
                new PdfImage(),
            };

            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>());
            _documentPdfMock.Setup(x => x.Load(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(true);
            _documentPdfMock.Setup(x => x.GetAllFields(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(pdfFields);
            _tempateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _documentPdfMock.Setup(x => x.GetPdfImages(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).Returns(pdfImages);
            _documentConnectorMock.Setup(x => x.ReadSignatures(It.IsAny<Document>())).Returns(new List<DocumentSignatureField>());
            string code = "";
            // Act

            (var document, var _ , var _ ) =await _documentsHandler.GetPagesInfoByDocumentId(signerToken, Guid.Parse(GUID), offset, limit, code);


            // Assert
            Assert.NotNull(document);
            Assert.IsType<Document>(document );
            Assert.Equal(pdfImages.Count,  document.Images.Count);
        }

        #endregion

        #region GetDocumentCollectionData

        [Fact]
        public async Task GetDocumentCollectionData_NullSignerTokenMapping_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping() { GuidToken = Guid.Parse(GUID) };
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<Guid>())).Returns(Mock.Of<ICacheEntry>());

            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.
            GetDocumentCollectionData("",signerToken));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task GetDocumentCollectionData_NullSigner_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = null;
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<Guid>())).Returns(Mock.Of<ICacheEntry>());

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.
            GetDocumentCollectionData("", signerToken));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task GetDocumentCollectionData_InvalidDocumentStatus_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Id = Guid.NewGuid(),
                DocumentStatus = DocumentStatus.Declined,
            };

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<Guid>())).Returns(Mock.Of<ICacheEntry>());
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.
            GetDocumentCollectionData("", signerToken));

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetDocumentCollectionData_InvalidSignerStatus_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.Parse(GUID),
                Status = SignerStatus.Signed,
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Id = Guid.NewGuid(),
                DocumentStatus = DocumentStatus.Created,
                Signers = new List<Signer>()
                {
                    signer
                },
            };


            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<Guid>())).Returns(Mock.Of<ICacheEntry>());
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());
            
            // Act

                 
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.
            GetDocumentCollectionData("", signerToken));

            // Assert
            Assert.Equal(ResultCode.DocumentAlreadySignedBySigner.GetNumericString(), actual.Message);
        }

        #endregion

        #region GetDocumentCollectionHtmlData

        [Fact]
        public async Task GetDocumentCollectionHtmlData_InvalidToken_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping() { GuidToken = Guid.Parse(GUID) };
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<Guid>())).Returns(Mock.Of<ICacheEntry>());
            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.
            GetDocumentCollectionHtmlData(signerToken));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetDocumentCollectionHtmlData_InvalidSigner_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping() { GuidToken = Guid.Parse(GUID) };
            Signer signer = null;

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<Guid>())).Returns(Mock.Of<ICacheEntry>());

            // Act
            var actual =await  Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.
            GetDocumentCollectionHtmlData( signerToken));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetDocumentCollectionHtmlData_InvalidDocumentStatus_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping() { GuidToken = Guid.Parse(GUID) };
            Signer signer = new Signer();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Id = Guid.NewGuid(),
                DocumentStatus = DocumentStatus.Declined,
            };

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<Guid>())).Returns(Mock.Of<ICacheEntry>());

            // Act
            var actual =await  Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.
            GetDocumentCollectionHtmlData( signerToken));

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetDocumentCollectionHtmlData_ReturnAllContent_Success()
        {
            // Arrange
            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Id = Guid.NewGuid(),
                DocumentStatus = DocumentStatus.Created,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                    },
                     new Document()
                    {
                    },
                      new Document()
                    {
                    },
                },
            };
            Template template = new Template()
            {
                Fields = new PDFFields()
                {
                    TextFields = new List<TextField>()
                    {
                        new TextField()
                        {
                            Name = "1",
                            TextFieldType = TextFieldType.Number
                        },
                         new TextField()
                        {
                            Name = "2",
                            TextFieldType = TextFieldType.Email
                        },
                          new TextField()
                        {
                            Name = "3",
                            TextFieldType = TextFieldType.Text
                        }
                    } // count: 3
                }
            };
            (string HTML, string JS) htmlContent;
            htmlContent.HTML = "html";
            htmlContent.JS = "js";
            PDFFields pdfFields = new PDFFields()
            {
                TextFields = new List<TextField>()
                {
                    new TextField()
                    {
                        Name = "1"
                    },
                    new TextField()
                    {
                        Name = "2"
                    },
                    new TextField()
                    {
                        Name = "3"
                    },
                } // count: 3
            };

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);


            
            _tempateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _templatePdfMock.Setup(x => x.GetHtmlTemplate()).Returns(htmlContent);
            _templatePdfMock.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(pdfFields);

            // Act
            var actual =await _documentsHandler.GetDocumentCollectionHtmlData(signerToken );

            // Assert
            Assert.NotNull(actual.HTML);
            Assert.NotEmpty(actual.HTML);
            Assert.Equal("html", actual.HTML);
            Assert.Equal(actual.FieldsData.Count, pdfFields.TextFields.Count);
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_InputValidation_NullDbDocumentCollection_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.NewGuid(),
            };
            Guid guid = Guid.NewGuid();
            DocumentCollection inputDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentCollection dbDocumentCollection = null;
            DocumentOperation documentOperation = DocumentOperation.Close;


            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(dbDocumentCollection);


            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _documentsHandler.Update(signerTokenMapping, inputDocumentCollection, documentOperation));


            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_InputValidation_DocumentNotBelongToCollection_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.NewGuid(),
                IPAddress = "ipAddress"
            };
            Guid guid = Guid.NewGuid();
            DocumentCollection inputDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentCollection dbDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentOperation documentOperation = DocumentOperation.Close;


            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(dbDocumentCollection);
            _validatorMock.Setup(x => x.AreDocumentsBelongToDocumentCollection(It.IsAny<DocumentCollection>(), It.IsAny<DocumentCollection>())).Returns(false);

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _documentsHandler.Update(signerTokenMapping, inputDocumentCollection, documentOperation));


            // Assert
            Assert.Equal(ResultCode.DocumentNotBelongToDocumentCollection.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_InputValidation_NotAllFieldsExistsInDocuments_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.NewGuid(),
                IPAddress = "ipAddress"
            };
            Guid guid = Guid.NewGuid();
            DocumentCollection inputDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentCollection dbDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentOperation documentOperation = DocumentOperation.Close;


            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(dbDocumentCollection);
            _validatorMock.Setup(x => x.AreDocumentsBelongToDocumentCollection(It.IsAny<DocumentCollection>(), It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllFieldsExistsInDocuments(It.IsAny<DocumentCollection>())).Returns(false);

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _documentsHandler.Update(signerTokenMapping, inputDocumentCollection, documentOperation));


            // Assert
            Assert.Equal(ResultCode.NotAllFieldsExistsInDocuments.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_InputValidation_NotAllFieldsBelongToSigner_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.NewGuid(),
                IPAddress = "ipAddress"
            };
            Guid guid = Guid.NewGuid();
            DocumentCollection inputDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentCollection dbDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentOperation documentOperation = DocumentOperation.Close;


            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(dbDocumentCollection);
            _validatorMock.Setup(x => x.AreDocumentsBelongToDocumentCollection(It.IsAny<DocumentCollection>(), It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllFieldsExistsInDocuments(It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllFieldsBelongToSigner(It.IsAny<Signer>(), It.IsAny<Signer>(), It.IsAny<DocumentCollection>())).Returns(false);

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _documentsHandler.Update(signerTokenMapping, inputDocumentCollection, documentOperation));


            // Assert
            Assert.Equal(ResultCode.NotAllFieldsBelongToSigner.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_InputValidation_AreAllMandatoryFieldsFilledIn_ShouldThrowException()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.NewGuid(),
                IPAddress = "ipAddress"
            };
            Guid guid = Guid.NewGuid();
            DocumentCollection inputDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentCollection dbDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentOperation documentOperation = DocumentOperation.Close;


            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(dbDocumentCollection);
            _validatorMock.Setup(x => x.AreDocumentsBelongToDocumentCollection(It.IsAny<DocumentCollection>(), It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllFieldsExistsInDocuments(It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllFieldsBelongToSigner(It.IsAny<Signer>(), It.IsAny<Signer>(), It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllMandatoryFieldsFilledIn(It.IsAny<Signer>(), It.IsAny<Signer>())).Returns(false);

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _documentsHandler.Update(signerTokenMapping, inputDocumentCollection, documentOperation));


            // Assert
            Assert.Equal(ResultCode.NotAllMandatoryFieldsFilledIn.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_ValidateIsCleanFile_DocumentOperationIsDeclined_ShouldReturnEmptyDownloadLink()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.NewGuid(),
                IPAddress = "ipAddress",
                SignerAttachments = new List<SignerAttachment>()
                {
                    new SignerAttachment()
                    {
                        Base64File ="base64file1"
                    },
                     new SignerAttachment()
                    {
                        Base64File ="base64file2"
                    },
                      new SignerAttachment()
                    {
                        Base64File ="base64file3"
                    },
                }
            };

            Guid guid = Guid.NewGuid();
            DocumentCollection inputDocumentCollection = new DocumentCollection()
            {
                Id = guid,
                Signers = new List<Signer>() { signer },
                RedirectUrl = "[docId]"
            };
            DocumentCollection dbDocumentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>() { signer },
            };
            DocumentOperation documentOperation = DocumentOperation.Decline;
            FileGateScanResult fileGateScan = new FileGateScanResult()
            {
                IsValid = true
            };


            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(inputDocumentCollection);
            _validatorMock.Setup(x => x.AreDocumentsBelongToDocumentCollection(It.IsAny<DocumentCollection>(), It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllFieldsExistsInDocuments(It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllFieldsBelongToSigner(It.IsAny<Signer>(), It.IsAny<Signer>(), It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllMandatoryFieldsFilledIn(It.IsAny<Signer>(), It.IsAny<Signer>())).Returns(true);
            _validatorMock.Setup(x => x.ValidateIsCleanFile(It.IsAny<string>())).ReturnsAsync(fileGateScan);

            // if (operation == DocumentOperation.Decline) -> Mockings related to this condition
            _documentCollectionConnectorMock.Setup(x => x.Update(It.IsAny<DocumentCollection>())).Callback(() => { }); // ?
            _signerTokenMappingConnectorMock.Setup(x => x.Delete(It.IsAny<SignerTokenMapping>())).Callback(() => { });
            _configurationConnectorMock.Setup(x => x.Read()).ReturnsAsync(new Configuration());
            _senderMock.Setup(x =>
            x.SendDocumentDecline(It.IsAny<DocumentCollection>(), It.IsAny<Configuration>(), It.IsAny<Signer>(), It.IsAny<User>()))
                .Callback(() => { });
            _loggerMock.Setup(x => x.Warning(It.IsAny<string>())).Callback(() => { });


            // Act
            var actual = await _documentsHandler.Update(signerTokenMapping, inputDocumentCollection, documentOperation);

            // Assert
            Assert.Equal("", actual.Downloadlink);
            Assert.Equal(guid.ToString(), actual.RedirectLink);
        }

        [Fact]
        public async Task Update_ValidateIsCleanFile_DocumentOperationIsDeclined_ShouldReturnRedirectLinkAndDownloadLink()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.NewGuid(),
                IPAddress = "ipAddress",
                SignerAttachments = new List<SignerAttachment>()
                {
                    new SignerAttachment()
                    {
                        Base64File ="base64file1"
                    },
                     new SignerAttachment()
                    {
                        Base64File ="base64file2"
                    },
                      new SignerAttachment()
                    {
                        Base64File ="base64file3"
                    },
                }
            };
            signerTokenMapping.SignerId = signer.Id;

            Guid guid = Guid.NewGuid();
            DocumentCollection inputDocumentCollection = new DocumentCollection()
            {
                Id = guid,
                Signers = new List<Signer>() { signer },
                RedirectUrl = "[docId]",
                User = new User()
                {
                    UserConfiguration = new UserConfiguration()
                    {
                        ShouldNotifyWhileSignerViewed = false,
                        ShouldNotifyWhileSignerSigned = false,
                        ShouldSendSignedDocument = false,
                    }
                },
                Mode = DocumentMode.SelfSign,
            };
            //DocumentCollection dbDocumentCollection = new DocumentCollection()
            //{
            //    Signers = new List<Signer>() { signer },
            //};
            DocumentOperation documentOperation = DocumentOperation.Close; // Look it the current operation mode breaks a condition
            FileGateScanResult fileGateScan = new FileGateScanResult()
            {
                IsValid = true
            };
            PDFFields pdfFields = new PDFFields();

            IDoneActionsHelper actionsHelper = null;
            GroupSignHandler orderedGroupSignHandlerFactory = new GroupSignHandler(_companyConnectorMock.Object, _configurationMock.Object,
                _validatorMock.Object, _senderMock.Object, _loggerMock.Object, actionsHelper);

            _validatorMock.Setup(x => x.ValidateSignerToken(It.IsAny<SignerTokenMapping>())).ReturnsAsync((signer, guid));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(inputDocumentCollection);
            _validatorMock.Setup(x => x.AreDocumentsBelongToDocumentCollection(It.IsAny<DocumentCollection>(), It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllFieldsExistsInDocuments(It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllFieldsBelongToSigner(It.IsAny<Signer>(), It.IsAny<Signer>(), It.IsAny<DocumentCollection>())).Returns(true);
            _validatorMock.Setup(x => x.AreAllMandatoryFieldsFilledIn(It.IsAny<Signer>(), It.IsAny<Signer>())).Returns(true);
            _validatorMock.Setup(x => x.ValidateIsCleanFile(It.IsAny<string>())).ReturnsAsync(fileGateScan);

            // if (operation == DocumentOperation.Decline) -> Mockings related to this condition
            _documentPdfMock.Setup(x => x.Load(It.IsAny<Guid>(), It.IsAny<bool>())).Callback(() => { });
            _documentPdfMock.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(pdfFields);

            // UpdateAllFieldsExceptSignatures (under UpdatePdfDocuments)
            _documentPdfMock.Setup(x => x.SaveCopy(It.IsAny<Guid>())).Callback(() => { });
            _tempateConnectorMock.Setup(x => x.GetTextFieldsByType(It.IsAny<Template>(), It.IsAny<TextFieldType>())).Returns(new List<TextField>());

            _documentPdfMock.Setup(x => x.TextFields.UpdateValue(It.IsAny<List<TextField>>())).Callback(() => { });
            _documentPdfMock.Setup(x => x.CheckBoxFields.UpdateValue(It.IsAny<List<CheckBoxField>>())).Callback(() => { });
            _documentPdfMock.Setup(x => x.ChoiceFields.UpdateValue(It.IsAny<List<ChoiceField>>())).Callback(() => { });
            _documentPdfMock.Setup(x => x.RadioGroupFields.UpdateValue(It.IsAny<List<RadioGroupField>>())).Callback(() => { });

            _documentPdfMock.Setup(x => x.SaveDocument()).Callback(() => { });
            _documentPdfMock.Setup(x => x.EmbadTextDataFields(It.IsAny<List<TextField>>(), It.IsAny<List<ChoiceField>>())).Callback(() => { });

            // AddAttachmentsToFS 

            // UpdateSingerInfoInDb
            _documentConnectorMock.Setup(x => x.Read(It.IsAny<Document>())).ReturnsAsync(new Document());
            _documentConnectorMock.Setup(x => x.ReadSignatures(It.IsAny<Document>())).Returns(new List<DocumentSignatureField>());
            _documentCollectionConnectorMock.Setup(x => x.Update(It.IsAny<DocumentCollection>())).Callback(() => { });

            // DoneProcess
            _validatorMock.Setup(x => x.AreAllSignersSigned(It.IsAny<IEnumerable<Signer>>())).Callback(() => { });
            _daterMock.Setup(x => x.UtcNow()).Callback(() => { });
            _loggerMock.Setup(x => x.Debug(It.IsAny<string>())).Callback(() => { });

            string downloadLink = string.Empty;
            
            _documentModeHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<DocumentMode>())).Returns(orderedGroupSignHandlerFactory).Callback(async () =>
            {
                downloadLink = await orderedGroupSignHandlerFactory.DoAction(inputDocumentCollection, signer);
            });

            // DoAction
            _configurationConnectorMock.Setup(x => x.Read()).Callback(() => { });
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).Callback(() => { });
            _configurationMock.Setup(x => x.ShouldNotifyWhileSignerSigned(It.IsAny<User>(), It.IsAny<CompanyConfiguration>(),
                It.IsAny<DocumentNotifications>())).Returns(false);
            _validatorMock.Setup(x => x.AreAllSignersSigned(It.IsAny<IEnumerable<Signer>>())).Returns(false);


            // Act
            var actual =await _documentsHandler.Update(signerTokenMapping, inputDocumentCollection, documentOperation);

            // Assert
            Assert.Equal("", actual.Downloadlink);
            Assert.Equal(guid.ToString(), actual.RedirectLink);
        }
        #endregion

        #region Download


        [Fact]
        public async Task Download_SignerIsNull_ShouldThrowException()
        {
            // Arrange
            //_dbConnectorMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = null;
       

            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.Download(signerTokenMapping));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }


        [Fact]
        public async Task Download_DocumentCollectionIsNull_ShouldThrowException()
        {
            // Arrange
            //_dbConnectorMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();
     
            DocumentCollection documentCollection = null;

            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.Download(signerTokenMapping));

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Download_CannotDownloadUnsignedDocument_ShouldThrowException()
        {
            // Arrange
            //_dbConnectorMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();
   
            DocumentCollection documentCollection = new DocumentCollection()
            {
                DocumentStatus = DocumentStatus.Created,
            };

            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.Download(signerTokenMapping));

            // Assert
            Assert.Equal(ResultCode.CannotDownloadUnsignedDocument.GetNumericString(), actual.Message);
        }


        [Fact]
        public async Task Download_FileNotExist_ShouldThrowException()
        {
            // Arrange
            //_dbConnectorMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();
          
            DocumentCollection documentCollection = new DocumentCollection()
            {
                DocumentStatus = DocumentStatus.ExtraServerSigned,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                       Name = "doc1"
                    },
                     new Document()
                    {
                       Name = "doc2"
                    },
                }
            };

            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            _fileSystemMock.Setup(x => x.File.Exists(It.IsAny<string>())).Returns(false);
            _fileSystemMock.Setup(x => x.Path.Combine(It.IsAny<string[]>())).Returns(It.IsAny<string>());

            // Act
            var actual =await Assert.ThrowsAsync<Exception>(() => _documentsHandler.Download(signerTokenMapping));

            // Assert
            Assert.IsType<Exception>(actual);
        }

        [Fact]
        public async Task Download_ReturnDocuments_Success()
        {
            // Arrange
            //_dbConnectorMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();
            string documentCollectionName;
            DocumentCollection documentCollection = new DocumentCollection()
            {
                DocumentStatus = DocumentStatus.ExtraServerSigned,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                       Id = Guid.NewGuid(),
                       Name = "doc1"
                    },
                     new Document()
                    {
                       Id = Guid.NewGuid(),
                       Name = "doc2"
                    },
                }
            };
            string str = "dodo";
            byte[] result = new byte[str.Length];
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);


            _fileSystemMock.Setup(x => x.File.Exists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(x => x.Path.Combine(It.IsAny<string[]>())).Returns(It.IsAny<string>());
            
            _documentFileWrapper.Setup(x => x.IsDocumentExist(DocumentType.Document, It.IsAny<Guid>())).Returns(true);
            _documentFileWrapper.Setup(x => x.ReadDocument(DocumentType.Document, It.IsAny<Guid>())).Returns(result);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());
            // Act
            (var actual, documentCollectionName) = await _documentsHandler.Download(signerTokenMapping );

            // Assert
            Assert.Equal(2, actual.Count);
            var a = documentCollection.Documents.First().Id;
            var b = actual.Values.First();
           
        }

        #endregion

        #region Read Appendix

        [Fact]
        public async Task ReadAppendix_SignerIsNull_ShouldThrowException()
        {
            // Arrange
            //_dbConnectorMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = null;

            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);

            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.ReadAppendix(signerTokenMapping, string.Empty));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }


        [Fact]
        public async Task ReadAppendix_DocumentCollectionIsNull_ShouldThrowException()
        {
            // Arrange
            //_dbConnectorMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();
            DocumentCollection documentCollection = null;

            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentsHandler.ReadAppendix(signerTokenMapping, string.Empty));

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadAppendix_CantFindAppendixByName_ShouldThrowException()
        {
            // Arrange
            //_dbConnectorMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();
            Appendix appendix1 = new Appendix()
            {
                Name = "appendix1",
                FileExtention = ".png"
            };
            Appendix appendix2 = new Appendix()
            {
                Name = "appendix2",
                FileExtention = ".png"
            };
            IEnumerable<Appendix> appendices = new List<Appendix>()
            {
                appendix1,
                appendix2,
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>()
                {
                    signer
                },
                Id = Guid.Parse(GUID),
            };

            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _appendicesMock.Setup(x => x.Read(It.IsAny<Guid>())).Returns(appendices);
            _documentCollectionConnectorMock.Setup(x => x.Exists(It.IsAny<DocumentCollection>())).ReturnsAsync(true);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            // Act
            var actual =await _documentsHandler.ReadAppendix(signerTokenMapping, "appendix3");

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task ReadAppendix_ReturnAppendix_Success()
        {
            // Arrange
            //_dbConnectorMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();
            Appendix appendix1 = new Appendix()
            {
                Name = "appendix1",
                FileExtention = ".png"
            };
            Appendix appendix2 = new Appendix()
            {
                Name = "appendix2",
                FileExtention = ".png"
            };
            IEnumerable<Appendix> appendices = new List<Appendix>()
            {
                appendix1,
                appendix2,
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>()
                {
                    signer
                },
                Id = Guid.Parse(GUID),
            };

            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _appendicesMock.Setup(x => x.Read(It.IsAny<Guid>())).Returns(appendices);
            _documentCollectionConnectorMock.Setup(x => x.Exists(It.IsAny<DocumentCollection>())).ReturnsAsync(true);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            // Act
            var actual =await _documentsHandler.ReadAppendix(signerTokenMapping, "appendix2");

            // Assert
            Assert.Equal("appendix2", actual.Name);
        }


        #endregion

        #region DownloadSmartCardDesktopClientInstaller

    

        [Fact]
        public void DownloadSmartCardDesktopClientInstaller_ReturnsByteArray_Success()
        {
            byte[] bytes = new byte[4];

            _signerFileWrapper.Setup(x => x.GetSmartCardDesktopClientInstaller()).Returns(bytes);

            var actual = _documentsHandler.DownloadSmartCardDesktopClientInstaller();

            Assert.IsType<byte[]>(actual);
            Assert.Equal(4, actual.Length);
        }

        #endregion
    }
}
