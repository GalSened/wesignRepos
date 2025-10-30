using Common.Models.Documents.Signers;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models.Documents.SplitSignature;

namespace Common.Interfaces.SignerApp
{
    public interface ISignerIdentity
    {

        IdentityCreateFlowResult GetURLForStartAuthForEIdasFlow(SignerTokenMapping signerTokenMapping);
        Task<IdentityCreateFlowResult> CreateIdentityFlow(SignerTokenMapping signerTokenMapping);
        Task<IdentityCheckFlowResult> CheckIdentityFlow(SignerTokenMapping signerTokenMapping, string code);
        Task<SplitDocumentProcess> ProcessAfterSignerAuth(IdentityFlow identityFlow);
    }
}
