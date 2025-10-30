using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.FileGateScanner;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.SignerApp
{
    public interface ISignerValidator
    {
        Task<(Signer, Guid documentCollectionId)> ValidateSignerToken(SignerTokenMapping signerTokenMapping);
        bool AreAllFieldsExistsInDocuments(DocumentCollection documentCollection);
        bool AreAllFieldsBelongToSigner(Signer dbSigner, Signer signer, DocumentCollection inputDocumentCollection);
        bool AreAllMandatoryFieldsFilledIn(Signer dbSigner, Signer signer);
        bool AreDocumentsBelongToDocumentCollection(DocumentCollection dbDocumentCollection, DocumentCollection inputDocumentCollection);
        bool AreAllSignersSigned(IEnumerable<Signer> signers);
        Task<FileGateScanResult> ValidateIsCleanFile(string base64string);
    }
}
