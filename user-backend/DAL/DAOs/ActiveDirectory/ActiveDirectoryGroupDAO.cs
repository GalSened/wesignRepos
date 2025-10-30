using Common.Enums.ActiveDirectory;
using Common.Models.ActiveDirectory;
using DAL.DAOs.Groups;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.ActiveDirectory
{
    [Table("ActiveDirectoryGroups")]
    public class ActiveDirectoryGroupDAO
    {

        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string ActiveDirectoryUsersGroupName { get; set; }
        public string ActiveDirectoryContactsGroupName { get; set; }

        public virtual GroupDAO Group { get; set; }

        public ActiveDirectoryGroupDAO() { }
        public ActiveDirectoryGroupDAO(ActiveDirectoryGroup activeDirectoryGroup)
        {
            Id = activeDirectoryGroup.Id == Guid.Empty ? default : activeDirectoryGroup.Id;
            GroupId = activeDirectoryGroup.GroupId == Guid.Empty ? default : activeDirectoryGroup.GroupId;
            ActiveDirectoryUsersGroupName = activeDirectoryGroup.ActiveDirectoryUsersGroupName;
            ActiveDirectoryContactsGroupName = activeDirectoryGroup.ActiveDirectoryContactsGroupName;
            
        }
    }
}

