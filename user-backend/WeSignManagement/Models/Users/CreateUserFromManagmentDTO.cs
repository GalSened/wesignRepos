using Common.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Models.Users
{
    public class CreateUserFromManagmentDTO
    {
        public string UserName { get; set; }
        public string UserUsername { get; set; }
        public string UserEmail { get; set; }
        public UserType UserType { get; set; }
        public Guid CompanyId { get; set; }
        public Guid GroupId { get; set; }
        public string Password { get; set; }
    }
}
