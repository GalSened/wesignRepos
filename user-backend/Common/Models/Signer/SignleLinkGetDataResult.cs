using Common.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public  class SignleLinkGetDataResult
    {
        public Template Template { get; set; }
        public bool IsSmsProviderSupportGloballySend { get; set; }
        public Language Language { get; set; }
        public List<SingleLinkAdditionalResource> SingleLinkAdditionalResources { get; set; }
    }
}
