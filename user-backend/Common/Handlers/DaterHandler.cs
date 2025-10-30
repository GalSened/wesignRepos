using Common.Interfaces;
using System;

namespace Common.Handlers
{
    public class DaterHandler : IDater
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        public DateTime MinValue()
        {
            return DateTime.MinValue;
        }
    }
}
