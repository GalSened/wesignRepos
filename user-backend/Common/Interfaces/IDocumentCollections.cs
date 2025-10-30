namespace Common.Interfaces
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Enums.Users;
    using Common.Models;
    using Common.Models.Configurations;
    using Common.Models.Documents;
    using Common.Models.Documents.Signers;
    using Common.Models.Files.PDF;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDocumentCollections
    {
        Task Create(DocumentCollection documentCollection, IEnumerable<SignerField> readOnlyFields);
        Task<DocumentCollection> Read(DocumentCollection documentCollection);
        Task<(IEnumerable<DocumentCollection>, int)> Read(string key, bool sent, bool viewed, bool signed, bool declined, bool sendingFailed, bool canceled,
            string from, string to, int offset, int limit , SearchParameter searchParameter = SearchParameter.DocumentName);
        Task<IEnumerable<DocumentCollection>> ReadByStatusAndDate(Company company, DateTime notifyWhatBeforeDate);
        Task Update(DocumentCollection documentCollection, IEnumerable<SignerField> readOnlyFields);
        Task Delete(DocumentCollection documentCollection);
        Task Cancel(DocumentCollection documentCollection);
        Task<IDictionary<Guid, (string name,string templateId, byte[] content)>> Download(DocumentCollection documentCollection);
        Task<IDictionary<Guid, (string name, string templateId, byte[] content)>> DownloadAllSelected(IEnumerable<DocumentCollection> documentCollections);
        Task<IEnumerable<SignerLink>> SendSignerLinks(DocumentCollection documentCollection);
        Task<Document> GetPageInfoByDocumentId(DocumentCollection documentCollection, int page);
        Task<Document> GetPagesInfoByDocumentId(DocumentCollection documentCollection, int offset, int limit);
        Task<(Document,string)> GetPagesCountByDocumentId(DocumentCollection documentCollection);
        Task<SignerLink> ResendDocument(DocumentCollection documentCollection);
        Task<List<SignerLink>> ReactivateDocument(DocumentCollection documentCollection);

        Task<IEnumerable<SignerLink>> GetDocumentCollectionSigningLinks(DocumentCollection documentCollection);

        Task ShareDocument(DocumentCollection documentCollection);
        Task<byte[]> ExportDocumentsCollection(bool sent, bool viewed, bool signed, bool declined, bool sendingFailed, bool canceled, Language language = Language.en);
        Task<byte[]> ExportDistributionDocumentsCollection(Language language);
        Task<(byte[] xml, byte[] csv)> ExportFieldsFromPdf(DocumentCollection documentCollection, bool xmlOnly);
        Task<PDFFields> ExportFieldsFromPdfData(DocumentCollection documentCollection);
        Task<(string name, byte[] content)> DownloadTrace(DocumentCollection documentCollection, int offset);
        Task<IDictionary<string, (FileType, byte[])>> DownloadAttachmentsForSigner(DocumentCollection documentCollection, Guid signerId);

        Task UpdateDb(DocumentCollection documentCollection, bool isFromDistributionMechanism = false);
        void UpdateDocumentPdfFile(DocumentCollection documentCollection, IEnumerable<SignerField> readOnlyFields, bool isLoaded = false);
        Task ReplaceSigner(DocumentCollection documentCollection, Guid oldSignerId);
        Task ExtraServerSigning(DocumentCollection documentCollection);
        Task<MessageInfo> BuildSignerLink(DocumentCollection documentCollection, CompanyConfiguration companyConfiguration, Configuration configuration);
        Task DeleteBatch(RecordsBatch documentBatch);
        Task<string> GetSenderLiveLink(DocumentCollection documentCollection, Guid signerId);
        Task<string> GetOcrHtmlFromImage(string base64Image);
    }
}
