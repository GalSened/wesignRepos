using Common.Models.Documents.Signers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.SignerApp
{
    public interface IContacts
    {
        Task<IEnumerable<string>> ReadSignaturesImages(SignerTokenMapping signerTokenMapping);
        Task UpdateSignaturesImages(SignerTokenMapping signerTokenMapping, IEnumerable<string> signaturesImages);
    }
}
