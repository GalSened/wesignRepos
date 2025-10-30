using Common.Enums.License;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.ManagementApp
{
    public class GenerateLicenseKeyResponse
    {
        public LicenseStatus LicenseStatus { get; set; }
        public string License { get; set; }

        
    }
}
