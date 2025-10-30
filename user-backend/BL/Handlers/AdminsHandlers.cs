

// Ignore Spelling: Admins

namespace BL.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Extensions;
    using Common.Enums.Results;
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Models;
    using Common.Interfaces.Emails;
    using Common.Consts;
    using Common.Enums.Users;
    using Common.Enums.Groups;
    using Serilog;
    using Microsoft.Extensions.Caching.Memory;
    using System.Threading.Tasks;
    using System.Runtime.CompilerServices;

    public class AdminsHandlers : IAdmins
    {
        private readonly string FREE_ACCOUNTS_COMPANY_NAME = "Free Accounts";
        private readonly IProgramConnector _programConnector;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly IUserConnector _userConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IEmail _email;
        private readonly IOneTimeTokens _oneTimeTokens;
        private readonly IUsers _users;
        private readonly IDater _dater;
        private readonly ILogger _logger;
        private readonly ISymmetricEncryptor _symmetricEncryptor;
        private readonly IGroupConnector _groupConnector;
        private readonly IMemoryCache _memoryCache;

        public AdminsHandlers( IEmail email, IOneTimeTokens oneTimeTokens, IUserConnector userConnector,ICompanyConnector companyConnector,
            IProgramConnector programConnector, IProgramUtilizationConnector programUtilizationConnector,
            IUsers users, IDater dater, ILogger logger, ISymmetricEncryptor symmetricEncryptor, IGroupConnector groupConnector,
            IMemoryCache memoryCache)
        {

            _programConnector = programConnector;
            _programUtilizationConnector = programUtilizationConnector;
            _userConnector = userConnector;
            _companyConnector = companyConnector;
           _email = email;
            _oneTimeTokens = oneTimeTokens;
            _users = users;
            _dater = dater;
            _logger = logger;
            _symmetricEncryptor = symmetricEncryptor;
            _groupConnector = groupConnector;
            _memoryCache = memoryCache;
        }

        #region Groups

        public async Task Create(Group group)
        {
            (var user, var _) = await _users.GetUser();
            await ValidateNonFreeTrialUser(user);
            group.CompanyId = user.CompanyId;
            await _groupConnector.Create(group);
            _logger.Information("New group {GroupName} created successfully by admin user in company ID: {CompanyId} [{UserId}: {UserName}]", group.Name, user.CompanyId, user.Id, user.Name);
        }

        public async Task Create(User user)
        {
            if (user.Type == UserType.SystemAdmin)
            {
                throw new InvalidOperationException(ResultCode.InvalidUserType.GetNumericString());
            }
            var groups = await ReadGroups();
            var group = groups.FirstOrDefault(g => g.Id == user.GroupId);
            if (group == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
            }

            Company company = await _companyConnector.Read(new Company() { Id = group.CompanyId });
            Program program = await _programConnector.Read(new Program() { Id = company.ProgramId });
            var usersCount = _userConnector.UsersInCompanyCount(company);

            if (program.Users >= 0 && program.Users <= usersCount)
            {
                throw new InvalidOperationException(ResultCode.UsersExceedLicenseLimit.GetNumericString());
            }

            (var adminUser, var _) = await _users.GetUser();
            foreach (var additinalGroups in user.AdditionalGroupsMapper ?? Enumerable.Empty<AdditionalGroupMapper>())
            {
                if (!groups.Any(g => g.Id == additinalGroups.GroupId))
                {
                    throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
                }
                additinalGroups.CompanyId = adminUser.CompanyId;
                additinalGroups.UserId = user.Id;
            }
            user.CompanyId = adminUser.CompanyId;
            user.CreationTime = _dater.UtcNow();
            user.UserConfiguration.Language = adminUser.UserConfiguration.Language;
            if (await _userConnector.Exists(user))
            {
                throw new InvalidOperationException(ResultCode.UsernameOrEmailAlreadyExist.GetNumericString());
            }
            await _userConnector.Create(user);
            await _programUtilizationConnector.UpdateUsersAmount(user, Common.Enums.CalcOperation.Add);
            await _oneTimeTokens.GenerateRefreshToken(user);

            string resetPasswordToken =await _oneTimeTokens.GenerateResetPasswordToken(user);
            await _email.ResetPassword(user, resetPasswordToken);

            _logger.Information("new User [{UserId} : {UserName}] created in group {GroupName} by admin user in company {CompanyId} [{UserIdAdmin}: {UserNameAdmin}]", user.Id, user.Name, group.Name, user.CompanyId, adminUser.Id, adminUser.Name);
        }

        public async Task<IEnumerable<Group>> ReadGroups()
        {
            (var user, var _) = await _users.GetUser();
            await ValidateNonFreeTrialUser(user);
            return _groupConnector.Read(new Company { Id = user.CompanyId }).Where(g=> g.GroupStatus != GroupStatus.Deleted);
        }


        public async Task Delete(User user)
        {
            (var adminUser, var _) = await _users.GetUser();
            await ValidateNonFreeTrialUser(adminUser);
            if (adminUser.Id == user.Id)
            {
                throw new InvalidOperationException(ResultCode.CannotDeleteYourOwnUser.GetNumericString());
            }
            var dbUser = await _userConnector.Read(new User() { Id = user.Id });
            if (dbUser.CompanyId != adminUser.CompanyId)
            {
                throw new InvalidOperationException(ResultCode.UserBelongToAnotherCompany.GetNumericString());
            }


            _logger.Information("Admin user {UserId}: {UserEmail} admin in company {CompanyId} -  deleted user  {UserIdDb}: {UserEmailDb}");
            await _userConnector.Delete(dbUser);

            await _programUtilizationConnector.UpdateUsersAmount(dbUser, Common.Enums.CalcOperation.Substruct);
        }


        public async Task Delete(Group group)
        {
            (var user, var _) = await _users.GetUser();
            await ValidateNonFreeTrialUser(user);
            var companyGroups = _groupConnector.Read(new Company { Id = user.CompanyId });
            if (companyGroups.FirstOrDefault(x => x.Id == group.Id) == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
            }
            group.CompanyId = user.CompanyId;

            var users = _userConnector.GetAllUsersInGroup(group);
            if (users.Any())
            {
                throw new InvalidOperationException(ResultCode.ThereAreUsersInGroup.GetNumericString());
            }
            
            _logger.Information("Admin user {UserId}: {UserEmail} admin in company {CompanyId} - deleted group {GroupId}: {CompanyName}",
                                         user.Id, user.Email, group.CompanyId, group.Id, companyGroups.FirstOrDefault(x => x.Id == group.Id).Name);
            await _groupConnector.Delete(group);
        }

        public async Task Update(Group group)
        {
            (var user, var _) = await _users.GetUser();
            await ValidateNonFreeTrialUser(user);
            group.CompanyId = user.CompanyId;
            await _groupConnector.Update(group);        
            _logger.Information("Group {GroupName} updated successfully by admin user in company {CompanyId} [{UserId} : {UserName}]", group.Name, user.CompanyId, user.Id, user.Name);
            

        }

        public async Task Update(User user)
        {
            var dbUser = await _userConnector.Read(new User() { Id = user.Id });
            (var adminUser, var _) = await _users.GetUser();
            await ValidateNonFreeTrialUser(adminUser);
            var newGroupIds = new HashSet<Guid>(user.AdditionalGroupsMapper.Select(_ => _.GroupId));
            var groupIdsToRemove = dbUser.AdditionalGroupsMapper.Where(prevGroup => !newGroupIds.Contains(prevGroup.GroupId)).Select(_ => _.GroupId);
            if (adminUser.Id == user.Id && groupIdsToRemove.Contains(adminUser.GroupId))
            {
                throw new InvalidOperationException(ResultCode.UserCannotRemoveConnectedGroup.GetNumericString());
            }
            if (dbUser.CompanyId != adminUser.CompanyId)
            {
                throw new InvalidOperationException(ResultCode.UserBelongToAnotherCompany.GetNumericString());
            }
            if (user.Email.ToLower().Trim() != dbUser.Email.ToLower().Trim())
            {
                if (await _userConnector.ExistsByEmail(user))
                {
                    throw new InvalidOperationException(ResultCode.EmailAlreadyExist.GetNumericString());
                }
            }

            if (user.Username != dbUser.Username)
            {

                if (await _userConnector.ExistsByUsername(user))
                {
                    throw new InvalidOperationException(ResultCode.UsernameAlreadyExist.GetNumericString());
                }
            }

            var companyGroups = _groupConnector.Read(new Company { Id = dbUser.CompanyId });
            if (companyGroups.FirstOrDefault(x => x.Id == user.GroupId) == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
            }
            HashSet<Guid> groups = new HashSet<Guid>();
            foreach (var additinalGroups in user.AdditionalGroupsMapper ?? Enumerable.Empty<AdditionalGroupMapper>())
            {
                if (!companyGroups.Any(g => g.Id == additinalGroups.GroupId))
                {
                    throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
                }
                if (groups.Contains(additinalGroups.GroupId))
                {
                    throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
                }
                groups.Add(additinalGroups.GroupId);
                additinalGroups.CompanyId = dbUser.CompanyId;
                additinalGroups.UserId = user.Id;
            }
            if (adminUser.Id != user.Id)
            {
                if (user.Type == UserType.SystemAdmin)
                {
                    throw new InvalidOperationException(ResultCode.InvalidUserType.GetNumericString());
                }
                dbUser.Name = user.Name;
                dbUser.GroupId = user.GroupId;
                dbUser.Email = user.Email;
                dbUser.Type = user.Type;
                dbUser.Username = user.Username;
                    
                await _userConnector.Update(dbUser, true);
            }
            if (user.AdditionalGroupsMapper.Count > 0)
            {

                user.AdditionalGroupsMapper.RemoveAll(z => z.GroupId == dbUser.GroupId);

            }
            await _groupConnector.UpdateAdditionalGroups(dbUser, user.AdditionalGroupsMapper);

            foreach (var group in companyGroups)
            {
                _memoryCache.Remove($"{dbUser.Id}_{group.Id}");
            }

            _logger.Information("User [{UserId} : {UserName}] updated by admin user in company {CompanyId} [{UserIdAdmin}: {UserNameAdmin}]", dbUser.Id, dbUser.Name, adminUser.CompanyId, adminUser.Id, adminUser.Name);
        }


        #endregion

        #region Users


        public async Task<(IEnumerable<User>,int)> ReadUsers(string key, int offset, int limit, UserStatus? status)
        {
            
            (var adminUser, var _) = await _users.GetUser();
            await ValidateNonFreeTrialUser(adminUser);
            IEnumerable<Guid> groupIds = null;
            if (key != null)
            {
                var dbGroups = _groupConnector.ReadManyByGroupName(key);
                if (dbGroups != null)
                {
                    groupIds = dbGroups.Select(g => g.Id);
                }
            }
            var users = _userConnector.Read(key, offset, limit, status, out int totalCount, adminUser.CompanyId, groupIds).ToList();
            foreach (var user in users ?? Enumerable.Empty<User>())
            {
                if (!string.IsNullOrWhiteSpace(user.Username))
                {
                    try
                    {
                        user.Username = await _symmetricEncryptor.DecryptString(Consts.symmetricKey, user.Username);
                    }
                    catch
                    {
                        // do nothing
                    }
                }
            }
            
            return (users,totalCount);
        }

        #endregion

        private async Task ValidateNonFreeTrialUser(User user)
        {
            var freeAccountCompany = await _companyConnector.Read(
               new Company()
               {
                   Id = Consts.FREE_ACCOUNTS_COMPANY_ID,
                   Name = FREE_ACCOUNTS_COMPANY_NAME
               });
            if (freeAccountCompany != null && user.CompanyId == freeAccountCompany.Id)
            {
                throw new InvalidOperationException(ResultCode.OperationNotAllowByFreeTrialUser.GetNumericString());
            }


        }


    }
}
