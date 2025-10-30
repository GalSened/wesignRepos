using Comda.License.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface ILicenseWrapper
    {
        ILicenseManager GetLicenseManager();
    }
}
