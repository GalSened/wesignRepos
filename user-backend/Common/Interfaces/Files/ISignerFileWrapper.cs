using Common.Enums;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Files
{
    public  interface ISignerFileWrapper
    {
        void CreateSharedAppendices(DocumentCollection documentCollection, Appendix appendix);
        void CreateSignerAppendices(Guid documentCollectionId, Signer signer, Appendix appendix);
        byte[] GetSmartCardDesktopClientInstaller();
        bool IsAttachmentExist(Signer signer);
        Dictionary<string, (FileType, byte[])> ReadAttachments(Signer dbSigner);
        IEnumerable<Appendix> ReadSharedAppendices(Guid documentCollectionId);
        IEnumerable<Attachment> ReadSharedAppendicesAsAttachment(Guid documentCollectionId);
        IEnumerable<Appendix> ReadSignerAppendices(Guid documentCollectionId, Guid signerId);
        IEnumerable<Attachment> ReadSignerAppendicesAsAttachment(Guid documentCollectionId, Guid signerId);
        void SaveSignerAttachment(Signer signer, SignerAttachment signerAttachment);
        IEnumerable<Attachment> ReadSignerAttachments(Signer signer);
    }
}
