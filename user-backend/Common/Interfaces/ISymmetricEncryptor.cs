using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISymmetricEncryptor
    {
        Task<string> EncryptString(string key, string plainText);
        
        Task<string> DecryptString(string key, string plainText);
    }
}
