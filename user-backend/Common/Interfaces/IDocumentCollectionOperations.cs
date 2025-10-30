using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Common.Enums.Documents;
using Common.Enums;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IDocumentCollectionOperations
    {
        Task<IEnumerable<SignerLink>> SendLinkToSpecificSigner(DocumentCollection documentCollection, Signer signer, User user, CompanyConfiguration companyConfiguration, bool sendDoc, MessageType messageType, bool shouldGenerateNewGuid = true);

        Task SendDocumentLinkToSigner(DocumentCollection documentCollection, IEnumerable<SignerLink> links,
           User user, CompanyConfiguration companyConfiguration, Configuration appConfiguration, Signer signer, MessageType messageType);

        string UpdateMessage(SendingMethod sendingMethod, string message, string signerLink, string documentCollectionName);
    }
}
