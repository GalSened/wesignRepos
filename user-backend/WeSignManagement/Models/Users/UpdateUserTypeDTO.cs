using Common.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Models.Users
{
    public class UpdateUserTypeDTO
    {
        public Guid UserId { get; set; }
        public UserType UserType { get; set; }
    }
}
