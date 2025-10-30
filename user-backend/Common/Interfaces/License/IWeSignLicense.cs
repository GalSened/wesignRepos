using Common.Models.License;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.License
{
    public interface IWeSignLicense
    {
        DateTime ExpirationTime { get; set; }
        LicenseCounters LicenseCounters { get; set; }
        UIViewLicense UIViewLicense { get; set; }
    }
}
