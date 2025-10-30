namespace Common.Interfaces.DB
{
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Documents.Signers;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDocumentConnector
    {
        Task<Document> Read(Template template);
        Task<Document> Read(Document document);
        Task<bool> DocumentExist(Template template);
        IEnumerable<Document> ReadDocumentsById(List<Guid> guids);
        IEnumerable<DocumentSignatureField> ReadSignatures(Document document);
        IEnumerable<DocumentSignatureField> ReadSignaturesByDocumentsId(List<Guid> documentsIds);

    }
}
