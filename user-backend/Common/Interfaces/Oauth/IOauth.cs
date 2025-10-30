using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.Documents.SplitSignature;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Oauth
{
    public interface IOauth
    {

        string GetURLForStartAuthForEIdasFlow(SignerTokenMapping signerTokenMapping, string callBackUrl);
        Task<SplitDocumentProcess> ProcessAfterSignerAuth(IdentityFlow identityFlow, string callBackUrl);
        void SaveDataForEidasProcess(SignerTokenMapping signerTokenMapping, DocumentCollection inputDocumentCollection);
    }
}
