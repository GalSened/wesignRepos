using Common.Interfaces.License;
using Common.Models.License;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Models.License
{
    public class LicenseInfoReponse
    {
        public IWeSignLicense LicenseLimits { get; set; }
        public LicenseCounters LicenseUsage { get; set; }
    }
}
