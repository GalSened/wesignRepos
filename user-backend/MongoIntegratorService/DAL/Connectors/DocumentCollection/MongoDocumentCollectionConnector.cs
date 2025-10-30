using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Extensions;
using HistoryIntegratorService.Common.Interfaces.Connectors;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Common.Models.ManagementReports;
using HistoryIntegratorService.Common.Models.UserReports;
using HistoryIntegratorService.Requests;

namespace HistoryIntegratorService.DAL.Connectors.DocumentCollection
{
    public class MongoDocumentCollectionConnector : IWeSignDocumentCollectionConnector, IManagementDocumentCollectionConnector
    {
        private IMongoConnector _connector;
        private GeneralSettings _settings;
        private IMongoCollection<DeletedDocumentCollection>? _collection;

        public MongoDocumentCollectionConnector(IMongoConnector connector, IOptions<GeneralSettings> options)
        {
            _connector = connector;
            _settings = options.Value;
            _collection = _connector.GetCollection<DeletedDocumentCollection>(_settings.MongoDb.DocumentCollectionName);
            CreateOrUpdateIndexes();
        }

        #region DocumentCollection

        public IEnumerable<DeletedDocumentCollection> Read(DeletedDocumentCollectionRequest deletedDocRequest)
        {
            if (_collection == null)
            {
                return Enumerable.Empty<DeletedDocumentCollection>();
            }

            var filterBuilder = Builders<DeletedDocumentCollection>.Filter;
            var filter = filterBuilder.Empty;
            filter &= filterBuilder.Gte(d => d.CreationTime, deletedDocRequest.From) &
                filterBuilder.Lte(d => d.CreationTime, deletedDocRequest.To);
            if (deletedDocRequest.UserId.HasValue)
            {
                filter &= filterBuilder.Eq(d => d.UserId, deletedDocRequest.UserId.Value);
            }
            if (deletedDocRequest.CompanyId.HasValue)
            {
                filter &= filterBuilder.Eq(d => d.User.CompanyId, deletedDocRequest.CompanyId.Value);
            }
            if (deletedDocRequest.GroupIds != null)
            {
                filter &= filterBuilder.In(d => d.GroupId, deletedDocRequest.GroupIds);
            }

            return _collection.Find(filter)
                .ToEnumerable();
        }

        public void Create(DeletedDocumentCollection deletedDoc)
        {
            if (_collection == null)
            {
                return;
            }
            _collection.InsertOne(deletedDoc);
        }

        #endregion

        #region WeSign

        public IEnumerable<UsageDataReport> ReadUserUsageDataReports(UserUsageDataRequest request)
        {
            if (_collection == null)
            {
                return Enumerable.Empty<UsageDataReport>();
            }

            var filter = Builders<DeletedDocumentCollection>.Filter.Eq(dc => dc.UserId, request.UserId) &
                     Builders<DeletedDocumentCollection>.Filter.Gte(dc => dc.CreationTime, request.From) &
                     Builders<DeletedDocumentCollection>.Filter.Lte(dc => dc.CreationTime, request.To);

            if (request.DocumentStatuses != null && request.DocumentStatuses.Any())
            {
                filter &= Builders<DeletedDocumentCollection>.Filter.In(dc => dc.DocumentStatus, request.DocumentStatuses);
            }

            if (request.GroupIds != null && request.GroupIds.Any())
            {
                filter &= Builders<DeletedDocumentCollection>.Filter.In(dc => dc.GroupId, request.GroupIds);
            }

            if (!request.IncludeDistributionDocs)
            {
                filter &= Builders<DeletedDocumentCollection>.Filter.Eq(dc => dc.DistributionId, Guid.Empty);
            }
            var results = _collection
            .Find(filter)
            .ToList()
            .GroupBy(dc => dc.GroupId)
            .Select(g => new UsageDataReport()
            {
                GroupId = g.Key,
                SentDocumentsCount = request.DocumentStatuses == null || request.DocumentStatuses.Contains(DocumentStatus.Sent) ?
                    g.Count(dc => dc.DocumentStatus == DocumentStatus.Sent || dc.DocumentStatus == DocumentStatus.Viewed) : -1,
                SignedDocumentsCount = request.DocumentStatuses == null || request.DocumentStatuses.Contains(DocumentStatus.Signed) ?
                    g.Count(dc => dc.DocumentStatus == DocumentStatus.Signed || dc.DocumentStatus == DocumentStatus.ExtraServerSigned) : -1,
                DeclinedDocumentsCount = request.DocumentStatuses == null || request.DocumentStatuses.Contains(DocumentStatus.Declined) ?
                    g.Count(dc => dc.DocumentStatus == DocumentStatus.Declined) : -1,
                CanceledDocumentsCount = request.DocumentStatuses == null || request.DocumentStatuses.Contains(DocumentStatus.Canceled) ?
                    g.Count(dc => dc.DocumentStatus == DocumentStatus.Canceled) : -1,
                DistributionDocumentsCount = request.IncludeDistributionDocs ?
                    g.Count(dc => dc.DistributionId != Guid.Empty) : -1
            });
            return results;
        }

        #endregion

        #region WeSignManagement

        public IEnumerable<UsageByUserReport> ReadUsageByUserDetails(UsageByUserDetailsRequest request)
        {
            if (_collection == null)
            {
                return Enumerable.Empty<UsageByUserReport>();
            }

            var filter = Builders<DeletedDocumentCollection>.Filter.Gte(dc => dc.CreationTime, request.From) &
                         Builders<DeletedDocumentCollection>.Filter.Lte(dc => dc.CreationTime, request.To);

            if (!string.IsNullOrEmpty(request.Email))
            {
                filter &= Builders<DeletedDocumentCollection>.Filter.Eq(dc => dc.User.Email, request.Email);
            }

            if (request.CompanyId != Guid.Empty)
            {
                filter &= Builders<DeletedDocumentCollection>.Filter.Eq(dc => dc.User.CompanyId, request.CompanyId);
            }

            if (request.GroupIds != null && request.GroupIds.Any())
            {
                filter &= Builders<DeletedDocumentCollection>.Filter.In(dc => dc.GroupId, request.GroupIds);
            }

            var documents = _collection.Find(filter).ToList();

            return documents
                .Where(dc => dc.DocumentStatus == DocumentStatus.Sent ||
                             dc.DocumentStatus == DocumentStatus.Signed ||
                             dc.DocumentStatus == DocumentStatus.Declined ||
                             dc.DocumentStatus == DocumentStatus.Canceled)
                .GroupBy(dc => new { dc.User.CompanyName, dc.GroupId, dc.User.Email })
                .Select(g => new UsageByUserReport
                {
                    CompanyName = g.Key.CompanyName,
                    GroupId = g.Key.GroupId,
                    Email = g.Key.Email,
                    SentDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Sent),
                    SignedDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Signed),
                    DeclinedDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Declined),
                    CanceledDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Canceled)
                }).AsEnumerable();
        }

        public IEnumerable<UsageByCompanyReport> ReadUsageByCompanyAndGroups(UsageByCompanyAndGroupsRequest request)
        {
            if (_collection == null)
            {
                return Enumerable.Empty<UsageByCompanyReport>();
            }

            var filter = Builders<DeletedDocumentCollection>.Filter.Gte(dc => dc.CreationTime, request.From) &
                    Builders<DeletedDocumentCollection>.Filter.Lte(dc => dc.CreationTime, request.To);

            if (request.CompanyId != Guid.Empty)
            {
                filter &= Builders<DeletedDocumentCollection>.Filter.Eq(dc => dc.User.CompanyId, request.CompanyId);
            }

            if (request.GroupIds != null && request.GroupIds.Any())
            {
                filter &= Builders<DeletedDocumentCollection>.Filter.In(dc => dc.GroupId, request.GroupIds);
            }

            var documents = _collection.Find(filter).ToList();

            return documents
                .Where(dc => dc.DocumentStatus == DocumentStatus.Sent ||
                             dc.DocumentStatus == DocumentStatus.Signed ||
                             dc.DocumentStatus == DocumentStatus.Declined ||
                             dc.DocumentStatus == DocumentStatus.Canceled)
                .GroupBy(dc => new { dc.User.CompanyName, dc.GroupId })
                .Select(g => new UsageByCompanyReport
                {
                    CompanyName = g.Key.CompanyName,
                    GroupId = g.Key.GroupId,
                    SentDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Sent),
                    SignedDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Signed),
                    DeclinedDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Declined),
                    CanceledDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Canceled)
                }).AsEnumerable();

        }

        public UsageBySignatureTypeReport ReadUsageByCompanyAndSignatureTypes(UsageByCompanyAndSignatureTypesRequest request)
        {
            if (_collection == null)
            {
                return null;
            }
            var filter = Builders<DeletedDocumentCollection>.Filter.Eq(dc => dc.User.CompanyId, request.CompanyId) &
                     Builders<DeletedDocumentCollection>.Filter.Gte(dc => dc.CreationTime, request.From) &
                     Builders<DeletedDocumentCollection>.Filter.Lte(dc => dc.CreationTime, request.To);

            var documents = _collection.Find(filter).ToList();

            var report = documents
                .SelectMany(dc => dc.Documents)
                .SelectMany(d => d.Template.TemplateSignatureFields)
                .Where(tsf => request.SignatureFieldTypes == null || !request.SignatureFieldTypes.Any() || request.SignatureFieldTypes.Contains(tsf.SignatureFieldType))
                .GroupBy(tsf => tsf.CompanyName)
                .Select(g => new UsageBySignatureTypeReport
                {
                    CompanyName = g.Key,
                    ServerFieldsCount = g.Count(tsf => tsf.SignatureFieldType == SignatureFieldType.Server),
                    GraphicFieldsCount = g.Count(tsf => tsf.SignatureFieldType == SignatureFieldType.Graphic),
                    SmartCardFieldsCount = g.Count(tsf => tsf.SignatureFieldType == SignatureFieldType.SmartCard)
                }).FirstOrDefault();

            if (report != null)
            {
                report.ServerFieldsCount = request.SignatureFieldTypes != null && !request.SignatureFieldTypes.Contains(SignatureFieldType.Server) ? -1 : report.ServerFieldsCount;
                report.SmartCardFieldsCount = request.SignatureFieldTypes != null && !request.SignatureFieldTypes.Contains(SignatureFieldType.SmartCard) ? -1 : report.SmartCardFieldsCount;
                report.GraphicFieldsCount = request.SignatureFieldTypes != null && !request.SignatureFieldTypes.Contains(SignatureFieldType.Graphic) ? -1 : report.GraphicFieldsCount;
            }

            return report;
        }

        #endregion

        #region PRIVATE

        private void CreateOrUpdateIndexes()
        {
            if (_collection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidCollectionName.GetNumericString());
            }

            var indexKeysDefinitionBuilder = Builders<DeletedDocumentCollection>.IndexKeys;

            // Index definitions
            var compoundIndex = new CreateIndexModel<DeletedDocumentCollection>(
            Builders<DeletedDocumentCollection>.IndexKeys.Combine(
                Builders<DeletedDocumentCollection>.IndexKeys.Ascending(d => d.CreationTime),
                Builders<DeletedDocumentCollection>.IndexKeys.Ascending(d => d.UserId)
            ), new CreateIndexOptions { Name = "CreationTime_UserId_CompoundIndex" });

            CreateIndexIfNotExists(compoundIndex, _collection);
        }

        private void CreateIndexIfNotExists(CreateIndexModel<DeletedDocumentCollection> indexModel, IMongoCollection<DeletedDocumentCollection> collection)
        {
            var indexName = indexModel.Options.Name;

            if (IndexExists(indexName, collection))
            {
                // Drop the existing index if it exists
                collection.Indexes.DropOne(indexName);
            }

            // Create the new index
            collection.Indexes.CreateOne(indexModel);
        }

        private bool IndexExists<T>(string indexName, IMongoCollection<T> collection)
        {
            var existingIndexes = collection.Indexes.List();
            var existingIndexNames = existingIndexes.ToList();

            return existingIndexNames.Any(index => index["name"] == indexName);
        }

        #endregion
    }
}
