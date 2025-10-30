using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface IAppendices 
    {
        void Create(DocumentCollection documentCollection);
        void Create(Guid documentCollectionId, Signer signer);
        IEnumerable<System.Net.Mail.Attachment> ReadForMail(Guid documentCollectionId);
        IEnumerable<System.Net.Mail.Attachment> ReadForMail(Guid documentCollectionId, Guid signerId);
        IEnumerable<Appendix> Read(Guid documentCollectionId);
        IEnumerable<Appendix> Read(Guid documentCollectionId, Guid signerId);
    }
}
