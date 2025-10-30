using Common.Models;
using System;

namespace WeSign.Models.Admins.Response
{
    public class GroupResponseAdminDTO
    {
        public Guid GroupId { get; set; }
        public string Name { get; set; }

        public GroupResponseAdminDTO() { }

        public GroupResponseAdminDTO(Group group)
        {
            GroupId = group.Id;
            Name = group.Name;
        }
    }
}
