using Common.Enums.ActiveDirectory;
using System;

namespace Common.Models.ActiveDirectory
{
    public class ActiveDirectoryGroup
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string ActiveDirectoryUsersGroupName { get; set; }
        public string ActiveDirectoryContactsGroupName { get; set; }
        public Group Group { get; set; }
    }
}
