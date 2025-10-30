using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.CleanDb
{
    public interface ICleanDBManager
    {
        Task StartCleanDBProcess();
    }
}
