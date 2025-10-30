using Comda.License.DAL;
using Common.Consts;
using Common.Enums;
using Common.Enums.Companies;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.License;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.License;
using Common.Models.Programs;
using ManagementBL.Handlers;
using ManagementBL.Tests.TestClass;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using IConfiguration = Common.Interfaces.IConfiguration;

namespace ManagementBL.Tests.Handlers
{
    public class CompaniesHandlerTests
    {        
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IEmail> _email;
        private readonly Mock<IOneTimeTokens> _oneTimeTokens;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IEncryptor> _encryptor;
        private readonly Mock<IDater> _dater;
        private readonly Mock<IFileSystem> _fileSystem;
        private readonly Mock<ILicense> _license;
        private readonly Mock<IActiveDirectoryGroupsConnector> _activeDirectoryGroups;
        private readonly Mock<IWeSignLicense> _wesignLicense;
        private Mock<Common.Interfaces.ManagementApp.IUsers> _users;
        private readonly Mock<IValidator> _validator;
        private CompaniesHandler _companiesHandler;
        private readonly int _zero = 0;
        private readonly string _fixedstring = "fixed";
        private Guid _oneGuid = new Guid("00000000-0000-0000-0000-000000000001");
        private Guid _twoGuid = new Guid("00000000-0000-0000-0000-000000000002");
        private Guid _threeGuid = new Guid("00000000-0000-0000-0000-000000000003");
        private Guid _fourGuid = new Guid("00000000-0000-0000-0000-000000000004");
        private Guid _fiveGuid = new Guid("00000000-0000-0000-0000-000000000005");
        private Guid _sixGuid = new Guid("00000000-0000-0000-0000-000000000006");

        private readonly Mock<ICompanyConnector> _companyConnector;
        private readonly Mock<IUserConnector> _userConnector;
        private readonly Mock<IProgramConnector> _programConnector;
        private readonly Mock<IContactConnector> _contactConnector;
        private readonly Mock<IGroupConnector> _groupConnector;
        private readonly Mock<ITemplateConnector> _templateConnector;
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnector;
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnector;
        private readonly Mock<IProgramUtilizationHistoryConnector> _programUtilizationHistoryConnector;

        public CompaniesHandlerTests()
        {
            _companyConnector = new Mock<ICompanyConnector>();
            _userConnector = new Mock<IUserConnector>();
            _programConnector = new Mock<IProgramConnector>();
            _contactConnector = new Mock<IContactConnector>();
            _groupConnector = new Mock<IGroupConnector>();
            _templateConnector = new Mock<ITemplateConnector>();
            _documentCollectionConnector = new Mock<IDocumentCollectionConnector>();
            _programUtilizationConnector = new Mock<IProgramUtilizationConnector>();
            _programUtilizationHistoryConnector = new Mock<IProgramUtilizationHistoryConnector>();
            _configuration = new Mock<IConfiguration>();
            _email = new Mock<IEmail>();
            _oneTimeTokens = new Mock<IOneTimeTokens>();
            _logger = new Mock<ILogger>();
            _encryptor = new Mock<IEncryptor>();
            _dater = new Mock<IDater>();
            _license = new Mock<ILicense>();
            _fileSystem = new Mock<IFileSystem>();
            _activeDirectoryGroups = new Mock<IActiveDirectoryGroupsConnector>();
            _wesignLicense = new Mock<IWeSignLicense>();
            _users = new Mock<Common.Interfaces.ManagementApp.IUsers>();
            _validator = new Mock<IValidator>();

            _companiesHandler = new CompaniesHandler(_companyConnector.Object, _userConnector.Object, _programConnector.Object,_groupConnector.Object,_programUtilizationConnector.Object,
                _programUtilizationHistoryConnector.Object, _templateConnector.Object,_contactConnector.Object, _documentCollectionConnector.Object, _configuration.Object, _email.Object,
                _oneTimeTokens.Object, _logger.Object, _encryptor.Object, _dater.Object, _license.Object, 
                _activeDirectoryGroups.Object, _users.Object, _validator.Object);

        }


        [Fact]
        public async Task Read_MultiParamsOverload_NoComanyExist_ReturnsEmptyList()
        {
            int zero;
            _companyConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out zero)).Returns(new List<Company>());
            int expectedListCountResult = 0;

            int testPara;
            (var result, testPara) =await _companiesHandler.Read("", 0, 0);

            Assert.Equal(result.Count(), expectedListCountResult);
            Assert.Equal(testPara, expectedListCountResult);

        }
        [Fact]
        public async Task Read_MultiParamsOverload_OneCompany_ReturnsOneCompanyInList()
        {
            int zero;
            _companyConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out zero)).Returns(new List<Company>() { new Company() });
            _programConnector.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(new Program() { Name = "AAA" });
            _userConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserStatus>(), out zero, It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(new List<User>());

            int expectedListCountResult = 1;

            int testPara;
            (var result, testPara) = await _companiesHandler.Read("", 0, 0);

            Assert.Equal(result.Count(), expectedListCountResult);
            

        }
        [Fact]
        public async Task Read_MultiParamsOverload_OneCompanyProgramUtilizationNotNull_ReturnsOneCompanyInList()
        {
            int zero;
            _companyConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out zero)).Returns(new List<Company>() { new Company() {
                ProgramUtilization = new ProgramUtilization(){}
            }
            });
            DateTime dateTime = DateTime.Now;
            _programUtilizationConnector.Setup(x => x.Read(It.IsAny<ProgramUtilization>())).ReturnsAsync(new ProgramUtilization() { Expired = dateTime });
            _programConnector.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(new Program() { Name = "AAA" });
            _userConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserStatus>(), out zero, It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(new List<User>());

            int expectedListCountResult = 1;

            int testPara;
            (var result, testPara) = await _companiesHandler.Read("", 0, 0);
            Assert.Equal(result.ToList()[0].ExipredTime, dateTime);
            Assert.Equal(result.Count(), expectedListCountResult);
            

        }
        [Fact]
        public async Task Read_MultiParamsOverload_multiComanyExist_ReturnsMoreThenOneCompanyInList()
        {
            int zero;
            _companyConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out zero)).Returns(new List<Company>()
                { new Company(), new Company(), new Company() });
            _programConnector.Setup(x => x.Read(It.IsAny<Program>()));
            _userConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserStatus>(), out zero, It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(new List<User>());
            int expectedListCountResult = 3;

            int testPara;
            (var result, testPara) = await _companiesHandler.Read(Consts.EMPTY, 0, 0);

            Assert.Equal(result.Count(), expectedListCountResult);
            

        }

        [Fact]
        public async Task Read_SendNullCompanyParameter_ThrowException()
        {


            var actual = await Assert.ThrowsAsync<Exception>(() => _companiesHandler.Read(null, new User()));

            Assert.Equal("Null input - user or company is null", actual.Message);

        }
        [Fact]
        public async Task Read_SendNullUserParameter_ThrowException()
        {

            var actual = await Assert.ThrowsAsync<Exception>(() => _companiesHandler.Read(new Company(), null));

            Assert.Equal("Null input - user or company is null", actual.Message);

        }
        [Fact]
        public async Task Read_SendNullUserAndCompanyParameter_ThrowException()
        {

            var actual = await Assert.ThrowsAsync<Exception>(() => _companiesHandler.Read(null, null));

            Assert.Equal("Null input - user or company is null", actual.Message);

        }

        [Fact]
        public async Task Read_NoCompanyFound_ReturnNullSucsses()
        {
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>()));

            var result = await _companiesHandler.Read(new Company(), new User());

            Assert.Null(result);



        }

        [Fact]
        public async Task Read_ReturnCompanyNoGroups_Sucsses()
        {
            string name = "name";
            string companyLogo = "CONF LOGO";
            LicenseCounters aaa = null;

            var licenceObj = new WesignTestClassLicenseResultModel
            {
                LicenseCounters = new LicenseCounters() { UseActiveDirectory = false }
            };

            _license.Setup(x => x.GetLicenseInformationAndUsing(It.IsAny<bool>())).ReturnsAsync((licenceObj,aaa));
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Name = name });
            _configuration.Setup(x => x.GetCompanyLogo(It.IsAny<Guid>())).Returns(companyLogo);
            _groupConnector.Setup(x => x.Read(It.IsAny<Company>()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));

            var item = await _companiesHandler.Read(new Company(), new User());

            Assert.Equal(item.Name, name);
            Assert.Equal(item.Groups.Count(), _zero);
            Assert.Equal(item.CompanyConfiguration.Base64Logo, companyLogo);


        }

        [Fact]
        public async Task Read_ReturnCompaniesMultiGroups_Sucsses()
        {
            string name = "name";
            int numberOfGorups = 2;
            List<Group> groups = new List<Group>();
            LicenseCounters aaa = null;

            for (int i = 0; i < numberOfGorups; ++i)
            {
                groups.Add(new Group());
            }
            var licenceObj = new WesignTestClassLicenseResultModel();
            licenceObj.LicenseCounters = new LicenseCounters() { UseActiveDirectory = false };
            
            _license.Setup(x => x.GetLicenseInformationAndUsing(It.IsAny<bool>())).ReturnsAsync((licenceObj, aaa));
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Name = name });
            _configuration.Setup(x => x.GetCompanyLogo(It.IsAny<Guid>())).Returns("CONF LOGO");
            _groupConnector.Setup(x => x.Read(It.IsAny<Company>())).Returns(groups);
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));
           
            var item = await _companiesHandler.Read(new Company(), new User());

            Assert.Equal(item.Name, name);
            Assert.Equal(item.Groups.Count(), numberOfGorups);


        }

        [Fact]
        public async Task Read_GetCompanyLogoFromComapnyConfigurationCompanyConfigurationIsNotNull_Sucsses()
        {
            string name = "name";
            int numberOfGorups = 1;
            List<Group> groups = new List<Group>();
            LicenseCounters aaa = null;

            for (int i = 0; i < numberOfGorups; ++i)
            {
                groups.Add(new Group());
            }
            string companyLogoBase64 = "companyLogoBase64";
            var licenceObj = new WesignTestClassLicenseResultModel();
            licenceObj.LicenseCounters = new LicenseCounters() { UseActiveDirectory = false };

            _license.Setup(x => x.GetLicenseInformationAndUsing(It.IsAny<bool>())).ReturnsAsync((licenceObj, aaa));
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Name = name, CompanyConfiguration = new CompanyConfiguration() });
            _configuration.Setup(x => x.GetCompanyLogo(It.IsAny<Guid>())).Returns(companyLogoBase64);
            _groupConnector.Setup(x => x.Read(It.IsAny<Company>())).Returns(groups);
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));

            var item = await _companiesHandler.Read(new Company(), new User());

            Assert.Equal(item.CompanyConfiguration.Base64Logo, companyLogoBase64);
        }

        [Fact]
        public async Task Read_GetCompanyEmailTemplateComapnyConfigurationIsNotNull_Sucsses()
        {
            string name = "name";
            int numberOfGorups = 1;
            List<Group> groups = new List<Group>();
            LicenseCounters aaa = null;
            var licenceObj = new WesignTestClassLicenseResultModel();


            for (int i = 0; i < numberOfGorups; ++i)
            {
                groups.Add(new Group());
            }
            
            licenceObj.LicenseCounters = new LicenseCounters() { UseActiveDirectory = false };
            string companyEmailTemplateBase64 = "companyEmailTemplateBase64";

            _license.Setup(x => x.GetLicenseInformationAndUsing(It.IsAny<bool>())).ReturnsAsync((licenceObj, aaa));
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Name = name, CompanyConfiguration = new CompanyConfiguration() });
            _configuration.Setup(x => x.GetCompanyEmailHtml(It.IsAny<Guid>(), It.IsAny<MessageType>())).Returns(companyEmailTemplateBase64);
            _groupConnector.Setup(x => x.Read(It.IsAny<Company>())).Returns(groups);
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));

            var item = await _companiesHandler.Read(new Company(), new User());

            Assert.Equal(item.CompanyConfiguration.EmailTemplates.BeforeSigningBase64String, companyEmailTemplateBase64);
        }

        [Fact]
        public async Task Read_GetCompanyEmailTemplateComapnyConfiguratioNotNull_Sucsses()
        {
            string name = "name";
            int numberOfGorups = 1;
            LicenseCounters aaa = null;
            List<Group> groups = new List<Group>();
            var licenceObj = new WesignTestClassLicenseResultModel();
            licenceObj.LicenseCounters = new LicenseCounters() { UseActiveDirectory = false };
            _license.Setup(x => x.GetLicenseInformationAndUsing(It.IsAny<bool>())).ReturnsAsync((licenceObj, aaa));
            for (int i = 0; i < numberOfGorups; ++i)
            {
                groups.Add(new Group() { GroupStatus = Common.Enums.Groups.GroupStatus.Created });
            }
            
            string companyEmailTemplateBase64 = "companyEmailTemplateBase64";


            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Name = name });
            _configuration.Setup(x => x.GetCompanyEmailHtml(It.IsAny<Guid>(), It.IsAny<MessageType>())).Returns(companyEmailTemplateBase64);
            _groupConnector.Setup(x => x.Read(It.IsAny<Company>())).Returns(groups);
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));
           
            var item = await _companiesHandler.Read(new Company(), new User());

            Assert.Equal(item.CompanyConfiguration.EmailTemplates.BeforeSigningBase64String, companyEmailTemplateBase64);
        }


        [Fact]
        public async Task Create_CompanyIsNull_ThrowException()
        {
            var actual = await Assert.ThrowsAsync<Exception>(() => _companiesHandler.Create(null, new Group(), new User()));

            Assert.Equal("Null input - user or company or group is null", actual.Message);


        }

        [Fact]
        public async Task Create_GroupIsNull_ThrowException()
        {
            var actual = await Assert.ThrowsAsync<Exception>(() => _companiesHandler.Create(new Company(), null, new User()));

            Assert.Equal("Null input - user or company or group is null", actual.Message);
        }

        [Fact]
        public async Task Create_UserIsNull_ThrowException()
        {
            var actual = await Assert.ThrowsAsync<Exception>(() => _companiesHandler.Create(new Company(), new Group(), null));

            Assert.Equal("Null input - user or company or group is null", actual.Message);
        }

        [Fact]
        public async Task Create_CompanyExist_ThrowException()
        {
            int totalCount;
            string companyName = "companyName";

            _companyConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), null, out totalCount)).Returns(
                new List<Company>() { new Company() { Name = companyName , Status = CompanyStatus.Created} }
                );

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _companiesHandler.Create(new Company() { Name = companyName }, new Group(), new User()));

            Assert.Equal(ResultCode.CompanyAlreadyExists.GetNumericString(), actual.Message);

        }


        [Fact]
        public async Task Create_GroupExist_ThrowException()
        {
            int totalCount;
            string companyName = "companyName";

            _companyConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out totalCount)).Returns(
                new List<Company>() { new Company() { Name = companyName } }
                );
            _programUtilizationConnector.Setup(x => x.Create(new ProgramUtilization()));
            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.Now);
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User());
            _groupConnector.Setup(x => x.Create(It.IsAny<Group>())).Throws(new InvalidOperationException(ResultCode.GroupAlreadyExistInCompany.GetNumericString()));
            _programConnector.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);
            _users.Setup(x => x.GetCurrentUser()).ReturnsAsync(new User { Email = "sysAdmin@comda.co.il" });

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _companiesHandler.Create(new Company(), new Group(), new User()));

            Assert.Equal(ResultCode.GroupAlreadyExistInCompany.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task Create_CreateNewCompanyUserFreeTrile_Sucsses()
        {
            int totalCount;
            string companyName = "companyName";

            _companyConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out totalCount)).Returns(
                new List<Company>() { }
                );
            _programUtilizationConnector.Setup(x => x.Create(new ProgramUtilization()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User());
            _programConnector.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);
            _groupConnector.Setup(x => x.Create(It.IsAny<Group>()));
            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.Now);
            _userConnector.Setup(x => x.Create(It.IsAny<User>()));
            _users.Setup(x => x.GetCurrentUser()).ReturnsAsync(new User { Email = "testAdminUser@comda.co.il" });
            Company company = new Company()
            {
                Name = companyName,
                Id = _oneGuid
            };
            Group group = new Group()
            {

                Id = _twoGuid
            };
            User user = new User();



          await  _companiesHandler.Create(company, group, user);


            Assert.Equal(UserType.CompanyAdmin, user.Type);
            Assert.Equal(group.Id, user.GroupId);
            Assert.Equal(company.Id, group.CompanyId);

        }

        [Fact]
        public async Task Create_CreateNewCompanyUserWithMessageProvidersEncription_Sucsses()
        {
            string companyName = "companyName";
            int totalCount;

            _companyConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out totalCount)).Returns(
                new List<Company>() { }
                );
            _programUtilizationConnector.Setup(x => x.Create(new ProgramUtilization()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User());
            _programConnector.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);
            _groupConnector.Setup(x => x.Create(It.IsAny<Group>()));
            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.Now);
            _userConnector.Setup(x => x.Create(It.IsAny<User>()));
            _encryptor.Setup(x => x.Encrypt(It.IsAny<string>())).Returns(_fixedstring);
            _users.Setup(x => x.GetCurrentUser()).ReturnsAsync(new User { Email = "sysAdmin@comda.co.il" });
            CompanyConfiguration companyConfigur = new CompanyConfiguration()
            {
                Base64Logo = "123456",
                EmailTemplates = new EmailHtmlBodyTemplates
                {
                    BeforeSigningBase64String = "123456",
                },
                MessageProviders = new List<MessageProvider>()
                {
                    {
                        new MessageProvider(){
                        Password = "123"
                        }

                    },
                    {
                        new MessageProvider(){
                        Password = "456"
                        }
                    }
                }
            };

            Company company = new Company()
            {
                Name = companyName,
                Id = _oneGuid,
                CompanyConfiguration = companyConfigur
            };
            Group group = new Group()
            {

                Id = _twoGuid
            };
            User user = new User();

            await _companiesHandler.Create(company, group, user);

            Assert.Equal(UserType.CompanyAdmin, user.Type);
            Assert.Equal(group.Id, user.GroupId);
            Assert.Equal(company.Id, group.CompanyId);
            Assert.Equal(2, company.CompanyConfiguration.MessageProviders.Count() );
            Assert.Equal(company.CompanyConfiguration.MessageProviders.ToList()[0].Password, _fixedstring);
            Assert.Equal(company.CompanyConfiguration.MessageProviders.ToList()[1].Password, _fixedstring);
        }

        [Fact]
        public async Task Create_CompanyExistStatusDeleted_Sucssecs()
        {
            int totalCount;
            string companyName = "companyName";

            _companyConnector.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CompanyStatus>(), out totalCount)).Returns(
                new List<Company>() { new Company() { Name = companyName, Status = Common.Enums.Companies.CompanyStatus.Deleted } }
                );
            _programUtilizationConnector.Setup(x => x.Create(new ProgramUtilization()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User());
            _programConnector.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);
            _groupConnector.Setup(x => x.Create(It.IsAny<Group>()));
            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.Now);
            _userConnector.Setup(x => x.Create(It.IsAny<User>()));
            _encryptor.Setup(x => x.Encrypt(It.IsAny<string>())).Returns(_fixedstring);
            _users.Setup(x => x.GetCurrentUser()).ReturnsAsync(new User { Email = "sysAdmin@comda.co.il" });
            Company company = new Company()
            {
                Name = companyName,
                Id = _oneGuid

            };
            Group group = new Group()
            {

                Id = _twoGuid
            };
            User user = new User();
           await _companiesHandler.Create(company, group, user);


            Assert.Equal(UserType.CompanyAdmin, user.Type);
            Assert.Equal(group.Id, user.GroupId);
            Assert.Equal(company.Id, group.CompanyId);


        }

        [Fact]
        public async Task Update_CompanyNullParameter_ThrowException()
        {
            var actual = await Assert.ThrowsAsync<Exception>(() => _companiesHandler.Update(null, null, null));
            Assert.Equal("Null input - user or company or group is null", actual.Message);

        }

        [Fact]
        public async Task Update_CompanyNotExist_ThrowException()
        {
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>()));
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _companiesHandler.Update(new Company(), null, null));
            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task Update_CompanyExistDiffrentId_ThrowException()
        {
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = _oneGuid });

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _companiesHandler.Update(new Company() { Id = _twoGuid }, null, null));

            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        private Company GetMockCompany()
        {
            return new Company()
            {
                Id = _oneGuid,
                ProgramId = _oneGuid,
                ProgramUtilization = new ProgramUtilization()
                {
                    Users = 3,
                    DocumentsUsage = 3,
                    Templates = 3,
                    SMS = 3,
                    VisualIdentifications = 3,
                },

            };
        }
        [Fact]
        public async Task Update_CompanyResetProgramUtilizationInDB_Succses()
        {
            Company mockDBCompany = GetMockCompany();
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(mockDBCompany);
            _programUtilizationHistoryConnector.Setup(x => x.Create(It.IsAny<ProgramUtilizationHistory>()));
            _userConnector.Setup(x => x.Read(It.IsAny<Group>()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));
            _programConnector.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);

            await _companiesHandler.Update(
                new Company()
                {
                    Id = _oneGuid,
                    ProgramId = _twoGuid
                }, new Group(), new User());

            Assert.Equal(_zero, mockDBCompany.ProgramUtilization.VisualIdentifications);
            Assert.Equal(_zero, mockDBCompany.ProgramUtilization.SMS);
            Assert.Equal(_zero, mockDBCompany.ProgramUtilization.DocumentsUsage);
            Assert.Equal(_zero, mockDBCompany.ProgramUtilization.Templates);
            Assert.Equal(_zero, mockDBCompany.ProgramUtilization.SMS);

        }

        [Fact]
        public async Task Update_CompanyUpdateExperationDateInDB_Succses()
        {
            Company mockDBCompany = GetMockCompany();
            DateTime expired = DateTime.UtcNow;
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(mockDBCompany);
            _programUtilizationHistoryConnector.Setup(x => x.Create(It.IsAny<ProgramUtilizationHistory>()));
            _userConnector.Setup(x => x.Read(It.IsAny<Group>()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));
            _programConnector.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);

            await _companiesHandler.Update(
                new Company()
                {
                    Id = _oneGuid,
                    ProgramId = _twoGuid,
                    ProgramUtilization = new ProgramUtilization() { Expired = expired }

                }, new Group(), new User());


            Assert.Equal(expired, mockDBCompany.ProgramUtilization.Expired);


        }

        [Fact]
        public async Task Update_CompanyUpdateMessageProviderPassword_Succses()
        {

            Company mockDBCompany = GetMockCompany();
            mockDBCompany.CompanyConfiguration = new CompanyConfiguration();
            mockDBCompany.CompanyConfiguration.MessageProviders = new List<MessageProvider>()
            {
                { new MessageProvider(){
                    Password = _fixedstring + "123123"
                }
                }

            };
            DateTime expired = DateTime.Now;
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(mockDBCompany);
            _encryptor.Setup(x => x.Encrypt(It.IsAny<string>())).Returns(_fixedstring);
            _programUtilizationHistoryConnector.Setup(x => x.Create(It.IsAny<ProgramUtilizationHistory>()));
            _userConnector.Setup(x => x.Read(It.IsAny<Group>()));
            _programConnector.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));
            Company sentCompany =
                new Company()
                {
                    Id = _oneGuid,
                    ProgramId = _twoGuid,
                    ProgramUtilization = new ProgramUtilization() { Expired = expired },
                    CompanyConfiguration = new CompanyConfiguration()
                    {
                        MessageProviders = new List<MessageProvider>()
                        {
                            { new MessageProvider(){
                                Password = "1"
                            } }
                        }
                    }


                };

            await _companiesHandler.Update(sentCompany
                , new Group(), new User());


            Assert.Equal(_fixedstring, mockDBCompany.CompanyConfiguration.MessageProviders.ToList()[0].Password);
            Assert.Equal(_fixedstring, sentCompany.CompanyConfiguration.MessageProviders.ToList()[0].Password);


        }

        [Fact]
        public async Task Update_GroupReadRerutnNullCreateNewGroup_Succses()
        {
            Company mockDBCompany = GetMockCompany();
            Group group = new Group();
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(mockDBCompany);
            _programUtilizationHistoryConnector.Setup(x => x.Create(It.IsAny<ProgramUtilizationHistory>()));
            _userConnector.Setup(x => x.Read(It.IsAny<Group>()));
            _groupConnector.Setup(x => x.Create(group)).Callback(() => { group.Id = _threeGuid; });
            _encryptor.Setup(x => x.Encrypt(It.IsAny<string>())).Returns(_fixedstring);
            _userConnector.Setup(x => x.Read(It.IsAny<Group>()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));
            _programConnector.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);
            Company sentCompany =
                new Company()
                {
                    Id = _oneGuid,
                    ProgramId = _twoGuid
                };


            await _companiesHandler.Update(sentCompany
                , group, new User());


            Assert.Equal(_threeGuid.ToString(), group.Id.ToString());

        }

        [Fact]
        public async Task Update_GroupReadRerutnDiffrentGroupCreateNewGroup_Succses()
        {
            Company mockDBCompany = GetMockCompany();
            Group group = new Group();
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(mockDBCompany);
            _programUtilizationHistoryConnector.Setup(x => x.Create(It.IsAny<ProgramUtilizationHistory>()));
            _groupConnector.Setup(x => x.Read(It.IsAny<Group>())).ReturnsAsync(new Group()
            {
                CompanyId = _fiveGuid
            });
            _groupConnector.Setup(x => x.Create(group)).Callback(() => { group.Id = _threeGuid; });
            _encryptor.Setup(x => x.Encrypt(It.IsAny<string>())).Returns(_fixedstring);
            _programConnector.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);

            _userConnector.Setup(x => x.Read(It.IsAny<User>()));
            Company sentCompany =
                new Company()
                {
                    Id = _oneGuid,
                    ProgramId = _twoGuid
                };


            await _companiesHandler.Update(sentCompany
                , group, new User());


            Assert.Equal(_threeGuid.ToString(), group.Id.ToString());

        }


        [Fact]
        public async Task Update_GroupReadExistUpdateCompanyId_Succses()
        {
            Company mockDBCompany = GetMockCompany();
            Group group = new Group() { CompanyId = _fiveGuid };
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(mockDBCompany);
            _groupConnector.Setup(x => x.Read(It.IsAny<Group>())).ReturnsAsync(new Group()
            {
                Id = _oneGuid,
                CompanyId = _fiveGuid
            });

            _encryptor.Setup(x => x.Encrypt(It.IsAny<string>())).Returns(_fixedstring);
            _programConnector.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);
            _programUtilizationHistoryConnector.Setup(x => x.Create(It.IsAny<ProgramUtilizationHistory>()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));
            Company sentCompany =
                new Company()
                {
                    Id = _oneGuid,
                    ProgramId = _twoGuid
                };


           await _companiesHandler.Update(sentCompany
                , group, new User());


            Assert.Equal(_oneGuid.ToString(), group.Id.ToString());

        }


        [Fact]
        public async Task Update_CreateCompanyUserAdminFromUserFreeTrailButTheUserIsNotFreeTrail_ThrowException()
        {
            Company mockDBCompany = GetMockCompany();

            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(mockDBCompany);
            _groupConnector.Setup(x => x.Read(It.IsAny<User>()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User()
                )
                ;
            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);
            _programConnector.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);
            _programConnector.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);
            _programUtilizationHistoryConnector.Setup(x => x.Create(It.IsAny<ProgramUtilizationHistory>()));
            Company sentCompany =
                new Company()
                {
                    Id = _oneGuid,
                    ProgramId = _twoGuid
                };
            User sentUser = new User();

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _companiesHandler.Update(sentCompany, new Group(), sentUser));

            Assert.Equal(ResultCode.EmailBelongToOtherCompany.GetNumericString(), actual.Message);

        }


        [Fact]
        public  async Task Update_UpdateFreeTrailUserToPayingUserUpdateAllContactTemplatsDocumentsToNewGroup_Succses()
        {
            Company mockDBCompany = GetMockCompany();
            Company sentCompany =
           new Company()
           {
               Id = _oneGuid,
               ProgramId = _twoGuid
           };
            User sentUser = new User();
            int outTotalCount;
            List<Template> templates = new List<Template>()
                {
                    { new Template()
                    {
                        GroupId = _threeGuid
                    } },
                { new Template()
                    {
                        GroupId = _threeGuid
                    } }
                };

            List<Contact> contacts = new List<Contact>()
            {
                {new Contact()
                {
                    GroupId = _threeGuid
                } } ,
                  {new Contact()
                {
                    GroupId = _threeGuid
                }}
            };


            List<DocumentCollection> documentCollections = new List<DocumentCollection>()
            {
                {new DocumentCollection()
                {
                    GroupId = _threeGuid
                } } ,
                  {new DocumentCollection()
                {
                    GroupId = _threeGuid
                }}
            };
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(mockDBCompany);
            _groupConnector.Setup(x => x.Read(It.IsAny<User>()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User() { CompanyId = _oneGuid, ProgramUtilization = new ProgramUtilization { Id = _oneGuid } });
            _templateConnector.Setup(x => x.Update(It.IsAny<Template>()));
            _contactConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), out outTotalCount, It.IsAny<bool>())).Returns(contacts);
            _programConnector.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);
            _programUtilizationHistoryConnector.Setup(x => x.Create(It.IsAny<ProgramUtilizationHistory>()));


            _documentCollectionConnector.Setup(x => x.Read(It.IsAny<User>(),  It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                 It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), out outTotalCount, It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<SearchParameter>())).Returns(documentCollections);


            _programUtilizationConnector.Setup(x => x.Delete(It.IsAny<ProgramUtilization>()));


            _templateConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), out outTotalCount)).Returns(templates);
            Group group = new Group() { Id = _fourGuid, CompanyId = _fiveGuid };
            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);
            _programConnector.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);
            _userConnector.Setup(x => x.Update(It.IsAny<User>(), It.IsAny<bool>())).Callback((User inUser, bool aaa) => { sentUser = inUser; });

            await _companiesHandler.Update(sentCompany
                , group, new User());




            documentCollections.ForEach(x => Assert.Equal(group.Id.ToString(), x.GroupId.ToString()));
            contacts.ForEach(x => Assert.Equal(group.Id.ToString(), x.GroupId.ToString()));
            templates.ForEach(x => Assert.Equal(group.Id.ToString(), x.GroupId.ToString()));

        }

        [Fact]
        public async Task ResendResetPasswordMail_SentNullUser_ThrowException()
        {
            var actual =await  Assert.ThrowsAsync<Exception>(() => _companiesHandler.ResendResetPasswordMail(null));

            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public async Task ResendResetPasswordMail_SentUserNotInDB_ThrowException()
        {
            _userConnector.Setup(x => x.Read(It.IsAny<User>()));
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _companiesHandler.ResendResetPasswordMail(new User()));

            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ResendResetPasswordMail_SentUserIsNotCompanyAdmin_ThrowException()
        {
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User()
            {
                Type = UserType.Editor
            });
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _companiesHandler.ResendResetPasswordMail(new User()));

            Assert.Equal(ResultCode.InvalidUserType.GetNumericString(), actual.Message);
        }

    }

}
