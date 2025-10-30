using Common.Interfaces.DB;
using Common.Interfaces.MessageSending;
using Common.Interfaces.PDF;
using Common.Interfaces;
using Common.Models.Settings;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using Microsoft.Extensions.Options;
using Serilog;
using Common.Models.Files.PDF;
using Common.Handlers;
using BL.Handlers.FilesHandler;
using Moq;
using BL.Handlers;
using Common.Models;
using Common.Models.Configurations;
using Xunit;
using Common.Models.Documents.Signers;
using Common.Enums.Results;
using Common.Extensions;
using Common.Models.Documents;
using System.Linq;
using Org.BouncyCastle.Crypto.Agreement;
using Common.Enums.Documents;
using System.Runtime.InteropServices;
using Common.Interfaces.Files;
using Common.Handlers.Files;
using Common.Enums;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace BL.Tests
{
    public class DocumentCollectionsHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";
        private const string GUID2 = "D14E4B8B-5970-4B50-A1F1-090B8F99D3B2";

        private readonly IOptions<FolderSettings> _folderSettings;
        
        private readonly Mock<IConfiguration> _configurationMock;        
        private readonly Mock<IDocumentPdf> _documentPdfMock;
        private readonly Mock<ITemplatePdf> _templatePdfMock;
        private readonly Mock<IValidator> _validatorMock;
        private readonly Mock<ISignersConnector> _signersConnectorMock;
        private readonly Mock<IGenerateLinkHandler> _generateLinkHandlerMock;
        private readonly Mock<ISendingMessageHandler> _sendingMessageHandlerMock;
        private readonly Mock<IUsers> _usersMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<IAppendices> _appendicesMock;
        private readonly Mock<IDoneActionsHelper> _doneActionsHelperMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IXmlHandler<PDFFields>> _xmlHandlerMock;
        private readonly Mock<IDocumentCollectionOperations> _documentsOperationsMock;
        private readonly Mock<IDocumentCollectionOperationsNotifier> _documentsOperationNotifierMock;
        private readonly IFilesWrapper _filesWrapper;
        private readonly IDocumentCollections _documentCollectionsHandler;



        private readonly Mock<IDocumentFileWrapper> _documentFileWrapper;
        private readonly Mock<IContactFileWrapper> _contactFileWrapper;
        private readonly Mock<IUserFileWrapper> _userFileWrapper;
        private readonly Mock<ISignerFileWrapper> _signerFileWrapper;
        private readonly Mock<IConfigurationFileWrapper> _configurationFileWrapper;

        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnector;
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnectorMock;
        private readonly Mock<ISignerTokenMappingConnector> _signerTokenMappingConnectorMock;
        private readonly Mock<ITemplateConnector> _templateConnectorMock;
        private readonly Mock<IDocumentConnector> _documentConnectorMock;
        private readonly Mock<IConfigurationConnector> _configurationConnectorMock;
        private readonly Mock<IContactConnector> _contactConnectorMock;
        private readonly Mock<IOcrService> _ocrConnectorMock;


        public void Dispose()
        {
            _documentCollectionConnectorMock.Invocations.Clear();
            _programConnectorMock.Invocations.Clear();
            _companyConnector.Invocations.Clear();
            _programUtilizationConnectorMock.Invocations.Clear();
            _configurationConnectorMock.Invocations.Clear();
            _documentConnectorMock.Invocations.Clear();
            _contactConnectorMock.Invocations.Clear();
            _templateConnectorMock.Invocations.Clear();
            _signerTokenMappingConnectorMock.Invocations.Clear();
            
            _configurationMock.Invocations.Clear();            
            _documentPdfMock.Invocations.Clear();
            _templatePdfMock.Invocations.Clear();
            _validatorMock.Invocations.Clear();
            _signersConnectorMock.Invocations.Clear();
            _generateLinkHandlerMock.Invocations.Clear();
            _sendingMessageHandlerMock.Invocations.Clear();
            _usersMock.Invocations.Clear();
            _loggerMock.Invocations.Clear();
            _daterMock.Invocations.Clear();
            _appendicesMock.Invocations.Clear();
            _doneActionsHelperMock.Invocations.Clear();
            _memoryCacheMock.Invocations.Clear();
            _xmlHandlerMock.Invocations.Clear();
            _documentsOperationsMock.Invocations.Clear();
            _documentsOperationNotifierMock.Invocations.Clear();
            _ocrConnectorMock.Invocations.Clear();
          

        }

        public DocumentCollectionsHandlerTests()
        {
          
            _folderSettings = Options.Create(new FolderSettings());

            
            _configurationMock = new Mock<IConfiguration>();
            
            _documentPdfMock = new Mock<IDocumentPdf>();
            _templatePdfMock = new Mock<ITemplatePdf>();
            _validatorMock = new Mock<IValidator>();
            _signersConnectorMock = new Mock<ISignersConnector>();
            _generateLinkHandlerMock = new Mock<IGenerateLinkHandler>();
            _sendingMessageHandlerMock = new Mock<ISendingMessageHandler>();
            _usersMock = new Mock<IUsers>();
            _loggerMock = new Mock<ILogger>();
            _daterMock = new Mock<IDater>();
            _appendicesMock = new Mock<IAppendices>();
            _doneActionsHelperMock = new Mock<IDoneActionsHelper>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _xmlHandlerMock = new Mock<IXmlHandler<PDFFields>>();
            _documentsOperationsMock = new Mock<IDocumentCollectionOperations>();
            _documentsOperationNotifierMock = new Mock<IDocumentCollectionOperationsNotifier>();


            _documentFileWrapper = new Mock<IDocumentFileWrapper>();
            _contactFileWrapper = new Mock<IContactFileWrapper>();
            _userFileWrapper = new Mock<IUserFileWrapper>();
            _signerFileWrapper = new Mock<ISignerFileWrapper>();
            _configurationFileWrapper = new Mock<IConfigurationFileWrapper>();

            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();
            _programConnectorMock = new Mock<IProgramConnector>();
            _companyConnector = new Mock<ICompanyConnector>();
            _programUtilizationConnectorMock = new Mock<IProgramUtilizationConnector>();
            _configurationConnectorMock = new Mock<IConfigurationConnector>();
            _documentConnectorMock = new Mock<IDocumentConnector>();
            _contactConnectorMock = new Mock<IContactConnector>();
            _templateConnectorMock = new Mock<ITemplateConnector>();
            _signerTokenMappingConnectorMock = new Mock<ISignerTokenMappingConnector>();
            _ocrConnectorMock = new Mock<IOcrService>();

            _filesWrapper = new FileWrapperStub(_documentFileWrapper.Object, _contactFileWrapper.Object, _userFileWrapper.Object, _signerFileWrapper.Object, _configurationFileWrapper.Object);
            _documentCollectionsHandler = new DocumentCollectionsHandler(_documentCollectionConnectorMock.Object,_programConnectorMock.Object, _companyConnector.Object, _programUtilizationConnectorMock.Object,
                _signerTokenMappingConnectorMock.Object,_templateConnectorMock.Object,_documentConnectorMock.Object,_configurationConnectorMock.Object, _contactConnectorMock.Object,
                _documentPdfMock.Object, _templatePdfMock.Object, _validatorMock.Object, _signersConnectorMock.Object, _configurationMock.Object,
                _usersMock.Object, _sendingMessageHandlerMock.Object,
                _generateLinkHandlerMock.Object, _memoryCacheMock.Object, _loggerMock.Object, 
                _daterMock.Object, _appendicesMock.Object, _xmlHandlerMock.Object,
                _doneActionsHelperMock.Object, _documentsOperationsMock.Object, 
                _documentsOperationNotifierMock.Object,
                _filesWrapper, _ocrConnectorMock.Object);
        }

        #region Create

        [Fact]
        public async Task Create_UserProgramExpired_ShouldThrowException()
        {
            User user = new User();
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            var actual =await  Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Create(new DocumentCollection(), new List<SignerField>()));

            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_ProgramUtilizationGetToMax_ShouldThrowException()
        {
            User user = new User();
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnectorMock.Setup(x => x.CanAddDocument(It.IsAny<User>(), 1)).ReturnsAsync(false);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Create(new DocumentCollection(), new List<SignerField>()));

            Assert.Equal(ResultCode.ProgramUtilizationGetToMax.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_NoDocuments_InvalidTemplateId_ShouldThrowException()
        {
            User user = new User();
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnectorMock.Setup(x => x.CanAddDocument(It.IsAny<User>(), 1)).ReturnsAsync(true);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Create(new DocumentCollection(), new List<SignerField>()));

            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_TemplateDoesNotExist_InvalidTemplateId_ShouldThrowException()
        {
            User user = new User();
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Documents = new List<Document>()
                {
                    new Document()
                    {

                    }
                }
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnectorMock.Setup(x => x.CanAddDocument(It.IsAny<User>(), 1)).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Exists(It.IsAny<Template>())).ReturnsAsync(false);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Create(documentCollection, new List<SignerField>()));

            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_TemplateNotBelongToUserGroup_ShouldThrowException()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        TemplateId = Guid.Parse(GUID),
                    }
                }
            };
            Template template = new Template()
            {
                Id = Guid.Parse(GUID2),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
                _programConnectorMock.Setup(x => x.CanAddDocument(It.IsAny<User>(), 1)).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Create(documentCollection, new List<SignerField>()));

            Assert.Equal(ResultCode.TemplateNotBelongToUserGroup.GetNumericString(), actual.Message);
        }


        [Fact]
        public async Task Create_ContactNotCreatedByUser_ShouldThrowException()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        TemplateId = Guid.Parse(GUID),
                    }
                },
                Signers = new List<Signer>()
                {
                     new Signer()
                    {
                        Id = Guid.Parse(GUID2),
                        Contact = new Contact()
                        {
                            UserId = Guid.Parse(GUID),
                            GroupId = Guid.Parse(GUID2)
                        }
                    },
                }
            };
            Template template = new Template()
            {
                Id = Guid.Parse(GUID),
                GroupId = Guid.Parse(GUID),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnectorMock.Setup(x => x.CanAddDocument(It.IsAny<User>(), 1)).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _contactConnectorMock.Setup(x => x.Read(It.IsAny<Contact>()));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Create(documentCollection, new List<SignerField>()));

            Assert.Equal(ResultCode.ContactNotCreatedByUser.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_SignerMethodNotFeetToContactMeans_ShouldThrowException()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        TemplateId = Guid.Parse(GUID),
                    }
                },
                Signers = new List<Signer>()
                {
                     new Signer()
                    {
                        Id = Guid.Parse(GUID2),
                        Contact = new Contact()
                        {
                            UserId = Guid.Parse(GUID),
                            GroupId = Guid.Parse(GUID),
                            Email=""
                        },
                        SendingMethod = Common.Enums.Documents.SendingMethod.Email
                    },
                }
            };
            Template template = new Template()
            {
                Id = Guid.Parse(GUID),
                GroupId = Guid.Parse(GUID),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnectorMock.Setup(x => x.CanAddDocument(It.IsAny<User>(), 1)).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _contactConnectorMock.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(documentCollection.Signers.First().Contact);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Create(documentCollection, new List<SignerField>()));

            Assert.Equal(ResultCode.SignerMethodNotFeetToContactMeans.GetNumericString(), actual.Message);
        }

        //[Fact]
        //public void Create_FieldNameNotExist_ShouldThrowException()
        //{
        //    User user = new User()
        //    {
        //        GroupId = Guid.Parse(GUID),
        //    };
        //    CompanySigner1Details companySigner1Details = new CompanySigner1Details();
        //    DocumentCollection documentCollection = new DocumentCollection()
        //    {
        //        Documents = new List<Document>()
        //        {
        //            new Document()
        //            {
        //                TemplateId = Guid.Parse(GUID),
        //            }
        //        },
        //        Signers = new List<Signer>()
        //        {
        //             new Signer()
        //            {
        //                Id = Guid.Parse(GUID2),
        //                Contact = new Contact()
        //                {
        //                    UserId = Guid.Parse(GUID),
        //                    GroupId = Guid.Parse(GUID),
        //                    Email="email"
        //                },
        //                SendingMethod = Common.Enums.Documents.SendingMethod.Email,
        //                SignerFields = new List<SignerField>()
        //                {

        //                    new SignerField()
        //                    {
        //                        FieldName = "fieldName",
        //                        TemplateId = Guid.Parse(GUID),
        //                    }
        //                }
        //            },
        //        }
        //    };
        //    Template template = new Template()
        //    {
        //        Id = Guid.Parse(GUID),
        //        GroupId = Guid.Parse(GUID),
        //    };
        //    PDFFields pdfFields = new PDFFields();

        //    _usersMock.Setup(x => x.GetUser(out companySigner1Details)).Returns(user);
        //    _dbConnectorMock.Setup(x => x.Programs.IsProgramExpired(It.IsAny<User>())).Returns(false);
        //    _dbConnectorMock.Setup(x => x.Programs.CanAddDocument(It.IsAny<User>(), 1)).Returns(true);
        //    _dbConnectorMock.Setup(x => x.Templates.Exists(It.IsAny<Template>())).Returns(true);
        //    _dbConnectorMock.Setup(x => x.Templates.Read(It.IsAny<Template>())).Returns(template);
        //    _dbConnectorMock.Setup(x => x.Contacts.Read(It.IsAny<Contact>())).Returns(documentCollection.Signers.First().Contact);

        //    _templatePdfMock.Setup(x => x.SetId(It.IsAny<Guid>())).Callback(() => { });
        //    _templatePdfMock.Setup(x => x.Load(It.IsAny<Guid>(), true)).Callback(() => { });
        //    _templatePdfMock.Setup(x => x.GetAllFields()).Returns(pdfFields);


        //    var actual = Assert.Throws<InvalidOperationException>(() => _documentCollectionsHandler.Create(documentCollection, new List<SignerField>()));

        //    Assert.Equal(ResultCode.FieldNameNotExist.GetNumericString(), actual.Message);
        //}

        ////[Fact]
        ////public void Create_FieldNameNotExist2_ShouldThrowException()
        //{
        //    User user = new User()
        //    {
        //        GroupId = Guid.Parse(GUID),
        //    };
        //    CompanySigner1Details companySigner1Details = new CompanySigner1Details();
        //    DocumentCollection documentCollection = new DocumentCollection()
        //    {
        //        Documents = new List<Document>()
        //        {
        //            new Document()
        //            {
        //                TemplateId = Guid.Parse(GUID),
        //            }
        //        },
        //        Signers = new List<Signer>()
        //        {
        //             new Signer()
        //            {
        //                Id = Guid.Parse(GUID2),
        //                Contact = new Contact()
        //                {
        //                    UserId = Guid.Parse(GUID),
        //                    GroupId = Guid.Parse(GUID),
        //                    Email="email"
        //                },
        //                SendingMethod = Common.Enums.Documents.SendingMethod.Email,
        //                SignerFields = new List<SignerField>()
        //                {

        //                    new SignerField()
        //                    {
        //                        FieldName = "name",
        //                        TemplateId = Guid.Parse(GUID),
        //                    }
        //                }
        //            },
        //        }
        //    };
        //    Template template = new Template()
        //    {
        //        Id = Guid.Parse(GUID),
        //        GroupId = Guid.Parse(GUID),
        //    };
        //    PDFFields pdfFields = new PDFFields()
        //    {
        //        TextFields = new List<TextField>()
        //        {
        //            new TextField()
        //            {
        //                Name= "name",
        //                Description = "description"
        //            }
        //        }
        //    };

        //    var readOnlyFields = new List<SignerField>()
        //    {
        //        //new SignerField()
        //        //{
        //        //    TemplateId = Guid.Parse(GUID),
        //        //    FieldName = "name"
        //        //},
        //    };

        //    _usersMock.Setup(x => x.GetUser(out companySigner1Details)).Returns(user);
        //    _dbConnectorMock.Setup(x => x.Programs.IsProgramExpired(It.IsAny<User>())).Returns(false);
        //    _dbConnectorMock.Setup(x => x.Programs.CanAddDocument(It.IsAny<User>(), 1)).Returns(true);
        //    _dbConnectorMock.Setup(x => x.Templates.Exists(It.IsAny<Template>())).Returns(true);
        //    _dbConnectorMock.Setup(x => x.Templates.Read(It.IsAny<Template>())).Returns(template);
        //    _dbConnectorMock.Setup(x => x.Contacts.Read(It.IsAny<Contact>())).Returns(documentCollection.Signers.First().Contact);

        //    _templatePdfMock.Setup(x => x.SetId(It.IsAny<Guid>())).Callback(() => { });
        //    _templatePdfMock.Setup(x => x.Load(It.IsAny<Guid>(), true)).Callback(() => { });
        //    _templatePdfMock.Setup(x => x.GetAllFields()).Returns(pdfFields);


        //    var actual = Assert.Throws<InvalidOperationException>(() => _documentCollectionsHandler.Create(documentCollection, readOnlyFields));

        //    //Assert.Equal(ResultCode.FieldNameNotExist.GetNumericString(), actual.Message);
        //}

        [Fact]
        public async Task Create_FieldNameNotExist3_ShouldThrowException()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Mode = Common.Enums.Documents.DocumentMode.SelfSign,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        TemplateId = Guid.Parse(GUID),
                    }
                },
                Signers = new List<Signer>()
                {
                     new Signer()
                    {
                        Id = Guid.Parse(GUID2),
                        Contact = new Contact()
                        {
                            UserId = Guid.Parse(GUID),
                            GroupId = Guid.Parse(GUID),
                            Email="email"
                        },
                        SendingMethod = Common.Enums.Documents.SendingMethod.Email,
                        SignerFields = new List<SignerField>()
                        {

                            new SignerField()
                            {
                                FieldName = "name",
                                TemplateId = Guid.Parse(GUID),
                            }
                        },
                        SignerAuthentication = new SignerAuthentication()
                    },
                }
            };
            Template template = new Template()
            {
                Id = Guid.Parse(GUID),
                GroupId = Guid.Parse(GUID),
            };
            PDFFields pdfFields = new PDFFields()
            {
                TextFields = new List<TextField>()
                {
                    new TextField()
                    {
                        Name= "name",
                        Description = "description"
                    }
                }
            };
         
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x  .IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnectorMock.Setup(x => x.CanAddDocument(It.IsAny<User>(), 1)).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _contactConnectorMock.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(documentCollection.Signers.First().Contact);
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company
            {
                CompanyConfiguration = new CompanyConfiguration
                {
                    ShouldNotifyWhileSignerSigned = false,
                }
            });
            _templatePdfMock.Setup(x => x.SetId(It.IsAny<Guid>())).Callback(() => { });
            _templatePdfMock.Setup(x => x.Load(It.IsAny<Guid>(), true)).Callback(() => { });
            _templatePdfMock.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(pdfFields);

            _validatorMock.Setup(x => x.ValidateIsCleanFile(It.IsAny<string>())).Callback(() => { });

            _documentCollectionConnectorMock.Setup(x => x.Create(It.IsAny<DocumentCollection>())).Callback(() => { });
            _templatePdfMock.Setup(x => x.Download());
            _documentPdfMock.Setup(x => x.Create(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<bool>(), It.IsAny<bool>()));
            _documentPdfMock.Setup(x => x.CreateImagesFromExternalList(It.IsAny<IList<PdfImage>>()));

            _programUtilizationConnectorMock.Setup(x => x.AddDocument(It.IsAny<User>()));
            _appendicesMock.Setup(x => x.Create(It.IsAny<DocumentCollection>()));
            _appendicesMock.Setup(x => x.Create(It.IsAny<Guid>(), It.IsAny<Signer>()));

            _loggerMock.Setup(x => x.Information(It.IsAny<string>(), It.IsAny<Guid>()));

            await _documentCollectionsHandler.Create(documentCollection, new List<SignerField>());

            _loggerMock.Verify(x => x.Information(It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
        }

        [Fact]
        public async Task Create_SanitizedAppendices_ShouldSuccess()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Mode = DocumentMode.GroupSign,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        TemplateId = Guid.Parse(GUID),
                    }
                },
                SenderAppendices = new List<Appendix>()
                {
                    new Appendix() {
                    Name = "test with spaces"
                    }
                },
                Signers = new List<Signer>()
                {
                     new Signer()
                    {
                        Id = Guid.Parse(GUID2),
                        Contact = new Contact()
                        {
                            UserId = Guid.Parse(GUID),
                            GroupId = Guid.Parse(GUID),
                            Email="email"
                        },
                        SendingMethod = SendingMethod.Email,
                        SignerAuthentication = new SignerAuthentication()
                    },
                }
            };
            Template template = new Template()
            {
                Id = Guid.Parse(GUID),
                GroupId = Guid.Parse(GUID),
            };
            PDFFields pdfFields = new PDFFields()
            {
                TextFields = new List<TextField>()
                {
                    new TextField()
                    {
                        Name= "name",
                        Description = "description"
                    }
                }
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnectorMock.Setup(x => x.CanAddDocument(It.IsAny<User>(), 1)).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _contactConnectorMock.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(documentCollection.Signers.First().Contact);
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company
            {
                CompanyConfiguration = new CompanyConfiguration
                {
                    ShouldNotifyWhileSignerSigned = false,
                }
            });
            _templatePdfMock.Setup(x => x.SetId(It.IsAny<Guid>())).Callback(() => { });
            _templatePdfMock.Setup(x => x.Load(It.IsAny<Guid>(), true)).Callback(() => { });
            _templatePdfMock.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(pdfFields);

            _validatorMock.Setup(x => x.ValidateIsCleanFile(It.IsAny<string>())).Callback(() => { });

            _documentCollectionConnectorMock.Setup(x => x.Create(It.IsAny<DocumentCollection>())).Callback(() => { });
            _templatePdfMock.Setup(x => x.Download());
            _documentPdfMock.Setup(x => x.Create(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<bool>(), It.IsAny<bool>()));
            _documentPdfMock.Setup(x => x.CreateImagesFromExternalList(It.IsAny<IList<PdfImage>>()));

            _programUtilizationConnectorMock.Setup(x => x.AddDocument(It.IsAny<User>()));
            _appendicesMock.Setup(x => x.Create(It.IsAny<DocumentCollection>()));
            _appendicesMock.Setup(x => x.Create(It.IsAny<Guid>(), It.IsAny<Signer>()));

            _loggerMock.Setup(x => x.Information(It.IsAny<string>(), It.IsAny<Guid>()));
            await _documentCollectionsHandler.Create(documentCollection, new List<SignerField>());

            _documentCollectionConnectorMock.Verify(_ => _.Create(It.Is<DocumentCollection>(dc => dc.SenderAppendices.Any(a => a.Name == "test_with_spaces"))), Times.Once);
        }
        #endregion

        #region Read

        [Fact]
        public async Task Read_InvalidDocumentCollectionId_ShouldThrowException()
        {
            User user = new User();
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = null;

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>());
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Read(new DocumentCollection()));

            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Read_DocumentNotBelongToUserGroup_ShouldThrowException()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                GroupId = Guid.Parse(GUID2),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>());
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Read(new DocumentCollection()));

            Assert.Equal(ResultCode.DocumentNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Read_ReturnDocumentCollection_Success()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                GroupId = Guid.Parse(GUID),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            var actual = await _documentCollectionsHandler.Read(new DocumentCollection());

            Assert.Equal(documentCollection, actual);
        }
        #endregion

        #region Update

        [Fact]
        public async Task Update_InvalidDocumentCollectionId_ShouldThrowException()
        {
            DocumentCollection documentCollection = null;

            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Update(new DocumentCollection(), new List<SignerField>()));

            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_DocumentNotBelongToUserGroup_ShouldThrowException()
        {
            DocumentCollection documentCollection = new DocumentCollection()
            {
                GroupId = Guid.Parse(GUID),
                UserId = Guid.Parse(GUID),
                Mode = Common.Enums.Documents.DocumentMode.Online
            };
            User user = new User()
            {
                GroupId = Guid.Parse(GUID2),
                Id = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();

            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Update(new DocumentCollection(), new List<SignerField>()));

            Assert.Equal(ResultCode.DocumentNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public void Update_WithNoDocuments_Success()
        {
            DocumentCollection documentCollection = new DocumentCollection()
            {
                GroupId = Guid.Parse(GUID),
                UserId = Guid.Parse(GUID),
                Mode = Common.Enums.Documents.DocumentMode.SelfSign
            };
            User user = new User()
            {
                GroupId = Guid.Parse(GUID2),
                Id = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();

            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));

            _documentCollectionConnectorMock.Setup(x => x.Update(It.IsAny<DocumentCollection>())).Callback(() => { });
            _loggerMock.Setup(x => x.Information(It.IsAny<string>(), It.IsAny<Guid>()));

            _documentCollectionsHandler.Update(new DocumentCollection(), new List<SignerField>());

            _loggerMock.Verify(x => x.Information(It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
        }
        #endregion

        #region Delete

        [Fact]
        public async Task Delete_InvalidDocumentCollectionId_ShouldThrowException()
        {
            User user = new User()
            {
                Id = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = null;

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _validatorMock.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>());

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Delete(new DocumentCollection()));

            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Delete_DocumentNotBelongToUserGroup_ShouldThrowException()
        {
            User user = new User()
            {
                Id = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID),
                GroupId = Guid.Parse(GUID),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _validatorMock.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Delete(new DocumentCollection()));

            Assert.Equal(ResultCode.DocumentNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Delete_UpdateStatus_Success()
        {
            User user = new User()
            {
                Id = Guid.Parse(GUID),
                GroupId = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                GroupId = Guid.Parse(GUID),
                UserId = Guid.Parse(GUID),
                DocumentStatus = Common.Enums.Documents.DocumentStatus.Created,
                Mode = DocumentMode.SelfSign
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _validatorMock.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            _loggerMock.Setup(x => x.Information(It.IsAny<string>())).Callback(() => { });
            _documentCollectionConnectorMock.Setup(x => x.UpdateStatus(documentCollection, Common.Enums.Documents.DocumentStatus.Deleted)).Callback(() =>
            documentCollection.DocumentStatus = DocumentStatus.Deleted);

            await _documentCollectionsHandler.Delete(documentCollection);

            Assert.Equal(DocumentStatus.Deleted, documentCollection.DocumentStatus);

        }

        #endregion

        #region Cancel

        [Fact]
        public async Task Cancel_InvalidDocumentCollectionId_ShouldThrowException()
        {
            User user = new User()
            {
                Id = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _validatorMock.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>());

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Cancel(new DocumentCollection()));

            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Cancel_DocumentNotBelongToUserGroup_ShouldThrowException()
        {
            User user = new User()
            {
                Id = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID),
                GroupId = Guid.Parse(GUID),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _validatorMock.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Cancel(new DocumentCollection()));

            Assert.Equal(ResultCode.DocumentNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Cancel_CannotCancelSignedDocument_ShouldThrowException()
        {
            User user = new User()
            {
                Id = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
                DocumentStatus = DocumentStatus.Signed
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _validatorMock.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Cancel(new DocumentCollection()));

            Assert.Equal(ResultCode.CannotCancelSignedDocument.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Cancel_DocumentAlreadyCanceled_ShouldThrowException()
        {
            User user = new User()
            {
                Id = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
                DocumentStatus = DocumentStatus.Canceled
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _validatorMock.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Cancel(new DocumentCollection()));

            Assert.Equal(ResultCode.DocumentAlreadyCanceled.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Cancel_Success()
        {
            User user = new User()
            {
                Id = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
                DocumentStatus = DocumentStatus.Sent,

            };
                
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _validatorMock.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            _signerTokenMappingConnectorMock.Setup(x => x.Delete(It.IsAny<SignerTokenMapping>()));
            _documentCollectionConnectorMock.Setup(x => x.UpdateStatus(documentCollection, It.IsAny<DocumentStatus>())).
                Callback(() => {
                    documentCollection.DocumentStatus = DocumentStatus.Canceled;
                }
                );

            _loggerMock.Setup(x => x.Information(It.IsAny<string>()));
            _loggerMock.Setup(x => x.Information(It.IsAny<string>(), It.IsAny<object>()));
            await _documentCollectionsHandler.Cancel(new DocumentCollection());
            Assert.Equal(DocumentStatus.Canceled, documentCollection.DocumentStatus);
        }

        [Fact]
        public async Task Cancel_SendNotification_ShouldSuccess()
        {
            User user = new User()
            {
                Id = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            DocumentCollection documentCollection = new DocumentCollection()
            {
                UserId = Guid.Parse(GUID2),
                GroupId = Guid.Parse(GUID2),
                DocumentStatus = DocumentStatus.Sent,

            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _validatorMock.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            _signerTokenMappingConnectorMock.Setup(x => x.Delete(It.IsAny<SignerTokenMapping>()));
            _documentCollectionConnectorMock.Setup(x => x.UpdateStatus(documentCollection, It.IsAny<DocumentStatus>())).
                Callback(() => {
                    documentCollection.DocumentStatus = DocumentStatus.Canceled;
                }
                );

            _loggerMock.Setup(x => x.Information(It.IsAny<string>()));
            _loggerMock.Setup(x => x.Information(It.IsAny<string>(), It.IsAny<object>()));
           await _documentCollectionsHandler.Cancel(documentCollection);
            _documentsOperationNotifierMock.Verify(_ => _.AddNotification(It.IsAny<DocumentCollection>(), DocumentNotification.DocumentCanceled, It.IsAny<Signer>()), Times.Once);
        }

        #endregion

        #region Download

        [Fact]
        public async Task Download_UserProgramExpired_ShouldThrowException()
        {
            User user = new User();
         
            DocumentCollection documentCollection = new DocumentCollection();


            _usersMock.Setup(x => x.GetUser());
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Download(documentCollection));

            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Download_InvalidDocumentCollectionId_ShouldThrowException()
        {
            User user = new User();
          
            DocumentCollection documentCollection = null;


            _usersMock.Setup(x => x.GetUser());
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Download(documentCollection));

            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Download_CannotDownloadUnsignedDocument_ShouldThrowException()
        {
            User user = new User();
      
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Mode = DocumentMode.SelfSign,
                DocumentStatus = DocumentStatus.Viewed,
            };


            _usersMock.Setup(x => x.GetUser());
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _documentCollectionConnectorMock.Setup(x => x.ReadWithTemplateInfo(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Download(documentCollection));

            Assert.Equal(ResultCode.CannotDownloadUnsignedDocument.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Download_DocumentNotBelongToUserGroup_ShouldThrowException()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
                Id = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = null;
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Mode = DocumentMode.OrderedGroupSign,
                DocumentStatus = DocumentStatus.Signed,
                GroupId = Guid.Parse(GUID2),
                UserId = Guid.Parse(GUID2),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _documentCollectionConnectorMock.Setup(x => x.ReadWithTemplateInfo(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Download(documentCollection));

            Assert.Equal(ResultCode.DocumentNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Download_DocumentNotBelongToUserGroup2_ShouldThrowException()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID2),
                Id = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = null;
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Mode = DocumentMode.GroupSign,
                DocumentStatus = DocumentStatus.Canceled,
                GroupId = Guid.Parse(GUID),
                UserId = Guid.Parse(GUID2),
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _documentCollectionConnectorMock.Setup(x => x.ReadWithTemplateInfo(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _documentCollectionsHandler.Download(documentCollection));

            Assert.Equal(ResultCode.DocumentNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]        
        public async Task Download_FileBundleNotExist_ShouldThrowException()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
                Id = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = null;
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Mode = DocumentMode.GroupSign,
                DocumentStatus = DocumentStatus.Sent,
                GroupId = Guid.Parse(GUID),
                UserId = Guid.Parse(GUID2),
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        Id = Guid.Parse(GUID),
                        Name = "name"
                    }
                }
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _documentCollectionConnectorMock.Setup(x => x.ReadWithTemplateInfo(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _documentFileWrapper.Setup(x => x.IsDocumentExist(DocumentType.Document, It.IsAny<Guid>())).Returns(false);

                
            

            var actual =await  Assert.ThrowsAsync<Exception>(() => _documentCollectionsHandler.Download(documentCollection));

            var expected = $"File [{DocumentType.Document}]  [{GUID}] not exist".ToLower();

            Assert.Equal(expected, actual.Message.ToLower());
        }

        [Fact]
        public async Task Download_ReturnDocuments_Success()
        {
            User user = new User()
            {
                GroupId = Guid.Parse(GUID),
                Id = Guid.Parse(GUID),
            };
            CompanySigner1Details companySigner1Details = null;
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Mode = DocumentMode.GroupSign,
                DocumentStatus = DocumentStatus.Sent,
                GroupId = Guid.Parse(GUID),
                UserId = Guid.Parse(GUID2),
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        Id = Guid.Parse(GUID),
                        Name = "name1",
                    },
                    new Document()
                    {
                        Id = Guid.Parse(GUID2),
                        Name = "name2",
                    },
                }
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _programConnectorMock.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _documentCollectionConnectorMock.Setup(x => x.ReadWithTemplateInfo(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _documentFileWrapper.Setup(x => x.IsDocumentExist(DocumentType.Document, It.IsAny<Guid>())).Returns(true);


            var actual = await _documentCollectionsHandler.Download(documentCollection);

            //var a = actual.TryGetValue(Guid.Parse(GUID), out )

            Assert.Equal(2, actual.Count);
        }

        #endregion

    }
}
