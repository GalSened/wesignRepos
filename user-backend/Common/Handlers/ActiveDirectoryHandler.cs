using Comda.Authentication.Enums;
using Comda.Authentication.Factory;
using Comda.Authentication.Handlers;
using Comda.Authentication.Interfaces;
using Comda.Authentication.Models;
using Common.Enums.Groups;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.ActiveDirectory;
using Common.Models.Configurations;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers
{
    public class ActiveDirectoryHandler : IActiveDirectory
    {
        private readonly IActiveDirectoryGroupsConnector _activeDirectoryGroupsConnector;
        private readonly IActiveDirectoryConfigConnector _activeDirectoryConfigConnector;
        private readonly IGroupConnector _groupConnector;
        private readonly ILicense _license;
        private readonly IEncryptor _encryptor;
        protected ILogger _logger;

        public ActiveDirectoryHandler(IActiveDirectoryGroupsConnector activeDirectoryGroupsConnector, IGroupConnector groupConnector,
            IActiveDirectoryConfigConnector activeDirectoryConfigConnector, ILicense license, ILogger logger,
            IEncryptor encryptor)
        {
            _activeDirectoryGroupsConnector = activeDirectoryGroupsConnector;
            _activeDirectoryConfigConnector = activeDirectoryConfigConnector;
            _groupConnector = groupConnector;
                _license = license;
            _encryptor = encryptor;
            _logger = logger;
        }

        public async Task AddUpdateCompanyADGroupsMapping(Company company, ICollection<ActiveDirectoryGroup> activeDirectoryGroups)
        {
            (var license, var _) = await _license.GetLicenseInformationAndUsing(false);
            if (license?.LicenseCounters?.UseActiveDirectory ?? false)
            {

                await RemoveDeletedGroupsMappers(company, activeDirectoryGroups);
                await AddNewMappingGroups(company, activeDirectoryGroups);
                await UpdateExistinggroups(company, activeDirectoryGroups);
            }
        }

        private async Task UpdateExistinggroups(Company company, ICollection<ActiveDirectoryGroup> activeDirectoryGroups)
        {
            var activeGroupsMapping = await _activeDirectoryGroupsConnector.GetAllGroupsForCompany(company);
            List<ActiveDirectoryGroup> activeDirectoryGroupsToUpdate = new List<ActiveDirectoryGroup>();
            foreach (var item in activeDirectoryGroups)
            {
                var selectedItem = activeGroupsMapping.FirstOrDefault(x => x.GroupName == item.GroupName);
                if(selectedItem !=null &&
                    (selectedItem.ActiveDirectoryUsersGroupName != item.ActiveDirectoryUsersGroupName || item.ActiveDirectoryContactsGroupName != selectedItem.ActiveDirectoryContactsGroupName))
                {
                    activeDirectoryGroupsToUpdate.Add(new ActiveDirectoryGroup()
                    {
                        Id = selectedItem.Id,
                        ActiveDirectoryContactsGroupName = item.ActiveDirectoryContactsGroupName,
                        ActiveDirectoryUsersGroupName = item.ActiveDirectoryUsersGroupName
                    }
                        );
                }
            }

            foreach(var itemToUpdate in activeDirectoryGroupsToUpdate)
            {
                await _activeDirectoryGroupsConnector.Update(itemToUpdate);
            }
        }

        private async Task AddNewMappingGroups(Company company, ICollection<ActiveDirectoryGroup> activeDirectoryGroups)
        {
            var distinctActiveDirectoryGroups = activeDirectoryGroups.Distinct(new ActiveDirectoryGroupComparer());
            var groups =_groupConnector.Read(company).Where(g => g.GroupStatus != GroupStatus.Deleted);

            var activeGroupsMapping =await _activeDirectoryGroupsConnector.GetAllGroupsForCompany(company);

            List<ActiveDirectoryGroup> activeDirectoryGroupsToAdd = new List<ActiveDirectoryGroup>();
            foreach(var item in distinctActiveDirectoryGroups)
            {
                var existItem = activeGroupsMapping.FirstOrDefault(x => x.GroupName == item.GroupName);
                var selectedgroup = groups.FirstOrDefault(x => x.Name  == item.GroupName);
                if ((existItem == null) && (selectedgroup != null))
                {
                    activeDirectoryGroupsToAdd.Add(new ActiveDirectoryGroup()
                    {
                        ActiveDirectoryContactsGroupName = item.ActiveDirectoryContactsGroupName,
                        ActiveDirectoryUsersGroupName = item.ActiveDirectoryUsersGroupName,
                        GroupId = selectedgroup.Id,
                        GroupName = item.GroupName

                    });
                }
            }
            
            foreach (var item in activeDirectoryGroupsToAdd)
            {
                await _activeDirectoryGroupsConnector.Create(item);
            }
        }

        private async Task RemoveDeletedGroupsMappers(Company company, ICollection<ActiveDirectoryGroup> activeDirectoryGroups)
        {
            var activeGroupsMapping =await _activeDirectoryGroupsConnector.GetAllGroupsForCompany(company);

            List<ActiveDirectoryGroup> activeDirectoryGroupsToRemove = new List<ActiveDirectoryGroup>();

            
            foreach(var item in activeGroupsMapping)
            {
                if(activeDirectoryGroups.FirstOrDefault(x => x.GroupName == item.GroupName) == null)
                {
                    activeDirectoryGroupsToRemove.Add(item);
                }
            }

           
            foreach (var item in activeDirectoryGroupsToRemove)
            {
                await _activeDirectoryGroupsConnector.Remove(item);
            }

        }

        public void Create(ActiveDirectoryConfiguration adConfig)
        {
            throw new NotImplementedException();
        }

        public async Task<ActiveDirectoryConfiguration> Read()
        {
           await _license.GetLicenseInformationAndUsing( false);
            return await _activeDirectoryConfigConnector.Read();
            
        }

        public  async Task<(IEnumerable<string>, bool isSuccess)> ReadADGroups(ActiveDirectoryConfiguration activeDirectoryConfiguration)
        {
            bool isSuccess;
            await _license.GetLicenseInformationAndUsing(false);

            IProvider authProvider = GetAuthProvider(activeDirectoryConfiguration);

            return (authProvider.GetGroups(out isSuccess), isSuccess);
        }


        private IProvider GetAuthProvider(ActiveDirectoryConfiguration activeDirectoryConfiguration)
        {
            UserCredentials userCredentials = new UserCredentials();
            if(activeDirectoryConfiguration == null)
            {
                throw new ArgumentException("", "activeDirectoryConfiguration");
            }
            if (!string.IsNullOrWhiteSpace(activeDirectoryConfiguration.User))
            {
                userCredentials.UserName = activeDirectoryConfiguration.User;
            }
            if (!string.IsNullOrWhiteSpace(activeDirectoryConfiguration.Password))
            {
                userCredentials.Password = _encryptor.Decrypt(activeDirectoryConfiguration.Password).EncryptString();
            }

            Connection adConnector = new ActiveDirectoryConnector(activeDirectoryConfiguration?.Host, (LdapConnectionPort)activeDirectoryConfiguration.Port, activeDirectoryConfiguration?.Container,
                activeDirectoryConfiguration?.Domain);
            var factory = new AuthProviderFactory();
            IProvider authProvider = null;
            try
            {
                authProvider = factory.Create(adConnector, userCredentials, _logger.Debug);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetAuthProvider = ");
            }
            return authProvider;
        }

        public async Task DeleteCompanyMappedGroup(Company company)
        {
            var activeDirectoryGroupsToRemove = await _activeDirectoryGroupsConnector.GetAllGroupsForCompany(company);
            foreach (var item in activeDirectoryGroupsToRemove)
            {
                await _activeDirectoryGroupsConnector.Remove(item);
            }
        }

        public async Task<(IEnumerable<Comda.Authentication.Models.User >, bool isSuccess)> ReadAllUsersFromActiveDirectoryByGroupName(ActiveDirectoryConfiguration activeDirectoryConfiguration, string adGroupNameout)
        {
            bool isSuccess;
            await _license.GetLicenseInformationAndUsing(false);

            IProvider authProvider = GetAuthProvider(activeDirectoryConfiguration);

            return (authProvider.GetUsers(out isSuccess, adGroupNameout), isSuccess);
            
        }
    }


    class ActiveDirectoryGroupComparer : IEqualityComparer<ActiveDirectoryGroup>
    {
        public bool Equals(ActiveDirectoryGroup x, ActiveDirectoryGroup y)
        {
            // Two items are equal if their keys are equal.
            return x.GroupName == y.GroupName;
        }

        public int GetHashCode(ActiveDirectoryGroup obj)
        {
            return obj.GroupName.GetHashCode();
        }
    }
}
