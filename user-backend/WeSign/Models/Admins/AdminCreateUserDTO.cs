namespace WeSign.Models.Admins
{
    using Common.Enums.Users;
    using System;
    using System.Collections.Generic;

    public class AdminCreateUserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public UserType Type { get; set; }
        public Guid GroupId { get; set; }
        public List<Guid> AdditionalGroupsIds { get; set; }
    }
}
