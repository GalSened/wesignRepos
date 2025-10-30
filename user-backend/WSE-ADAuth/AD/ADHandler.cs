
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using WSE_ADAuth.Models;
using Serilog;
using System.DirectoryServices;
using WSE_ADAuth.Extensions;

namespace WSE_ADAuth.AD
{
    public class ADHandler : IAD
    {

        private string _userName;
        private UserPrincipal _userPrincipal;
        private readonly ILogger _logger;
        private readonly IOptions<ADGeneralSettings> _generalSettings;
      
        public ADHandler(ILogger logger, IOptions<ADGeneralSettings> generalSettings)
        {
            _logger = logger;
            _generalSettings = generalSettings;
        }

        public void InItADHandlerForUser(string userName)
        {
            _userName = userName;
            _logger.Debug("Name {UserName}", _userName);
            _userPrincipal = GetUserPrincipal();
        }

        public bool IsEmailAddressExist()
        {
            if (_userPrincipal != null)
            {
                try
                {
                    return !string.IsNullOrWhiteSpace(_userPrincipal.EmailAddress);
                }
                catch (Exception ex)
                {
                    _logger.Error("User Have no Email Address", ex);
                }
            }
            else
            {
                _logger.Warning("User Principal in null");
            }
            return false;

        }

        public string GetUserAdName()
        {
            return _userPrincipal.SamAccountName;
        }

        public string GetUserEmail()
        {          
            if (IsEmailAddressExist())
            {
                
                return _userPrincipal.EmailAddress;
                
            }
            return string.Empty;

        }

        public  bool IsUserInADGroup(string searchForGroup)
        {
            _logger.Debug("check if user in group {Group}", searchForGroup);
            if (_userPrincipal != null)
            {

                if (!_generalSettings.Value.CheckNestedGroups)
                {
                    foreach (GroupPrincipal group in _userPrincipal.GetGroups())
                    {
                        //    _logger.Debug("group {GroupName}", group.name);
                        if (group.Name.ToLower() == searchForGroup.ToLower())
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    // search in nested groups
                    foreach (GroupPrincipal group in _userPrincipal.GetAuthorizationGroups())
                    {
                        if (group.Name.ToLower() == searchForGroup.ToLower())
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                _logger.Debug("_userPrincipal is null");
            }

            return false;
        }

        private UserPrincipal GetUserPrincipal()
        {

            PrincipalContext principalContext;
            string domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            if (!string.IsNullOrWhiteSpace(_generalSettings.Value.ADDomainName))
            {
                domain = _generalSettings.Value.ADDomainName;
            }

            _logger.Debug("Selected domain: {Domain}", domain);
            if (!string.IsNullOrEmpty(_generalSettings.Value.ADAdminUser) &&
                !string.IsNullOrEmpty(_generalSettings.Value.ADAdminPass))
            {

                principalContext = new PrincipalContext(ContextType.Domain, domain,
                   _generalSettings.Value.ADAdminUser, _generalSettings.Value.ADAdminPass);
            }
            else
            {
                _logger.Debug("creating   principal domain: {Domain}", domain);
                principalContext = new PrincipalContext(ContextType.Domain, domain);
            }
            _logger.Debug("find User Principal  : {UserName}", _userName);
            return UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, _userName);
        }

        public List<string> GetUserPhones()
        {
            List<string> phones = new List<string>();
            var phone = "";
            if (!string.IsNullOrWhiteSpace(_generalSettings.Value.ADKeyForMoblie))
            {
                if (_userPrincipal.GetUnderlyingObjectType() == typeof(DirectoryEntry))
                {
                    // Transition to directory entry to get other properties
                    using (var entry = (DirectoryEntry)_userPrincipal.GetUnderlyingObject())
                    {
                        if (entry.Properties[_generalSettings.Value.ADKeyForMoblie] != null)
                        {
                            phone = entry.Properties[_generalSettings.Value.ADKeyForMoblie].Value.ToString();
                        }
                    }
                }
            }
           
            return phone.GetAllPhones();
        }

        
    }
}
