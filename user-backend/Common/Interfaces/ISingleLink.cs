using Common.Models;
using Common.Models.Documents.Signers;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISingleLink
    {
        Task<SignerLink> Create(SingleLink singleLink);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="singleLink"></param>
        /// <param name="template"></param>
        /// <returns>
        /// IsSmsProviderSupportGloballySend
        /// </returns>
        Task<SignleLinkGetDataResult> GetData(SingleLink singleLink);
    }
}
