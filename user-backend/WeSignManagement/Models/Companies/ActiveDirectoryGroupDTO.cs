using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Models.Companies
{
    public class ActiveDirectoryGroupDTO
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string ActiveDirectoryUsersGroupName { get; set; }
        public string ActiveDirectoryContactsGroupName { get; set; }
    }
}
