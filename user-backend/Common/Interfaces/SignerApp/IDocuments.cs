namespace Common.Interfaces.SignerApp
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Documents.Signers;
    using Common.Models.Files.PDF;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDocuments
    {
        Task<DocumentCollectionData> GetDocumentCollectionData(string signerIP,  SignerTokenMapping signerTokenMapping, string exrtaAuthId = "");
        Task<(Document document,bool returnImages, PDFFields)> GetPagesInfoByDocumentId(SignerTokenMapping signerTokenMapping, Guid documentId, int offset, int limit, string code);
        Task<(string RedirectLink, string Downloadlink)> Update(SignerTokenMapping signerTokenMapping, DocumentCollection documentCollection, DocumentOperation operation, bool useForAllFields = false);
        Task<(IDictionary<Guid, (string name, byte[] content)>,string documentCollectionName)> Download(SignerTokenMapping signerTokenMapping);
        Task<Appendix> ReadAppendix(SignerTokenMapping signerTokenMapping, string appendixName);
        Task<DocumentCollectionHtmlData> GetDocumentCollectionHtmlData(SignerTokenMapping signerTokenMapping);
        Task<DocumentCollectionDataFlowInfo> GetDocumentCollectionDataFlowInfoForUser(Guid newGuid);
        Task<string> GetOcrHtmlFromImage(string base64Image);
        bool GetTemplateSignatureFieldMandatory(string fieldName);

    }
}
