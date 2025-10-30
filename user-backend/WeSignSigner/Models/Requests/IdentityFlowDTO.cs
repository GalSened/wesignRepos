using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignSigner.Models.Requests
{
    public class IdentityFlowDTO
    {
        public string SignerToken { get; set; }
        public string Code{ get; set; }
    }
}
