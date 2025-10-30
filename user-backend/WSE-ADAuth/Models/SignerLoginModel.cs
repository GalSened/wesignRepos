using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WSE_ADAuth.Models
{
    public class SignerLoginModel
    {
        public Guid GuidAuthToken { get; set; }
        public List<string> PhoneNumbers { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string AuthToken { get; set; }
        public string AuthId { get; set; }
        public string AuthName { get; set; }
    }
}
