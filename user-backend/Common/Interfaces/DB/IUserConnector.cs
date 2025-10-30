using Common.Enums.Users;
using Common.Models;
using Common.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Interfaces.DB
{
    public interface IUserConnector
    {
        Task Create(User user);
        Task<User> Read(User user);
     
        IEnumerable<User> Read(string key, int offset, int limit, UserStatus? status, out int totalCount, Guid companyId = default, IEnumerable<Guid> groupIds = null);
        IEnumerable<User> Read(Group group);
        Task<User> ReadWithUserToken(User user);
        Task<User> ReadAllStatuesById(Guid UserId); 
        Task<User> ReadAsync(User user);
        /// <summary>
        /// No link to the ProgramUtilization table is made here because the package utilization can be done simultaneously by several different users
        /// if companyId is Guid.Empty we will return all users from all companies
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
       
        IEnumerable<User> ReadAdminUsersInCompany(Company company);
        Task<bool> Exists(User user);
        Task<bool> ExistsByEmail(User user);
       Task<bool> ExistsByUsername(User user);
        IEnumerable<User> ReadDeletedUsers();
        Task Update(User user, bool adminUpdate);
        Task UpdateLastSeen(User user);
        Task UpdateAsync(User user);
        Task Delete(User user);
        Task Delete(User user, Action<User> cleanUserCertificate);
        IEnumerable<User> GetAllUsersInGroup(Group group);           
        IEnumerable<User> GetAllUsersInCompany(Company company);
        IEnumerable<User> ReadFreeTrialUsers();
        Task UpdateUserMainGroup(User user);
        IEnumerable<User> ReadUsersByType(UserType systemAdmin);
        int UsersInCompanyCount(Company company);
        UserOtpDetails ReadOtpDitails(User user);
        Task SetUserOtpDitails(User user, UserOtpDetails userOtpDetails);
        Task UpdateUserPhone(User user);
    }
}
