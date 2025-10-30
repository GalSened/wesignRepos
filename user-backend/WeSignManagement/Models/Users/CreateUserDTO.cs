using Common.Enums.Users;
using System;

namespace WeSignManagement.Models.Users
{
    public class CreateUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserUsername { get; set; }
        public Guid CompanyId { get; set; }
        public Guid GroupId { get; set; }
        public UserType Type { get; set; }
        public Language Language { get; set; }

    }
}
