using Common.Interfaces.PDF;
using Common.Models.Configurations;
using System.Threading.Tasks;

namespace PdfHandler.Signing
{
    public interface ISigning
    {
        Task<byte[]> Sign(SigningInfo signingInfo, bool useForAllFields = false);
        Task VerifyCredential(SigningInfo signingInfo);
    }
}
