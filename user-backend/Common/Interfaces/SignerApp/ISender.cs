using Common.Enums;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using System.Threading.Tasks;

namespace Common.Interfaces.SignerApp
{
    public interface ISender
    {
        
        Task SendSigningLinkToNextSigner(DocumentCollection dbDocumentCollection, Configuration appConfiguration, CompanyConfiguration companyConfiguration, Signer nextSigner);
        Task SendEmailNotification(MessageType messageType, DocumentCollection dbDocumentCollection, Configuration appConfiguration, Signer dbSigner, CompanyConfiguration companyConfiguration);        
        Task<string> SendSignedDocument(DocumentCollection documentCollection, Configuration appConfiguration, Signer dbSigner, CompanyConfiguration companyConfiguration);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appConfiguration"></param>
        /// <param name="signer"></param>
        /// <param name="user"></param>
        /// <returns>
        /// signer means that code is sending to 
        /// </returns>
        Task<string> SendOtpCode(Configuration appConfiguration, Signer signer, User user, Company companyConfiguration);
        Task SendDocumentDecline(DocumentCollection dbDocumentCollection, Configuration appConfiguration, Signer signer, User user);        
    }
}
