using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.ManagementApp
{
    public interface ILicenseDMZ
    {
        Task<bool> IsDMZReachable();
    }
}
