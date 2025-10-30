using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum OtpMode
    {
        None = 0,
        CodeRequired = 1,
        PasswordRequired = 2,
        CodeAndPasswordRequired = 3
    }
}