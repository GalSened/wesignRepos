using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using System.Threading.Tasks;

namespace Common.Interfaces.SignerApp
{
    public interface IDocumentModeAction
    {
        Task<string> DoAction(DocumentCollection dbDocumentCollection, Signer dbSigner );
    }
}
