using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IDater
    {
        DateTime UtcNow();
        DateTime MinValue();
    }
}
