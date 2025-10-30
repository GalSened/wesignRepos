using Common.Models;
using Common.Models.ActiveDirectory;
using Common.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IActiveDirectory
    {
        /// <summary>
        /// Read AD configuration for companyId from DB
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        Task<ActiveDirectoryConfiguration> Read();
        void Create(ActiveDirectoryConfiguration adConfig);

        /// <summary>
        /// Return all AD groups for domain
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        Task<(IEnumerable<string>, bool isSuccess)> ReadADGroups(ActiveDirectoryConfiguration activeDirectoryConfiguration);
        Task AddUpdateCompanyADGroupsMapping(Company company, ICollection<ActiveDirectoryGroup> activeDirectoryGroups);
        Task  DeleteCompanyMappedGroup(Company company);

       Task<( IEnumerable<Comda.Authentication.Models.User>, bool isSuccess)> ReadAllUsersFromActiveDirectoryByGroupName(ActiveDirectoryConfiguration activeDirectoryConfiguration, string adGroupNameout);
        /// <summary>
        /// Create AD configuration for companyId in DB
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="adConfig"></param>
    }
}
