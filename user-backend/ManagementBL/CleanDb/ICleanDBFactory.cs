using System;
using System.Collections.Generic;
using System.Text;

namespace ManagementBL.CleanDb
{
    public interface ICleanDBFactory
    {
        IDeleter GetDeleter(Type type);
        
    }
}
