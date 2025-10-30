using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WSE_ADAuth.Models
{
    public class LoginToClient
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; } 
        public string UserAuthToken { get; set; } 
        public LoginSource LoginSource { get; set; }
    }
}
