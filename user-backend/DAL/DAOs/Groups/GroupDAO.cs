namespace DAL.DAOs.Groups
{
    using Common.Enums.Groups;
    using Common.Models;
    using DAL.DAOs.ActiveDirectory;
    using DAL.DAOs.Companies;
    using DAL.DAOs.Users;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [Table("Groups")]
    public class GroupDAO
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public GroupStatus GroupStatus { get; set; }
        public virtual CompanyDAO Company { get; set; }
        public virtual ICollection<UserDAO> Users { get; set; }
        public virtual ActiveDirectoryGroupDAO ActiveDirectoryGroup { get; set; }

        public GroupDAO() { }
        public GroupDAO(Group group)
        {
            Id = group.Id == Guid.Empty ? default : group.Id;
            CompanyId = group.CompanyId == Guid.Empty ? default : group.CompanyId;
            Name = group.Name;
            GroupStatus = group.GroupStatus;
            Users = group.Users.Select(u => new UserDAO(u)).ToList();
        }
    }
}
