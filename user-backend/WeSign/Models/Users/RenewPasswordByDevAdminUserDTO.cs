using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Users
{
    public class RenewPasswordByDevAdminUserDTO
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }

    }
}
