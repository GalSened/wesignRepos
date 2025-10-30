using BL.Handlers;
using Common.Enums;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Models;
using Common.Models.Configurations;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class AdminsHandlerTests : IDisposable
    {

        private readonly Mock<IUserConnector> _userConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<IProgramUtilizationConnector> _ProgramUtilizationConnectorMock;
        private readonly Mock<IValidator> _validatorMock;
        private readonly Mock<IEmail> _emailMock;
        private readonly Mock<IOneTimeTokens> _oneTimeTokensMock;
        private readonly Mock<IUsers> _usersMock;
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<ILogger> _loggerMock;
        private Mock<ISymmetricEncryptor> _symmetricEncryptorMock;
        private IAdmins _adminsHandler;
        private Mock<IGroupConnector> _groupConnectorMock;

        private readonly Mock<IMemoryCache> _memoryCache;
        private readonly Guid COMPANY_ID = new Guid("00000000-0000-0000-0000-000000000016");
        private readonly Guid GROUP_ID = new Guid("00000000-0000-0000-0000-000000000004");
        private readonly Guid CALLBACK_ID = new Guid("00000000-0000-0000-0000-000000000008");

        public AdminsHandlerTests()
        {
            _userConnectorMock = new Mock<IUserConnector>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _programConnectorMock = new Mock<IProgramConnector>();
            _ProgramUtilizationConnectorMock = new Mock<IProgramUtilizationConnector>();
              _validatorMock = new Mock<IValidator>();
            _emailMock = new Mock<IEmail>();
            _oneTimeTokensMock = new Mock<IOneTimeTokens>();
            _usersMock = new Mock<IUsers>();
            _daterMock = new Mock<IDater>();
            _loggerMock = new Mock<ILogger>();
            _symmetricEncryptorMock = new Mock<ISymmetricEncryptor>();
            _groupConnectorMock = new Mock<IGroupConnector>();
            _memoryCache = new Mock<IMemoryCache>();
            _adminsHandler = new AdminsHandlers(_emailMock.Object, _oneTimeTokensMock.Object,_userConnectorMock.Object,_companyConnectorMock.Object, 
                _programConnectorMock.Object, _ProgramUtilizationConnectorMock.Object, _usersMock.Object,
                _daterMock.Object, _loggerMock.Object, _symmetricEncryptorMock.Object, _groupConnectorMock.Object, _memoryCache.Object);
        }

        public void Dispose()
        {
            _userConnectorMock.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
            _programConnectorMock.Invocations.Clear();
            _ProgramUtilizationConnectorMock.Invocations.Clear();
            
            _validatorMock.Invocations.Clear();
            _emailMock.Invocations.Clear();
            _oneTimeTokensMock.Invocations.Clear();
            _usersMock.Invocations.Clear();
            _daterMock.Invocations.Clear();
        }

        #region Groups


        [Fact]
        public async  Task   CreateGroup_UserCompanyIsFreeTrial_ThrowsException()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User() { CompanyId = COMPANY_ID }, companySigner1Details));

            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = COMPANY_ID });

            var ioe =await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.Create(new Group()));

            Assert.Equal(ResultCode.OperationNotAllowByFreeTrialUser.GetNumericString(), ioe.Message);
        }

        [Fact]
        public async Task CreateGroup_CreateGroupSuccessfully_Success()
        {
            var user = new User() { CompanyId = CALLBACK_ID };
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));            
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = COMPANY_ID });
            _groupConnectorMock.Setup(x => x.Create(It.IsAny<Group>()));
            var group = new Group();
            await _adminsHandler.Create(group);

            Assert.Equal(user.CompanyId, group.CompanyId);


        }

        [Fact]
        public async Task ReadGroups_ReadGroupsUserIsFreeTrial_ThrowsException()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = COMPANY_ID });
            
            var ioe =await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.ReadGroups());

            Assert.Equal(ResultCode.OperationNotAllowByFreeTrialUser.GetNumericString(), ioe.Message);
        }


        [Fact]
        public async Task ReadGroups_ReadCompanyGroups_OneGroupReturn()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            var GroupsResult = new List<Group>();
            GroupsResult.Add(new Group { Id = GROUP_ID, GroupStatus = Common.Enums.Groups.GroupStatus.Created });
            _groupConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).Returns(GroupsResult);

            var groups = await _adminsHandler.ReadGroups();


            Assert.Single(groups);
           
            Assert.Equal(groups.ToList()[0].Id, GROUP_ID);
        }


        [Fact]
        public async Task ReadGroups_ReadCompanyGroups_MultipleGroupsReturnAsync()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            var groupsResult = new List<Group>
            {
                new Group { Id = GROUP_ID, GroupStatus = Common.Enums.Groups.GroupStatus.Created },
                new Group { Id = GROUP_ID, GroupStatus = Common.Enums.Groups.GroupStatus.Created },
                new Group { Id = GROUP_ID, GroupStatus = Common.Enums.Groups.GroupStatus.Created },
                new Group { Id = GROUP_ID, GroupStatus = Common.Enums.Groups.GroupStatus.Created },
                new Group { Id = GROUP_ID, GroupStatus = Common.Enums.Groups.GroupStatus.Created }
            };
            _groupConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).Returns(groupsResult);

            var groups = await _adminsHandler.ReadGroups();


       
            Assert.Equal(groups.Count(), groupsResult.Count);
            
        }


        [Fact]
        public async Task ReadGroups_ReadCompanyGroups_ZeroGroupsReturn()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            var GroupsResult = new List<Group>();           
            _groupConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).Returns(GroupsResult);

            var groups = await _adminsHandler.ReadGroups();
            Assert.Empty(groups);
        }

        [Fact]
        public async Task Delete_DeleteGroupUserIsFreeTrial_ThrowsException()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = COMPANY_ID });

            var ioe = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.Delete(new Group()));

            Assert.Equal(ResultCode.OperationNotAllowByFreeTrialUser.GetNumericString(), ioe.Message);
        }

        [Fact]
        public async Task Delete_DeleteGroupThatNotBelongToCompany_ThrowsException()
        {
            int totalCount = 1;
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID, GroupId = GROUP_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            List<User> users = new List<User>() { new User() { GroupId = GROUP_ID } };
            _userConnectorMock.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), null, out totalCount, It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(users);
            _groupConnectorMock.Setup(x => x.Read(new Company { Id = COMPANY_ID })).Returns(new List<Group>());

            var ioe = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.Delete(new Group() { Id = GROUP_ID }));

            Assert.Equal(ResultCode.InvalidGroupId.GetNumericString(), ioe.Message);
        }

        [Fact]  
        public async Task Delete_DeleteGroupWithOneUser_ThrowsException()
        {
           
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID, GroupId = GROUP_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            List<User> users = new List<User>() { new User() { GroupId = GROUP_ID } };
            _userConnectorMock.Setup(x => x.GetAllUsersInGroup(It.IsAny<Group>())).Returns(users);
            _groupConnectorMock.Setup(x=>x.Read(It.IsAny<Company>())).Returns(new List<Group> { new Group { Id = GROUP_ID } });
          
            var ioe = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.Delete(new Group() { Id = GROUP_ID }));

            Assert.Equal(ResultCode.ThereAreUsersInGroup.GetNumericString(), ioe.Message);
        }

        [Fact]
        public async Task Delete_DeleteGroupEmpty_Success()
        {
            int totalCount = 2;
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID, GroupId = GROUP_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            List<User> users = new List<User>();
            
            _userConnectorMock.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), null, out totalCount, It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(users);
            _groupConnectorMock.Setup(x => x.Delete(It.IsAny<Group>()));
            var group  = new Group() { Id = GROUP_ID };
            _groupConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).Returns(new List<Group> { group });
            
            await _adminsHandler.Delete(group);

            Assert.Equal(COMPANY_ID, group.CompanyId);
        }

        [Fact]
        public async Task Update_UpdateGroupUserIsFreeTrial_ThrowsException()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = COMPANY_ID });

            var ioe = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.Update(new Group()));

            Assert.Equal(ResultCode.OperationNotAllowByFreeTrialUser.GetNumericString(), ioe.Message);
        }
        [Fact]
        public async Task Update_UpdateGroup_Success()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { CompanyId = COMPANY_ID, GroupId = GROUP_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });            
            _groupConnectorMock.Setup(x => x.Update(It.IsAny<Group>()));
            var group = new Group() { Id = GROUP_ID };

            await _adminsHandler.Update(group);

            Assert.Equal(COMPANY_ID, group.CompanyId);
        }

        #endregion


        #region Users


        [Fact]
        public async Task CreateUser_UserCompanyIsFreeTrial_ThrowsException()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User() { CompanyId = COMPANY_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = COMPANY_ID });

            var ioe = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.Create(new User()));

            Assert.Equal(ResultCode.OperationNotAllowByFreeTrialUser.GetNumericString(), ioe.Message);
        }

        [Fact]
        public async Task CreateUser_CantFindUserGroup_ThrowsException()
        {
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User() { CompanyId = COMPANY_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            _groupConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).Returns(new List<Group>());


            var ioe =await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.Create(new User()));

            Assert.Equal(ResultCode.InvalidGroupId.GetNumericString(), ioe.Message);
        }

        [Fact]
        public async Task CreateUser_ProgramUsersCountIsZero_ThrowsException()
        {
            int totalCount = 1;
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User() { CompanyId = COMPANY_ID }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            var groups = new List<Group>(){
                new Group() { Id = GROUP_ID }
            };
            _groupConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).Returns(groups);
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(new Program() { Users = 0 });
            _userConnectorMock.Setup(x => x.Read(It.IsAny<string>(), 0, -1, null, out totalCount, It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(new List<User> { new User() });


            var ioe = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.Create(new User() { GroupId = GROUP_ID }));

            Assert.Equal(ResultCode.UsersExceedLicenseLimit.GetNumericString(), ioe.Message);
        }

        [Fact]
        public async Task CreateUser_UserEmailAlreadyExist_ThrowsException()
        {
            int totalCount = 1;
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User() { CompanyId = COMPANY_ID, UserConfiguration = new UserConfiguration() { Language = Language.en} }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            var groups = new List<Group>(){
                new Group() { Id = GROUP_ID }
            };
            _groupConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).Returns(groups);
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(new Program() { Users = -1});
            _userConnectorMock.Setup(x => x.Read(It.IsAny<string>(), 0, -1, null, out totalCount, It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(new List<User> { new User() });
            _daterMock.Setup(x => x.UtcNow()).Returns(DateTime.Now);
            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);

           var ioe = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminsHandler.Create(new User() { GroupId = GROUP_ID }));

            Assert.Equal(ResultCode.UsernameOrEmailAlreadyExist.GetNumericString(), ioe.Message);
        }


        [Fact]
        public async Task CreateUser_CreateNewUser_Success()
        {
            int totalCount = 1;
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User() { CompanyId = COMPANY_ID, UserConfiguration = new UserConfiguration() { Language = Language.en } }, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { Id = CALLBACK_ID });
            var groups = new List<Group>(){
                new Group() { Id = GROUP_ID }
            };
            _groupConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).Returns(groups);
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(new Program() { Users = -1 });
            _userConnectorMock.Setup(x => x.Read(It.IsAny<string>(), 0, -1, null, out totalCount, It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(new List<User> { new User() });
            _ProgramUtilizationConnectorMock.Setup(x => x.UpdateUsersAmount(It.IsAny<User>(), It.IsAny<CalcOperation>(), 1));
            DateTime date = DateTime.Now;
            _daterMock.Setup(x => x.UtcNow()).Returns(date);
            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _userConnectorMock.Setup(x => x.Create(It.IsAny<User>()));
            _oneTimeTokensMock.Setup(x => x.GenerateRefreshToken(It.IsAny<User>()));
            _oneTimeTokensMock.Setup(x => x.GenerateResetPasswordToken(It.IsAny<User>()));
            _emailMock.Setup(x => x.ResetPassword(It.IsAny<User>(), It.IsAny<string>()) );
            var user = new User() { GroupId = GROUP_ID };

            await _adminsHandler.Create(user);

            Assert.Equal(user.CompanyId,  COMPANY_ID);
            Assert.Equal(date , user.CreationTime );
            Assert.Equal(Language.en, user.UserConfiguration.Language);           
        }



        /*
         




          _dbConnector.Users.Create(user);
            _oneTimeTokens.GenerateRefreshToken(user);

            string resetPasswordToken = _oneTimeTokens.GenerateResetPasswordToken(user);
            _email.ResetPassword(user, resetPasswordToken);

         var adminUser = _users.GetUser();
            user.CompanyId = adminUser.CompanyId;
            user.CreationTime = _dater.UtcNow();
            user.UserConfiguration.Language = adminUser.UserConfiguration.Language;
            if (_dbConnector.Users.Exists(user))
            {
                throw new InvalidOperationException(ResultCode.EmailAlreadyExist.GetNumericString());
            }

         */
        #endregion
    }
}

