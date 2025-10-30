namespace WeSign.Models.Admins.Response
{
    using Common.Enums.Users;
    using Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class UserResponseAdminDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public UserType Type { get; set; }
        public Guid GroupId { get; set; }
        public List<Guid> AdditionalGroupsIds { get; set; }

        public UserResponseAdminDTO() { }
        public UserResponseAdminDTO(User user)
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
            Username = user.Username;
            Type = user.Type;
            GroupId= user.GroupId;
            AdditionalGroupsIds =  user?.AdditionalGroupsMapper?.Select(x => x.GroupId).ToList();

        }
    }
}
