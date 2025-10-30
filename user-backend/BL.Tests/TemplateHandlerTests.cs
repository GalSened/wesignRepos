using BL.Handlers;
using Common.Enums;
using Common.Enums.Results;
using Common.Enums.Templates;
using Common.Extensions;
using Common.Handlers.Files;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.Files.PDF;
using Common.Models.Settings;
using Common.Models.XMLModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using PdfHandler.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class TemplateHandlerTests : IDisposable
    {
        private const string TEMPLATE_BASE64FILE = "example_base64file";
        private const string MERGE_TEMPLATES_NAME = "merge_templates_name";
        private const string TEMPLATE_NAME = "template_name";
        private readonly Guid TEMPLATE_GROUP_ID = Guid.NewGuid();
        private readonly Guid USER_GROUP_ID = Guid.NewGuid();

        //private readonly ClaimsPrincipal _user;
        
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnectorMock;
        private readonly Mock<IDocumentConnector> _documentConnectorMock;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<ITemplateConnector> _templateConnectorMock;
        private readonly Mock<ITemplatePdf> _templatePdfMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IValidator> _validatorMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IFile> _fileMock;
        private readonly Mock<IPath> _pathMock;
        private readonly Mock<IOptions<GeneralSettings>> _optionsGeneralSettingsMock;
        private readonly Mock<GeneralSettings> _generalSettingsMock;
        private readonly Mock<IUsers> _usersMock;
        private readonly Mock<IDataUriScheme> _dataUriSchemeMock;
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<IOptions<FolderSettings>> _optionsFolderSettingsMock;
        private readonly Mock<FolderSettings> _folderSettingsMock;
        private readonly Mock<IXmlHandler<PDFMetaData>> _xmlHandlerMock;
        private readonly Mock<IContacts> _contactsMock;
        private readonly Mock<IPdfConverter> _pdfConverterMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IExternalPDFService> _externalPDFServiceMock;
        private readonly ITemplates _templatesHandler;
        private readonly IFilesWrapper _fileWrapperMock;

        private readonly Mock<IDocumentFileWrapper> _documentFileWrapper;
        private readonly Mock<IContactFileWrapper> _contactFileWrapper;
        private readonly Mock<IUserFileWrapper> _userFileWrapper;
        private readonly Mock<ISignerFileWrapper> _signerFileWrapper;
        private readonly Mock<IConfigurationFileWrapper> _configurationFileWrapper;
        private readonly Mock<IUserConnector>_userConnectorMock;
        private readonly Mock<IOcrService> _ocrConnectorMock;


        public TemplateHandlerTests()
        {
           
            _programUtilizationConnectorMock = new Mock<IProgramUtilizationConnector>();
            _documentConnectorMock = new Mock<IDocumentConnector>();
            _programConnectorMock = new Mock<IProgramConnector>();
            _templateConnectorMock = new Mock<ITemplateConnector>();
            _userConnectorMock=  new Mock<IUserConnector>();
           
            _templatePdfMock = new Mock<ITemplatePdf>();
            _loggerMock = new Mock<ILogger>();
            _validatorMock = new Mock<IValidator>();
            _fileSystemMock = new Mock<IFileSystem>();
            _fileMock = new Mock<IFile>();
            _pathMock = new Mock<IPath>();
            _fileSystemMock.SetupGet(_ => _.File).Returns(_fileMock.Object);
            _fileSystemMock.SetupGet(_ => _.Path).Returns(_pathMock.Object);
            _optionsGeneralSettingsMock = new Mock<IOptions<GeneralSettings>>();
            _generalSettingsMock = new Mock<GeneralSettings>();
            _optionsGeneralSettingsMock.SetupGet(_ => _.Value).Returns(_generalSettingsMock.Object);
            _usersMock = new Mock<IUsers>();
            _dataUriSchemeMock = new Mock<IDataUriScheme>();
            _daterMock = new Mock<IDater>();
            _optionsFolderSettingsMock = new Mock<IOptions<FolderSettings>>();
            _folderSettingsMock = new Mock<FolderSettings>();
            _optionsFolderSettingsMock.SetupGet(_ => _.Value).Returns(_folderSettingsMock.Object);
            _xmlHandlerMock = new Mock<IXmlHandler<PDFMetaData>>();
            _contactsMock = new Mock<IContacts>();
            _pdfConverterMock = new Mock<IPdfConverter>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _externalPDFServiceMock = new Mock<IExternalPDFService>();
           

            _documentFileWrapper = new Mock<IDocumentFileWrapper>();
            _contactFileWrapper = new Mock<IContactFileWrapper>();
            _userFileWrapper = new Mock<IUserFileWrapper>();
            _signerFileWrapper = new Mock<ISignerFileWrapper>();
            _configurationFileWrapper = new Mock<IConfigurationFileWrapper>();
            _ocrConnectorMock = new Mock<IOcrService>();

            _fileWrapperMock = new FileWrapperStub(_documentFileWrapper.Object, _contactFileWrapper.Object, _userFileWrapper.Object, _signerFileWrapper.Object, _configurationFileWrapper.Object);


            _templatesHandler = new TemplatesHandler(_templateConnectorMock.Object,_programConnectorMock.Object,_documentConnectorMock.Object,
                _programUtilizationConnectorMock.Object, _templatePdfMock.Object, _loggerMock.Object, _userConnectorMock.Object,
                _validatorMock.Object, _optionsGeneralSettingsMock.Object, _usersMock.Object,
                _dataUriSchemeMock.Object, _daterMock.Object,
                _xmlHandlerMock.Object, _contactsMock.Object, _pdfConverterMock.Object, _memoryCacheMock.Object, _externalPDFServiceMock.Object,
                _fileWrapperMock, _ocrConnectorMock.Object);

            
        
        }

        public void Dispose()
        {
            
            _programUtilizationConnectorMock.Invocations.Clear();
            _userConnectorMock.Invocations.Clear();
            _documentConnectorMock.Invocations.Clear();
            _programConnectorMock.Invocations.Clear();
            _templatePdfMock.Invocations.Clear();
            _loggerMock.Invocations.Clear();
            _validatorMock.Invocations.Clear();
            _fileSystemMock.Invocations.Clear();
            _fileMock.Invocations.Clear();
            _pathMock.Invocations.Clear();
            _optionsGeneralSettingsMock.Invocations.Clear();
            _generalSettingsMock.Invocations.Clear();
            _usersMock.Invocations.Clear();
            _dataUriSchemeMock.Invocations.Clear();
            _daterMock.Invocations.Clear();
            _optionsFolderSettingsMock.Invocations.Clear();
            _folderSettingsMock.Invocations.Clear();
            _xmlHandlerMock.Invocations.Clear();
            _contactsMock.Invocations.Clear();
            _pdfConverterMock.Invocations.Clear();
            _memoryCacheMock.Invocations.Clear();
            _externalPDFServiceMock.Invocations.Clear();
            _ocrConnectorMock.Invocations.Clear();
        }

        #region MergeTemplates

        [Fact]
        public async Task MergeTemplates_InvalidTemplateId_ThrowInvalidOperationException()
        {
            // Arrange
            Template template = new Template() { Id = Guid.NewGuid() };
            Template dbTemplate = new Template() { GroupId = TEMPLATE_GROUP_ID };
            List<Template> templates = new List<Template>() { template };
            MergeTemplates mergeTemplates = new MergeTemplates() { Templates = templates };
            User user = new User() { GroupId = USER_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
          //  _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).Returns(dbTemplate);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.MergeTemplates(mergeTemplates));

            // Assert
            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task MergeTemplates_InvalidFileType_ThrowInvalidOperationException()
        {
            // Arrange
            Template template = new Template() { Id = Guid.Empty };
            List<Template> templates = new List<Template>() { template };
            MergeTemplates mergeTemplates = new MergeTemplates() { Templates = templates };
            User user = new User();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _dataUriSchemeMock.Setup(_ => _.IsValidFileType(It.IsAny<string>(), out It.Ref<FileType>.IsAny)).Returns(false);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.MergeTemplates(mergeTemplates));

            // Assert
            Assert.Equal(ResultCode.InvalidFileType.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task MergeTemplates_CreateTemplateFailed_ThrowException()
        {
            // Arrange
            Template template = new Template() { Id = Guid.Empty, FileType = FileType.PDF };
            Template dbTemplate = new Template();
            List<Template> templates = new List<Template> { template };
            MergeTemplates mergeTemplates = new MergeTemplates() { Templates = templates, Name = MERGE_TEMPLATES_NAME };
            User user = new User();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _dataUriSchemeMock.Setup(_ => _.IsValidFileType(It.IsAny<string>(), out It.Ref<FileType>.IsAny)).Returns(true);
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), out It.Ref<int>.IsAny))
                .Returns(Enumerable.Empty<Template>());
            _dataUriSchemeMock.Setup(_ => _.Getbase64Content(It.IsAny<string>())).Returns(TEMPLATE_BASE64FILE);
            _externalPDFServiceMock.Setup(_ => _.Merge(It.IsAny<List<string>>())).ReturnsAsync(TEMPLATE_BASE64FILE);
            _templatePdfMock.Setup(_ => _.Create(It.IsAny<Guid>(), It.IsAny<string>())).Returns(false);

            // Action
            var actual =await Assert.ThrowsAsync<Exception>(() => _templatesHandler.MergeTemplates(mergeTemplates));

            // Assert
            Assert.Contains($"Failed to create template PDF for: Template - Name: {template.Name}", actual.Message);

        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ProgramUtilizationGetToMax_ThrowInvalidOperationException()
        {
            // Arrange
            Template template = new Template() { Status = TemplateStatus.MultipleUse };
            User user = new User();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.CanAddTemplate(It.IsAny<User>())).ReturnsAsync(false);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.Create(template));

            // Assert
            Assert.Equal(ResultCode.ProgramUtilizationGetToMax.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_CreateTemplateFailed_ThrowException()
        {
            // Arrange
            Template template = new Template() { Status = TemplateStatus.OneTimeUse };
            User user = new User();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.CanAddTemplate(It.IsAny<User>())).ReturnsAsync(true);

            // Action
            var actual =await Assert.ThrowsAsync<Exception>(() => _templatesHandler.Create(template));

            // Assert
            Assert.Contains("Failed to create template PDF", actual.Message);
        }

        [Fact]
        public async Task Create_Valid_ShouldSuccess()
        {
            // Arrange
            Template template = new Template() { Status = TemplateStatus.OneTimeUse };
            User user = new User();
            PDFFields pDFFields = new PDFFields();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.CanAddTemplate(It.IsAny<User>())).ReturnsAsync(true);
            _templatePdfMock.Setup(_ => _.Create(It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);
            _templatePdfMock.Setup(_ => _.GetAllFields(It.IsAny<bool>())).Returns(pDFFields);

            // Action
            await _templatesHandler.Create(template);

            // Assert
            _templatePdfMock.Verify(_ => _.CreateImagesFromPdfInFileSystem(), Times.Once());
            _templatePdfMock.Verify(_ => _.GetAllFields(It.IsAny<bool>()), Times.Once);
        }

        #endregion

        #region IsTemplateInUse

        [Fact]
        public async Task IsTemplateInUse_NotInUse_ShouldReturnFalse()
        {
            // Arrange
            Template template = new Template();
            _documentConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync((Document)null);

            // Action
            var actual =await _templatesHandler.IsTemplateInUse(template);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public async Task IsTemplateInUse_InUse_ShouldReturnTrue()
        {
            // Arrange
            Template template = new Template();
            Document document = new Document();
            _documentConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(document);

            // Action
            var actual =await _templatesHandler.IsTemplateInUse(template);

            // Assert
            Assert.True(actual);
        }

        #endregion

        #region DeleteBatch

        [Fact]
        public async Task DeleteBatch_TemplateNotBelongToUserGroup_ThrowInvalidOperationException()
        {
            // Arrange
            RecordsBatch recordsBatch = new RecordsBatch() { Ids = Enumerable.Empty<Guid>().ToList() };
            User user = new User() { GroupId = USER_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            List<Template> templates = new List<Template>() { template };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<List<Guid>>())).Returns(templates);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.DeleteBatch(recordsBatch));

            // Assert
            Assert.Equal(ResultCode.TemplateNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task DeleteBatch_ValidRecordsBatch_ShouldSuccess()
        {
            // Arrange
            RecordsBatch recordsBatch = new RecordsBatch() { Ids = Enumerable.Empty<Guid>().ToList() };
            User user = new User() { GroupId = USER_GROUP_ID };
            Template template = new Template() { GroupId = USER_GROUP_ID };
            List<Template> templates = new List<Template>() { template };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<List<Guid>>())).Returns(templates);

            // Action
            await _templatesHandler.DeleteBatch(recordsBatch);

            // Assert
            _programUtilizationConnectorMock.Verify(_ => _.UpdateTemplatesAmount(It.IsAny<User>(), It.IsAny<CalcOperation>(), It.IsAny<int>()), Times.Once);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_InvalidTemplateId_ThrowInvalidOperationException()
        {
            // Arrange
            Template template = new Template();
            User user = new User();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Exists(It.IsAny<Template>())).ReturnsAsync(false);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.Delete(template));

            // Assert
            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Delete_TemplateNotBelongToUserGroup_ThrowInvalidOperationException()
        {
            // Arrange
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            User user = new User() { GroupId = USER_GROUP_ID };
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Exists(It.IsAny<Template>())).ReturnsAsync(true);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.Delete(template));

            // Assert
            Assert.Equal(ResultCode.TemplateNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Delete_ValidTemplate_ShouldSuccess()
        {
            // Arrange
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            User user = new User() { GroupId = TEMPLATE_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);

            // Action
            await _templatesHandler.Delete(template);

            // Assert
            _templateConnectorMock.Verify(_ => _.Delete(It.IsAny<Template>()), Times.Once);
            _programUtilizationConnectorMock.Verify(_ => _.UpdateTemplatesAmount(It.IsAny<User>(), It.IsAny<CalcOperation>(), It.IsAny<int>()), Times.Once);
        }

        #endregion

        #region ReadDeletedTemplates

        [Fact]
        public void ReadDeletedTemplates_EmptyDeletedTemplates_ShouldSuccess()
        {
            // Arrange
            _templateConnectorMock.Setup(_ => _.ReadDeletedTemplates()).Returns(Enumerable.Empty<Template>());

            // Action
            _templatesHandler.ReadDeletedTemplates();

            // Assert
            _templateConnectorMock.Verify(_ => _.ReadDeletedTemplates(), Times.Once);
        }

        #endregion

        #region DuplicateTemplate

        [Fact]
        public async Task DuplicateTemplate_ProgramUtilizationGetToMax_ThrowInvalidOperationException()
        {
            // Arrange
            Template template = new Template() { Status = TemplateStatus.MultipleUse };
            User user = new User();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.CanAddTemplate(It.IsAny<User>())).ReturnsAsync(false);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.DuplicateTemplate(template));

            // Assert
            Assert.Equal(ResultCode.ProgramUtilizationGetToMax.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task DuplicateTemplate_InvalidTemplateId_ThrowInvalidOperationException()
        {
            // Arrange
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            User user = new User() { GroupId = USER_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.CanAddTemplate(It.IsAny<User>())).ReturnsAsync(true);
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.DuplicateTemplate(template));

            // Assert
            Assert.Equal(ResultCode.TemplateNotBelongToUserGroup.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task DuplicateTemplate_ValidTemplate_ShouldSuccess()
        {
            // Arrange
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID, Name = TEMPLATE_NAME };
            User user = new User() { GroupId = TEMPLATE_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.CanAddTemplate(It.IsAny<User>())).ReturnsAsync(true);
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _templateConnectorMock.Setup(_ => _.Exists(It.IsAny<Template>())).ReturnsAsync(true);

            // Action
            Template duplicateTemplate = await _templatesHandler.DuplicateTemplate(template);

            // Assert
            Assert.Equal(duplicateTemplate.GroupId, template.GroupId);
        }

        #endregion

        #region GetPageByTemplateId

        [Fact]
        public async Task GetPageByTemplateId_InvalidTemplateId_ThrowInvalidOperationException()
        {
            // Arrange
            User user = new User() { GroupId = USER_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            int page = 0;
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.GetPageByTemplateId(template, page));

            // Assert
            Assert.Equal(ResultCode.TemplateNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetPageByTemplateId_ValidTemplate_ShouldSuccess()
        {
            // Arrange
            User user = new User() { GroupId = TEMPLATE_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            PdfImage pdfImage = new PdfImage();
            int page = 0;
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _templateConnectorMock.Setup(_ => _.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templatePdfMock.Setup(_ => _.GetPdfImageByIndex(It.IsAny<int>(), It.IsAny<Guid>())).Returns(pdfImage);


            // Action
            await _templatesHandler.GetPageByTemplateId(template, page);

            // Assert
            _templatePdfMock.Verify(_ => _.SetId(It.IsAny<Guid>()), Times.Once);
            _templatePdfMock.Verify(_ => _.GetPdfImageByIndex(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
            _templatePdfMock.Verify(_ => _.GetAllFields(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            _templateConnectorMock.Verify(_ => _.Read(It.IsAny<Template>()), Times.Exactly(1));
        }

        #endregion

        #region GetPagesByTemplateId

        [Fact]
        public async Task GetPagesByTemplateId_InvalidTemplateId_ThrowInvalidOperationException()
        {
            // Arrange
            User user = new User() { GroupId = USER_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.GetPagesByTemplateId(template, 0, 0));

            // Assert
            Assert.Equal(ResultCode.TemplateNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetPagesByTemplateId_Valid_ShouldSuccess()
        {
            // Arrange
            User user = new User() { GroupId = TEMPLATE_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _templateConnectorMock.Setup(_ => _.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templatePdfMock.Setup(_ => _.GetPdfImages(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).Returns(Enumerable.Empty<PdfImage>().ToList());

            // Action
            await _templatesHandler.GetPagesByTemplateId(template, 0, 0);

            // Assert
            _templateConnectorMock.Verify(_ => _.Read(It.IsAny<Template>()), Times.Exactly(1));
        }

        #endregion

        #region GetPagesCountByTemplateId

        [Fact]
        public async Task GetPagesCountByTemplateId_InvalidTemplateId_ThrowInvalidOperationException()
        {
            // Arrange
            User user = new User() { GroupId = USER_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.GetPagesCountByTemplateId(template));

            // Assert
            Assert.Equal(ResultCode.TemplateNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        #endregion

        #region Read

        [Fact]
        public async Task Read_Valid_ShouldSuccess()
        {
            // Arrange
            User user = new User() { GroupId = TEMPLATE_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            IEnumerable<Template> templates = new List<Template>() { template };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), out It.Ref<int>.IsAny))
                .Returns(templates);

            // Action
            (IEnumerable<Template> resultedTemplates, int totalCount) = await  _templatesHandler.Read(string.Empty, string.Empty, string.Empty, 0, 0, true, true);

            // Assert
            Assert.Equal(templates, resultedTemplates);
        }

        #endregion

        #region GetTemplateByTemplateId

        [Fact]
        public async Task GetTemplateByTemplateId_InvalidTemplateId_ThrowInvalidOperationException()
        {
            // Arrange
            User user = new User() { GroupId = USER_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.GetTemplateByTemplateId(template));

            // Assert
            Assert.Equal(ResultCode.TemplateNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetTemplateByTemplateId_Valid_ShouldSuccess()
        {
            // Arrange
            User user = new User() { GroupId = TEMPLATE_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _templateConnectorMock.Setup(_ => _.Exists(It.IsAny<Template>())).ReturnsAsync(true);

            // Action
            Template resultedTemplate = await _templatesHandler.GetTemplateByTemplateId(template);

            // Assert
            Assert.Equal(template, resultedTemplate);
        }

        #endregion

        #region Download

        [Fact]
        public async Task Download_UserProgramExpired_ThrowInvalidOperationException()
        {
            // Arrange
            User user = new User();
            Template template = new Template();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.Download(template));

            // Assert
            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Download_InvalidTemplateId_ThrowInvalidOperationException()
        {
            // Arrange
            User user = new User();
            Template template = null;
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.Download(template));

            // Assert
            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Download_FileNotExists_ThrowException()
        {
            // Arrange
            User user = new User() { GroupId = TEMPLATE_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID, Id = Guid.NewGuid() };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);
           
            _documentFileWrapper.Setup(x => x.IsDocumentExist(DocumentType.Template, It.IsAny<Guid>())).Returns(false);
            // Action
            var actual = await Assert.ThrowsAsync<Exception>(() => _templatesHandler.Download(template));

            // Assert
            Assert.Equal($"File type [{DocumentType.Template}] [{template.Id}]  not exist", actual.Message);
        }

        [Fact]
        public async Task Download_Valid_ShouldSuccess()
        {
            // Arrange
            User user = new User() { GroupId = TEMPLATE_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID, Id = Guid.NewGuid(), Name = TEMPLATE_NAME };
            byte[] exampleByteArray = new byte[5];
            Random random = new Random();
            random.NextBytes(exampleByteArray);
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _documentFileWrapper.Setup(x => x.IsDocumentExist(DocumentType.Template, It.IsAny<Guid>())).Returns(true);
            
            _documentFileWrapper.Setup(x => x.ReadDocument(DocumentType.Template, It.IsAny<Guid>())).Returns(exampleByteArray);
            // Action
            (string name, byte[] content) downloadResult =await _templatesHandler.Download(template);

            // Assert
            Assert.Equal((template.Name, exampleByteArray), downloadResult);
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_UserProgramExpired_ThrowInvalidOperationException()
        {
            // Arrange
            User user = new User();
            Template template = new Template();
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.Update(template));

            // Assert
            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_InvalidTemplateId_ThrowInvalidOperationException()
        {
            // Arrange
            User user = new User() { GroupId = USER_GROUP_ID};
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _programConnectorMock.Setup(_ => _.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _templatesHandler.Update(template));

            // Assert
            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_Valid_ShouldSuccess()
        {
            // Arrange
            User user = new User() { GroupId = TEMPLATE_GROUP_ID };
            Template template = new Template() { GroupId = TEMPLATE_GROUP_ID };
            _usersMock.Setup(_ => _.GetUser()).ReturnsAsync((user, It.Ref<CompanySigner1Details>.IsAny));
            _templateConnectorMock.Setup(_ => _.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _programConnectorMock.Setup(_ => _.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _templateConnectorMock.Setup(_ => _.Exists(It.IsAny<Template>())).ReturnsAsync(true);

            // Action
            await _templatesHandler.Update(template);

            // Assert
            _templateConnectorMock.Verify(_ => _.Update(It.IsAny<Template>()), Times.Once);
        }

        #endregion
    }
}
