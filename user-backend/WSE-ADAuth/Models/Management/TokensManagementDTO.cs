using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WSE_ADAuth.Models.Management
{

    public class TokensManagementDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

}
