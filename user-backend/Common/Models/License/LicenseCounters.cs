using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.License
{
    public class LicenseCounters
    {
        public long Templates { get; set; }
        public long Users { get; set; }
        public long DocumentsPerMonth { get; set; }
        public long SmsPerMonth { get; set; }
        public long VisualIdentificationsPerMonth { get; set; }  
        public bool UseActiveDirectory { get; set; }


    }
}
