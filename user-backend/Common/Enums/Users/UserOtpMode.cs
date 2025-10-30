using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums.Users
{
    public enum UserOtpMode
    {
        None = 0,
        Login = 1,
        PhoneUpdate = 2
    }
}
