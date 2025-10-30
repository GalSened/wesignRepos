using Common.Models;
using Common.Models.Documents.Signers;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IDoneDocuments
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbDocumentCollection"></param>
        /// <param name="dbSigner"></param>
        /// <returns>Download link</returns>
        Task<string> DoneProcess(DocumentCollection dbDocumentCollection, Signer dbSigner);
        byte[] DownloadSmartCardDesktopClientInstaller();

    }
}
