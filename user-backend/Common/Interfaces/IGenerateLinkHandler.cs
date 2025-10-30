using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IGenerateLinkHandler
    {
        Task<IEnumerable<SignerLink>> GenerateSigningLink(DocumentCollection documentCollection, User user, CompanyConfiguration companyConfiguration, bool shouldGenerateNewGuid = true);
        Task<SignerLink> GenerateDocumentDownloadLink(DocumentCollection documentCollection, Signer signer, User user, CompanyConfiguration companyConfiguration);
        Task<SignerLink> GenerateSigningLinkToSingleSigner(DocumentCollection documentCollection, bool shouldGenerateNewGuid, int expirationTimeInHours, Configuration appConfiguration, Signer signer);    }
}
