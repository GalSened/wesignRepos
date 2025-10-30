using Common.Interfaces.License;
using Common.Models.License;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManagementBL.Tests.TestClass
{
    public class WesignTestClassLicenseResultModel : IWeSignLicense
    {
        public DateTime ExpirationTime { get ; set ; }
        public LicenseCounters LicenseCounters { get ; set; }
        public UIViewLicense UIViewLicense { get ; set ; }
    }
}
