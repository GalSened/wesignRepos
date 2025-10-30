using Common.Consts;
using Common.Enums.Companies;
using Common.Enums.Documents;
using Common.Enums.PDF;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.ManagementApp.Reports;
using Common.Models.Programs;
using Common.Models.Reports;
using Common.Models.Settings;
using ManagementBL.Handlers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Threading.Tasks;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.Reports;
using Common.Enums.Reports;
using Common.Models.ManagementApp;

namespace ManagementBL.Tests.Handlers
{
    public class ReportsHandlerTests : IDisposable
    {
        private readonly IOptions<GeneralSettings> _generalSettings;
        
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly ReportsHandler _report;
        private readonly Mock<IManagementHistoryReports> _managementHistoryReportsMock;
        private readonly Mock<IHistoryDocumentCollection> _historyDocumentCollectionMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<ITemplateConnector> _templateConnectorMock;
        private readonly Mock<IGroupConnector> _groupConnectorMock;
        private readonly Mock<IUserConnector> _userConnectorMock;
        private readonly Mock<Common.Interfaces.ManagementApp.IUsers> _users;
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<IProgramUtilizationHistoryConnector> _programUtilizationHistoryConnectorMock;
        private readonly Mock<IManagementPeriodicReportConnector> _managementPeriodicReportConnectorMock;
        private readonly Mock<IManagementPeriodicReportEmailConnector> _managementPeriodicReportEmailConnectorMock;

        private Guid _sampleGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public ReportsHandlerTests()
        {
            _generalSettings = Options.Create(new GeneralSettings());
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _daterMock = new Mock<IDater>();
            _managementHistoryReportsMock = new Mock<IManagementHistoryReports>();
            _historyDocumentCollectionMock = new Mock<IHistoryDocumentCollection>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _templateConnectorMock = new Mock<ITemplateConnector>();    
            _groupConnectorMock = new Mock<IGroupConnector>();
            _userConnectorMock = new Mock<IUserConnector>();
            _users = new Mock<Common.Interfaces.ManagementApp.IUsers>();
            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();
            _programConnectorMock = new Mock<IProgramConnector>();
            _programUtilizationHistoryConnectorMock = new Mock<IProgramUtilizationHistoryConnector>();
            _managementPeriodicReportConnectorMock = new Mock<IManagementPeriodicReportConnector>();
            _managementPeriodicReportEmailConnectorMock = new Mock<IManagementPeriodicReportEmailConnector>();
            var serviceCollection = new ServiceCollection();

            // Add any DI stuff here:
            serviceCollection.AddSingleton(_companyConnectorMock.Object);
            serviceCollection.AddSingleton(_templateConnectorMock.Object);
            serviceCollection.AddSingleton(_groupConnectorMock.Object);
            serviceCollection.AddSingleton(_userConnectorMock.Object);
            serviceCollection.AddSingleton(_documentCollectionConnectorMock.Object);
            serviceCollection.AddSingleton(_programConnectorMock.Object);
            serviceCollection.AddSingleton(_programUtilizationHistoryConnectorMock.Object);
            serviceCollection.AddSingleton(_managementPeriodicReportConnectorMock.Object);
            serviceCollection.AddSingleton(_managementPeriodicReportEmailConnectorMock.Object);
            // Create the ServiceProvider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // serviceScopeMock will contain my ServiceProvider
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider)
                .Returns(serviceProvider);

            // serviceScopeFactoryMock will contain my serviceScopeMock
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeFactoryMock.Setup(s => s.CreateScope())
                .Returns(serviceScopeMock.Object);



            _report = new ReportsHandler(_generalSettings, _serviceScopeFactoryMock.Object, _daterMock.Object, _users.Object, 
                _managementHistoryReportsMock.Object, _historyDocumentCollectionMock.Object);
        }

        public void Dispose()
        {
            _serviceScopeFactoryMock.Invocations.Clear();
            _daterMock.Invocations.Clear();
            _managementHistoryReportsMock.Invocations.Clear();
            _historyDocumentCollectionMock.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
            _templateConnectorMock.Invocations.Clear();
            _groupConnectorMock.Invocations.Clear();
            _userConnectorMock.Invocations.Clear();
            _documentCollectionConnectorMock.Invocations.Clear();
            _programConnectorMock.Invocations.Clear();
            _programUtilizationHistoryConnectorMock.Invocations.Clear();
            _managementPeriodicReportConnectorMock.Invocations.Clear();
            _managementPeriodicReportEmailConnectorMock.Invocations.Clear();
        }
        
        #region Read

        [Fact]
        public void Read_WithMultiParams_Success()
        {
            int totalCount;
            IEnumerable<ProgramUtilizationHistory> programUtilizationHistories = new List<ProgramUtilizationHistory>()
            {
                new ProgramUtilizationHistory()
                {
                    CompanyId = Guid.NewGuid(),
                    UpdateDate= new DateTime(2021,10,03)
                },
                new ProgramUtilizationHistory()
                {
                    CompanyId = Guid.NewGuid(),
                    UpdateDate= new DateTime(2021,10,03)
                },
            };

            _programUtilizationHistoryConnectorMock.Setup(x => x.
            Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), out totalCount, default)).Returns(programUtilizationHistories);

            var actual = _report.Read("", 0, 5, new DateTime(2020, 10, 3), new DateTime(2022, 10, 03), out totalCount);

            Assert.Equal(programUtilizationHistories.Count(), actual.Count());
        }

        [Fact]
        public void Read_WithMultiParams_ReturnEmptyList()
        {
            int totalCount;
            IEnumerable<ProgramUtilizationHistory> programUtilizationHistories = new List<ProgramUtilizationHistory>()
            {
                new ProgramUtilizationHistory()
                {
                    CompanyId = Guid.NewGuid(),
                    UpdateDate= new DateTime(2021,10,03)
                },
                new ProgramUtilizationHistory()
                {
                    CompanyId = Guid.NewGuid(),
                    UpdateDate= new DateTime(2021,10,03)
                },
            };

            _programUtilizationHistoryConnectorMock.Setup(x => x.
            Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), out totalCount, default)).Returns(programUtilizationHistories);

            var actual = _report.Read("", 0, 2, new DateTime(2020, 10, 03), new DateTime(2020, 10, 04), out totalCount);

            Assert.Empty(actual);
        }
        #endregion

        #region GetUtilizationReports

        [Fact]
        public async Task GetUtilizationReports_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUtilizationReports(null, 0, null, null, null, invalidOffset, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUtilizationReports_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -100;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUtilizationReports(null, 0, null, null, null, 0, invalidLimit));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUtilizationReports_Valid_ShouldSuccess()
        {
            // Arrange
            var programUtilization = new ProgramUtilization()
            {
                StartDate = DateTime.MinValue,
                SMS = 123,
                DocumentsUsage = Consts.UNLIMITED
            };
            var company = new Company() { Id = Guid.NewGuid(), ProgramUtilization = programUtilization };
            var programUtilizationHistory = new ProgramUtilizationHistory()
            {
                CompanyId = company.Id,
                UpdateDate = DateTime.UtcNow,
                Expired = DateTime.MaxValue
            };
            var programUtilizationHistories = new List<ProgramUtilizationHistory>() { programUtilizationHistory };
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            _programUtilizationHistoryConnectorMock.Setup(_ => _.Read(It.IsAny<bool?>(), It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<Guid>())).Returns(programUtilizationHistories);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);

            // Action
            (var reports, int totalCount) = await _report.GetUtilizationReports(null, 0, null, null, null, 0, 1);

            // Assert
            Assert.Equal(programUtilizationHistories.Count, totalCount);
        }


        #endregion

        #region GetUtilizationReportsByProgram
        [Fact]
        public async Task GetUtilizationReportsByProgram_WhenProgramIdInvalid_ShouldThwrowInvalidOperationException()
        {
            // Arrange
            Program program = new Program();

            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync((Program)null);

            // Assert
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUtilizationReportsByProgram(_sampleGuid, 12, 0, 0, 0, 20));
        }

        [Fact]
        public async Task GetUtilizationReportsByProgram_WhenProgramIsNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync((Program)null);

            // Assert
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUtilizationReportsByProgram(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));
        }
        #endregion

        #region GetUtilizationReportsByPercentage

        [Fact]
        public void GetUtilizationReportsByPercentage_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = Assert.Throws<InvalidOperationException>(() => _report.GetUtilizationReportsByPercentage(0, 0, invalidOffset, 0, out _));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public void GetUtilizationReportsByPercentage_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -1;

            // Action
            var actual = Assert.Throws<InvalidOperationException>(() => _report.GetUtilizationReportsByPercentage(0, 0, 0, invalidLimit, out _));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public void GetUtilizationReportsByPercentage_WhencCompaniesIsNull_ShouldReturnEmptyCollection()
        {
            // Arrange
            int totalCount;
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out totalCount)).Returns((IEnumerable<Company>)null);

            // Action
            var result = _report.GetAllCompaniesUtilizations(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), out totalCount);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetUtilizationReportsByPercentage_Valid_ShouldSuccess()
        {
            // Arrange
            var programUtilization = new ProgramUtilization() { StartDate = DateTime.MinValue };
            var company = new Company() { ProgramUtilization = programUtilization };
            var companies = new List<Company>() { company };
            var programUtilizationHistory = new ProgramUtilizationHistory() { Expired = DateTime.Now };
            var programUtilizationHistories = new List<ProgramUtilizationHistory>() { programUtilizationHistory };
            var companyUtilizationReports = new List<CompanyUtilizationReport>()
            {
                new CompanyUtilizationReport(programUtilizationHistories, programUtilization.StartDate, 0)
            };
            var companyUtilizationHistories = new List<ProgramUtilizationHistory>() { programUtilizationHistory };
            _programUtilizationHistoryConnectorMock.Setup(_ => _.Read(It.IsAny<bool?>(),
                It.IsAny<Guid?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<Guid>())).Returns(companyUtilizationHistories);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), out It.Ref<int>.IsAny)).Returns(companies);
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);

            // Actual
            _report.GetUtilizationReportsByPercentage(0, 0, 0, 1, out int total);

            // Assert
            Assert.Equal(companyUtilizationHistories.Count, total);
        }

        #endregion

        #region GetAllCompaniesUtilizations
        [Fact]
        public void GetAllCompaniesUtilizations_WhencCompaniesIsNull_ShouldReturnEmptyCollection()
        {
            // Arrange
            int totalCount;
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out totalCount)).Returns((IEnumerable<Company>)null);

            // Action
            var result = _report.GetAllCompaniesUtilizations(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), out totalCount);

            //Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetUtilizationReportPerGroup

        [Fact]
        public async Task GetUtilizationReportPerGroup_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUtilizationReportPerGroup(Guid.Empty, 0, 0, invalidOffset, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUtilizationReportPerGroup_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUtilizationReportPerGroup(Guid.Empty, 0, 0, 0, invalidLimit));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUtilizationReportPerGroup_CompanyNotExists_ThrowException()
        {
            // Arrange
            Company company = null;
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUtilizationReportPerGroup(Guid.Empty, 0, 0, 0, 0));

            // Assert
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUtilizationReportPerGroup_Valid_ShouldSuccess()
        {
            // Arrange
            var documentCollection = new DocumentCollection()
            {
                CreationTime = DateTime.MaxValue
            };
            var docs = new List<DocumentCollection>() { documentCollection };
            var programUtilization = new ProgramUtilization()
            {
                DocumentsLimit = 0
            };
            var company = new Company()
            {
                ProgramUtilization = programUtilization
            };
            var group = new Group()
            {
                Id = Guid.NewGuid(),
                Name = "group1"
            };
            var groups = new List<Group>() { group };
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).Returns(groups);
            _documentCollectionConnectorMock.Setup(_ => _.Read(It.IsAny<Group>())).Returns(docs);

            // Action
            (var reportsResponse, int totalCount) = await _report.GetUtilizationReportPerGroup(Guid.Empty, 0, 0, 0, 1);

            // Assert
            Assert.Equal(groups.Count, totalCount);
        }

        #endregion

        #region Programs

        #region GetProgramsReport

        [Fact]
        public void GetProgramsReport_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = Assert.Throws<InvalidOperationException>(() => _report.GetProgramsReport(0, 0, null, invalidOffset, 0, out _));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public void GetProgramsReport_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -1;

            // Action
            var actual = Assert.Throws<InvalidOperationException>(() => _report.GetProgramsReport(0, 0, null, 0, invalidLimit, out _));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public void GetProgramsReport_Valid_ShouldSuccess()
        {
            // Arrange
            var program = new Program();
            var programs = new List<Program>();
            _programConnectorMock.Setup(_ => _.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), out It.Ref<int>.IsAny)).Returns(programs);

            // Action
            var response = _report.GetProgramsReport(0, 0, null, 0, 1, out _);

            // Assert
            Assert.Equal(programs.Count, response.Count());
        }

        #endregion

        #endregion

        #region DocCount Per Group

        #region GetGroupDocumentReports

        [Fact]
        public async Task GetGroupDocumentReports_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetGroupDocumentReports(Guid.Empty, invalidOffset, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetGroupDocumentReports_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetGroupDocumentReports(Guid.Empty, 0, invalidLimit));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetGroupDocumentReports_CompanyNotExist_ThrowException()
        {
            // Arrange
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(default(Company));

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetGroupDocumentReports(Guid.Empty, 0, 1));

            // Assert
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetGroupDocumentReports_Valid_ShouldSuccess()
        {
            // Arrange
            var company = new Company();
            var group = new Group()
            {
                Name = "groupName"
            };
            var groups = new List<Group>() { group };
            var documentCollection = new DocumentCollection() { DocumentStatus = DocumentStatus.Created };
            var docCollections = new List<DocumentCollection>() { documentCollection };
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).Returns(groups);
            _documentCollectionConnectorMock.Setup(_ => _.Read(It.IsAny<Group>())).Returns(docCollections);

            // Action
            (var reportsResponse, int totalCount) = await _report.GetGroupDocumentReports(Guid.Empty, 0, 1);

            // Assert
            Assert.Equal(groups.Count, totalCount);
        }
        #endregion

        #endregion

        #region DocCount Per User/Contact

        #region GetUserDocumentReports

        [Fact]
        public async Task GetUserDocumentReports_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUserDocumentReports(Guid.Empty, null, false, null, null, invalidOffset, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUserDocumentReports_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUserDocumentReports(Guid.Empty, null, false, null, null, 0, invalidLimit));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUserDocumentReports_CompanyNotExist_ThrowException()
        {
            // Arrange
            Company company = null;
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUserDocumentReports(Guid.Empty, null, false, null, null, 0, 1));

            // Assert
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUserDocumentReports_InvalidGroupId_ThrowException()
        {
            // Arrange
            var company = new Company() { Id = Guid.NewGuid() };
            var groupIds = new List<Guid>() { Guid.NewGuid() };
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupConnectorMock.Setup(_ => _.ReadMany(It.IsAny<List<Group>>())).Returns(Enumerable.Empty<Group>);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUserDocumentReports(Guid.Empty, groupIds, false, null, null, 0, 1));

            // Assert
            Assert.Equal(ResultCode.InvalidGroupId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUserDocumentReports_ValidIsUser_ShouldSuccess()
        {
            // Arrange
            var company = new Company() { Id = Guid.NewGuid() };
            var group = new Group() { Id = Guid.NewGuid(), CompanyId = company.Id };
            var groups = new List<Group>() { group };
            var groupIds = new List<Guid>() { group.Id };
            var user = new User()
            {
                Id = Guid.NewGuid(),
                Name = "name"
            };
            var documentCollection = new DocumentCollection()
            {
                CreationTime = DateTime.UtcNow,
                User = user
            };
            var docCollections = new List<DocumentCollection>() { documentCollection };
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupConnectorMock.Setup(_ => _.ReadMany(It.IsAny<List<Group>>())).Returns(groups);
            _documentCollectionConnectorMock.Setup(_ => _.ReadByGroups(It.IsAny<List<Group>>())).Returns(docCollections);
            _userConnectorMock.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(user);

            // Action
            (var reports, int totalCount) = await _report.GetUserDocumentReports(Guid.Empty, groupIds, true, DateTime.MinValue, DateTime.MaxValue, 0, 1);

            // Assert
            Assert.Equal(docCollections.Count, totalCount);
        }

        [Fact]
        public async Task GetUserDocumentReports_ValidIsNotUser_ShouldSuccess()
        {
            // Arrange
            var company = new Company() { Id = Guid.NewGuid() };
            var group = new Group() { Id = Guid.NewGuid(), CompanyId = company.Id };
            var groups = new List<Group>() { group };
            var groupIds = new List<Guid>() { group.Id };
            var contact = new Contact() { Id = Guid.NewGuid() };
            var signer = new Signer()
            {
                Contact = contact
            };
            var documentCollection = new DocumentCollection()
            {
                CreationTime = DateTime.UtcNow,
                Signers = new List<Signer>() { signer }
            };
            var docCollections = new List<DocumentCollection>() { documentCollection };
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupConnectorMock.Setup(_ => _.ReadMany(It.IsAny<List<Group>>())).Returns(groups);
            _documentCollectionConnectorMock.Setup(_ => _.ReadByGroups(It.IsAny<List<Group>>())).Returns(docCollections);

            // Action
            (var reports, int totalCount) = await _report.GetUserDocumentReports(Guid.Empty, groupIds, false, DateTime.MinValue, DateTime.MaxValue, 0, 1);

            // Assert
            Assert.Equal(docCollections.Count, totalCount);
        }

        #endregion

        #endregion

        #region GetUsersByCompany

        [Fact]
        public async Task GetUsersByCompany_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsersByCompany(Guid.Empty, invalidOffset, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsersByCompany_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsersByCompany(Guid.Empty, 0, invalidLimit));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsersByCompany_CompanyNotExist_ThrowException()
        {
            // Arrange
            Company company = null;
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsersByCompany(Guid.Empty, 0, 1));

            // Assert
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsersByCompany_Valid_ShouldSuccess()
        {
            // Arrange
            var company = new Company();
            var group = new Group() { Name = "groupName" };
            var groups = new List<Group>();
            var user = new User()
            {
                Id = Guid.NewGuid(),
                Name = "userName",
                Email = "email"
            };
            var users = new List<User>() { user };
            var documentCollection = new DocumentCollection()
            {
                User = user
            };
            var docCollections = new List<DocumentCollection>();
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).Returns(groups);
            _documentCollectionConnectorMock.Setup(_ => _.Read(It.IsAny<Group>())).Returns(docCollections);
            _userConnectorMock.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(user);
            _userConnectorMock.Setup(_ => _.Read(It.IsAny<Group>())).Returns(users);

            // Action
            (var response, int totalCount) = await _report.GetUsersByCompany(Guid.Empty, 0, 1);

            // Assert
            Assert.Equal(groups.Count, totalCount);
        }

        #endregion

        #region GetFreeTrialUsers

        [Fact]
        public void GetFreeTrialUsers_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = Assert.Throws<InvalidOperationException>(() => _report.GetFreeTrialUsers(invalidOffset, 0, out _));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public void GetFreeTrialUsers_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -1;

            // Action
            var actual = Assert.Throws<InvalidOperationException>(() => _report.GetFreeTrialUsers(0, invalidLimit, out _));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public void GetFreeTrialUsers_Valid_ShouldSuccess()
        {
            // Arrange
            var programUtilization = new ProgramUtilization()
            {
                DocumentsUsage = 0,
                SMS = 1,
                Templates = 1,
                Expired = DateTime.MaxValue
            };
            var user = new User()
            {
                Name = "name",
                Email = "email",
                Username = "username",
                CreationTime = DateTime.Now,
                ProgramUtilization = programUtilization
            };
            var users = new List<User>() { user };
            _userConnectorMock.Setup(_ => _.ReadFreeTrialUsers()).Returns(users);

            // Action
            var response = _report.GetFreeTrialUsers(0, 1, out int totalCount);

            // Assert
            Assert.Equal(users.Count, response.Count());
        }

        #endregion

        #region GetUsageByUsers

        [Fact]
        public async Task GetUsageByUsers_InvalidInput_ThrowException()
        {
            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsageByUsers(null, Guid.Empty, null, null, null, 0, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsageByUsers_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsageByUsers(null, Guid.NewGuid(), null, null, null, invalidOffset, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsageByUsers_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsageByUsers(null, Guid.NewGuid(), null, null, null, 0, invalidLimit));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsageByUsers_Valid_ShouldSuccess()
        {
            // Arrange
            var group = new Group() { Id = Guid.NewGuid(), Name = "groupname" };
            var report = new UsageByUserReport() { GroupId = group.Id };
            var reports = new List<UsageByUserReport>() { report };
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            _documentCollectionConnectorMock.Setup(_ => _.ReadUsageByUserDetails(It.IsAny<string>(), It.IsAny<Company>(), It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(reports);
            _groupConnectorMock.Setup(_ => _.Read(It.IsAny<Group>())).ReturnsAsync(group);

            // Action
            (var reportsResponse, int _) = await _report.GetUsageByUsers(null, Guid.NewGuid(), null, null, null, 0, 1);

            // Assert
            Assert.Equal(reports.Count, reportsResponse.Count());
        }

        #endregion

        #region GetUsageByCompanies

        [Fact]
        public async Task GetUsageByCompanies_CompanyNotExist_ThrowException()
        {
            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsageByCompanies(Guid.Empty, null, null, null, 0, 0));

            // Assert
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsageByCompanies_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsageByCompanies(Guid.NewGuid(), null, null, null, invalidOffset, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsageByCompanies_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsageByCompanies(Guid.NewGuid(), null, null, null, 0, invalidLimit));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsageByCompanies_Valid_ShouldSuccess()
        {
            // Arrange
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            var group = new Group() { Id =  Guid.NewGuid() };
            var report = new UsageByCompanyReport()
            {
                GroupId = group.Id
            };
            var reports = new List<UsageByCompanyReport>() { report };
            _documentCollectionConnectorMock.Setup(_ => _.ReadUsageByCompanyAndGroups(It.IsAny<Company>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateTime>(), 
                It.IsAny<DateTime>())).Returns(reports);  
            _groupConnectorMock.Setup(_ => _.Read(It.IsAny<Group>())).ReturnsAsync(group);
            
            // Action
            (var response, int totalCount) = await _report.GetUsageByCompanies(Guid.NewGuid(), null, null, null, 0, 1);

            // Assert
            Assert.Equal(reports.Count, response.Count());
        }

        #endregion

        #region GetTemplatesByUsage

        [Fact]
        public async Task GetTemplatesByUsage_CompanyNotExist_ThrowException()
        {
            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetTemplatesByUsage(Guid.Empty, null, null, null, 0, 0));

            // Assert
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetTemplatesByUsage_InvalidOffset_ThrowException()
        {
            // Arrange
            const int invalidOffset = -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetTemplatesByUsage(Guid.NewGuid(), null, null, null, invalidOffset, 0));

            // Assert
            Assert.Equal(ResultCode.InvalidPositiveOffsetNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetTemplatesByUsage_InvalidLimit_ThrowException()
        {
            // Arrange
            const int invalidLimit= -1;

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetTemplatesByUsage(Guid.NewGuid(), null, null, null, 0, invalidLimit));

            // Assert
            Assert.Equal(ResultCode.InvalidLimitNumber.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetTemplatesByUsage_Valid_ShouldSuccess()
        {
            // Arrange
            var company = new Company() { Id = Guid.NewGuid(), Name = "company_name" };
            var group = new Group() { Id = Guid.NewGuid() };
            var report = new TemplatesByUsageReport() { GroupId = group.Id };
            var reports = new List<TemplatesByUsageReport>() { report };
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _templateConnectorMock.Setup(_ => _.ReadTemplatesByUsage(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<int>(), It.IsAny<int>(), out It.Ref<int>.IsAny)).Returns(reports);
            _groupConnectorMock.Setup(_ => _.Read(It.IsAny<Group>())).ReturnsAsync(group);

            // Action
            (var response, int totalCount) = await _report.GetTemplatesByUsage(Guid.NewGuid(), null, null, null, 0, 1);

            // Assert
            Assert.Equal(reports.Count, response.Count);
        }

        #endregion

        #region GetUsageBySignatureType

        [Fact]
        public async Task GetUsageBySignatureType_CompanyNotExist_ThrowException()
        {
            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetUsageBySignatureType(Guid.Empty, null, null, null));

            // Assert
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetUsageBySignatureType_Valid_ShouldSuccess()
        {
            // Arrange
            var report = new UsageBySignatureTypeReport() { CompanyName = "companyName" };
            _daterMock.Setup(_ => _.UtcNow()).Returns(DateTime.Now);
            _documentCollectionConnectorMock.Setup(_ => _.ReadUsageByCompanyAndSignatureTypes(It.IsAny<Company>(), It.IsAny<IEnumerable<SignatureFieldType>>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(report);

            // Action
            var response = await _report.GetUsageBySignatureType(Guid.NewGuid(), null, null, null);

            // Assert
            Assert.Equal(report.CompanyName, response.CompanyName);
        }

        #endregion

        #region GetGroupsByCompany

        [Fact]
        public async Task GetGroupsByCompany_CompanyNotExist_ThrowException()
        {
            // Arrange
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(default(Company));

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.GetGroupsByCompany(Guid.Empty));

            // Assert
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task GetGroupsByCompany_Valid_ShouldSuccess()
        {
            // Arrange
            var company = new Company();
            var group = new Group();
            var groups = new List<Group>() { group };
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).Returns(groups);

            // Action
            (var response, int totalCount) = await _report.GetGroupsByCompany(Guid.Empty);

            // Assert
            Assert.Equal(groups.Count, totalCount);
        }

        #endregion

        #region CreateFrequencyReport

        [Fact]
        public async Task CreateFrequencyReport_UserNotExist_ThrowException()
        {
            // Arrange
            User user = null;
            _users.Setup(_ => _.GetCurrentUser()).ReturnsAsync(user);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.CreateFrequencyReport(null, ManagementReportFrequency.Weekly, ManagementReportType.CompanyUsers, null));

            // Assert
            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateFrequencyReport_EmailsRequiredToManagementPeriodicReports_ThrowException()
        {
            // Arrange
            User user = new User();
            _users.Setup(_ => _.GetCurrentUser()).ReturnsAsync(user);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _report.CreateFrequencyReport(null, ManagementReportFrequency.Weekly, ManagementReportType.CompanyUsers, null));

            // Assert
            Assert.Equal(ResultCode.EmailsRequiredToManagementPeriodicReports.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateFrequencyReport_Valid_ShouldSuccess()
        {
            // Arrange
            User user = new User();
            ReportParameters reportParameters = new ReportParameters();
            List<string> emails = new List<string>() { "test@test.com" };
            _users.Setup(_ => _.GetCurrentUser()).ReturnsAsync(user);
            _managementPeriodicReportConnectorMock.Setup(_ => _.Create(It.IsAny<ManagementPeriodicReport>()));
            _managementPeriodicReportEmailConnectorMock.Setup(_ => _.Create(It.IsAny<ManagementPeriodicReportEmail>()));
            // Action
            await _report.CreateFrequencyReport(null, ManagementReportFrequency.Weekly, ManagementReportType.CompanyUsers, emails);

            // Assert
            _managementPeriodicReportConnectorMock.Verify(_ => _.Create(It.IsAny<ManagementPeriodicReport>()), Times.Once);
            _managementPeriodicReportEmailConnectorMock.Verify(_ => _.Create(It.IsAny<ManagementPeriodicReportEmail>()),Times.Once);
        }

        #endregion
    }
}
