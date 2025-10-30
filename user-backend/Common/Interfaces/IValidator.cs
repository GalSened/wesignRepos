using Common.Models;
using Common.Models.FileGateScanner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IValidator
    {  
        bool HasDuplication(IEnumerable<string> collection);
        Task ValidateEditorUserPermissions(User user);
        Task<FileGateScanResult> ValidateIsCleanFile(string base64string);
    }
}
