using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Configuration
{
    public class InitConfigurationDTO
    {
        public bool EnableFreeTrailUsers { get; internal set; }
        public bool EnableTabletsSupport { get; internal set; }
        public bool EnableSigner1ExtraSigningTypes { get; internal set; }
        public bool ShouldUseReCaptchaInRegistration { get; internal set; }
        public bool ShouldUseSignerAuth { get; internal set; }
        public bool ShouldUseSignerAuthDefault { get; internal set; }
        public bool EnableShowSSOOnlyInUserUI { get; internal set; }
        
    }
}
