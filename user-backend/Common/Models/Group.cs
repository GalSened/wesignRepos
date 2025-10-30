namespace Common.Models
{
    using Common.Enums.Groups;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public GroupStatus GroupStatus { get; set; }
        public IEnumerable<User> Users { get; set; }
        public Group()
        {
            GroupStatus = GroupStatus.Created;
            Users = Enumerable.Empty<User>();
        }
    }
}
