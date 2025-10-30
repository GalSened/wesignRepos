namespace Common.Interfaces.DB
{
    using Common.Enums.Documents;
    using Common.Enums.PDF;
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Documents.Signers;
    using Common.Models.ManagementApp.Reports;
    using Common.Models.Reports;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDocumentCollectionConnector
    {
        Task Create(DocumentCollection documentCollection);
        Task<DocumentCollection> Read(DocumentCollection documentCollection);
        Task<DeletedDocumentCollection> Read(DeletedDocumentCollection documentCollection);
        Task<DocumentCollection> ReadWithTemplateInfo(DocumentCollection documentCollection);
        Task<DocumentCollection> ReadBySignerId(Signer signer);
        IEnumerable<UsageByUserReport> ReadUsageByUserDetails(string userEmail, Company company, IEnumerable<Guid> groupIds, DateTime from, DateTime to);
        IEnumerable<UsageByCompanyReport> ReadUsageByCompanyAndGroups(Company company, IEnumerable<Guid> groupIds, DateTime from, DateTime to);
        Task<UsageBySignatureTypeReport> ReadUsageByCompanyAndSignatureTypes(Company company, IEnumerable<SignatureFieldType> signatureTypes, DateTime from, DateTime to);
        IEnumerable<DocumentCollection> Read(User user, string key, bool sent, bool viewed, bool signed, bool declined, bool sendingFailed, bool canceled, string from, string to, int offset, int limit,  out int totalCount,  bool isDistribution = false, Guid distributionId = default, SearchParameter searchParameter = SearchParameter.DocumentName);

        IEnumerable<DocumentCollection> ReadDistribution(User user, string key, int offset, int limit, out int totalCount);

        IEnumerable<DocumentCollection> ReadByStatusAndDate(Company company, DateTime notifyWhatBeforeDate);
        IEnumerable<DocumentCollection> Read(Company company);

        IEnumerable<DocumentCollection> Read(HashSet<Guid> companyIds);
        IEnumerable<DocumentCollection> ReadUnsigned();
        Task Update(DocumentCollection documentCollection);
        Task Delete(DocumentCollection documentCollection);
       // void DeleteDermanently(DocumentCollection documentCollection);
        Task Delete(DocumentCollection documentGroup, Action<DocumentCollection> deleteAction);
        
        Task<bool> Exists(DocumentCollection documentCollection);
        Task UpdateSignerSendingTime(DocumentCollection documentCollection, Signer signer);

        IEnumerable<DocumentCollection> ReadBySignerEmail(string email, string key, int offset, int limit, out int totalCount);
        Task UpdateStatus(DocumentCollection documentCollection, DocumentStatus documentStatus);
        IEnumerable<DocumentCollection> Read(Group group);
        IEnumerable<DocumentCollection> ReadByGroups(IEnumerable<Group> groups);
        IEnumerable<DocumentCollection> ReadDeletedCollections();
    
        IEnumerable<DocumentCollection> ReadCollectionsInList(List<Guid> ids);
        Task UpdateStatusAllDistributionCollenttion(Guid distributionId, DocumentStatus documentStatus);
        IEnumerable<DocumentCollection> ReadByDistributionId(User user, Guid distributionId, int offset, int limit, out int totalCount);
        Task ReadDistributionItemCounters(User user, Guid distributionId, Dictionary<DocumentStatus, int> statusCounts);
        IEnumerable<DocumentCollection> ReadDocumentsCollectionToDeleteByInterval(Company company, int signedInterval, int unsignedInterval);
        Task<bool> ExistNotDeletedCollectionsInGroup(Group group);
        Task DeleteAllUndeletedCollectionsInGroup(Group group);
        Task DocumentDeclined(DocumentCollection dbDocumentCollection, Signer dbSigner);
        Task ReactivateCollection(DocumentCollection documentCollection, List<Guid> signersId);
        IEnumerable<UsageDataReport> ReadUserUsageDataReports(Guid userId, DateTime from, DateTime to, List<DocumentStatus> documentStatuses, List<Guid> groupIds, bool includeDistributionDocs);
        Task<IEnumerable<DocumentCollection>> ReadDocumentsForRemainder(Company company);
    }
}
