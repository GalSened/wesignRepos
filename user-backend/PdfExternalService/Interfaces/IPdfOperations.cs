
using Common.Models;
using PdfExternalService.Models;
using PdfExternalService.Models.DTO;

namespace PdfExternalService.Interfaces
{
    public interface IPdfOperations
    {
        //AuthResult LoginUser(User user);
        byte[] MergeFiles(FileMergeObject filesForMerge);
    }
}
