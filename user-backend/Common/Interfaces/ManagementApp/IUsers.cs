using Common.Enums.Users;
using Common.Models;
using Common.Models.ManagementApp;
using Common.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.ManagementApp
{
    public interface IUsers
    {
        Task<(IEnumerable<UserDetails>, int totalCount)> Read(string key, int offset, int limit, UserStatus? status);
        Task<(bool, UserTokens userTokens)> TryLogin(User user);
        Task ResetPassword(User user);
        Task UpdateEmail(User user);
        Task Refresh(UserTokens tokens);
        Task Delete(User user);
        Task CreateUserFromManagment(User user);
        Task<User> Read(User user);        
        Task<User> GetCurrentUser();
        Task UpdateUser(User user);
        Task ResendResetPasswordMail(User user);
        Task<Dictionary<Guid, string>> ReadTemplates(User user);
        Task CreateHtmlTemplate(User user, Template template, string htmlFile, string jsFile);
        Task<(IEnumerable<UserDetails>, int totalCount)> ReadAllUsersInCompany(Company company);
        Task<List<Group>> GetUserGroups();
        Task<List<Group>> GetUserGroups(User user);

    }
}
