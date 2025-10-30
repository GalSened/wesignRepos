using System;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IOTP
    {
        string GenerateCode(Guid token = default, string identification = null);
       Task<(string, Guid)> ValidatePassword(Guid token = default, string identification = null, bool incrementAttempts = false);
        Task<(bool, Guid)> IsValidCode(string code, Guid token = default, bool incrementAttempts = false);
    }
}
