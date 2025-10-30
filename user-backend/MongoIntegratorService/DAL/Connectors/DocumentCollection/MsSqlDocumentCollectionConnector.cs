using Microsoft.EntityFrameworkCore;
using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Interfaces.Connectors;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Common.Models.ManagementReports;
using HistoryIntegratorService.Common.Models.UserReports;
using HistoryIntegratorService.DAL.DAOs.Documents;
using HistoryIntegratorService.DAL.Extensions;
using HistoryIntegratorService.Requests;

namespace HistoryIntegratorService.DAL.Connectors.DocumentCollection
{
    public class MsSqlDocumentCollectionConnector : IWeSignDocumentCollectionConnector, IManagementDocumentCollectionConnector
    {
        private readonly IMsSqlConnector _dbContext;

        public MsSqlDocumentCollectionConnector(IMsSqlConnector dbContext)
        {
            _dbContext = dbContext;
        }

        #region DocumentCollection

        public IEnumerable<DeletedDocumentCollection> Read(DeletedDocumentCollectionRequest deletedDocRequest)
        {
            
            var query = _dbContext.DeletedDocumentCollections.Where(d => d.CreationTime >= deletedDocRequest.From && d.CreationTime <= deletedDocRequest.To);
            if (deletedDocRequest.UserId.HasValue)
            {
                query = query.Where(d => d.UserId == deletedDocRequest.UserId);
            }
            if (deletedDocRequest.GroupIds != null && deletedDocRequest.GroupIds.Any())
            {
                query = query.Where(d => deletedDocRequest.GroupIds.Contains(d.GroupId));
            }
            return query.Select(dc => dc.ToDeletedDocumentCollection()).AsEnumerable();
        }
        
        public void Create(DeletedDocumentCollection deletedDocRequest)
        {
            var docCollectionDAO = new DeletedDocumentCollectionDAO(deletedDocRequest);
            _dbContext.DeletedDocumentCollections.Add(docCollectionDAO);
            _dbContext.SaveChanges();
        }

        #endregion

        #region WeSign

        public IEnumerable<UsageDataReport> ReadUserUsageDataReports(UserUsageDataRequest request)
        {
            var reports = _dbContext.DeletedDocumentCollections
                .Where(dc => dc.UserId == request.UserId)
                .Where(dc => dc.CreationTime >= request.From && dc.CreationTime <= request.To)
                .Where(dc => request.DocumentStatuses == null || request.DocumentStatuses.Contains(dc.DocumentStatus))
                .Where(dc => request.GroupIds == null || request.GroupIds.Contains(dc.GroupId))
                .Where(dc => !request.IncludeDistributionDocs ? dc.DistributionId == Guid.Empty : true)
                .GroupBy(dc => dc.GroupId)
                .Select(g => new UsageDataReport()
                {
                    GroupId = g.Key,
                    SentDocumentsCount = request.DocumentStatuses == null || request.DocumentStatuses.Contains(DocumentStatus.Sent) ?
                    g.Count(dc => dc.DocumentStatus == DocumentStatus.Sent || dc.DocumentStatus == DocumentStatus.Viewed) : -1,
                    SignedDocumentsCount = request.DocumentStatuses == null || request.DocumentStatuses.Contains(DocumentStatus.Signed) ?
                    g.Count(dc => dc.DocumentStatus == DocumentStatus.Signed) : -1,
                    DeclinedDocumentsCount = request.DocumentStatuses == null || request.DocumentStatuses.Contains(DocumentStatus.Declined) ?
                    g.Count(dc => dc.DocumentStatus == DocumentStatus.Declined) : -1,
                    CanceledDocumentsCount = request.DocumentStatuses == null || request.DocumentStatuses.Contains(DocumentStatus.Canceled) ?
                    g.Count(dc => dc.DocumentStatus == DocumentStatus.Canceled) : -1,
                    DistributionDocumentsCount = request.IncludeDistributionDocs ?
                    g.Count(dc => dc.DistributionId != Guid.Empty) : -1,
                }).AsEnumerable();
            return reports;
        }

        #endregion

        #region WeSignManagement

        public IEnumerable<UsageByUserReport> ReadUsageByUserDetails(UsageByUserDetailsRequest request)
        {
            var query = _dbContext.DeletedDocumentCollections.AsQueryable()
                .Where(dc => dc.CreationTime >= request.From && dc.CreationTime <= request.To);
            
            if (!string.IsNullOrEmpty(request.Email))
            {
                query = query
                  
                    .Where(dc => dc.UserEmail == request.Email);
            }
            
            if (request.CompanyId != Guid.Empty)
            {
                query = query
                    
                    .Where(dc => dc.CompanyId == request.CompanyId);
            }

            if (request.GroupIds != null && request.GroupIds.Any()) 
            {
                query = query.Where(dc => request.GroupIds.Contains(dc.GroupId));
            }

            var groupedQuery = query
                .Where(dc => dc.DocumentStatus == DocumentStatus.Sent ||
                dc.DocumentStatus == DocumentStatus.Signed ||
                dc.DocumentStatus == DocumentStatus.Declined ||
                dc.DocumentStatus == DocumentStatus.Canceled)
                .GroupBy(dc => new { dc.CompanyName, dc.GroupId, dc.UserEmail });
            return groupedQuery.Select(g => new UsageByUserReport()
            {
                CompanyName = g.Key.CompanyName,
                GroupId = g.Key.GroupId,
                Email = g.Key.UserEmail,
                SentDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Sent),
                SignedDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Signed),
                DeclinedDocumentsCount = g.Count(dc => dc.DocumentStatus == DocumentStatus.Declined),
                CanceledDocumentsCount = g.Count(dc=> dc.DocumentStatus == DocumentStatus.Canceled)
            }).AsEnumerable();
        }

        public IEnumerable<UsageByCompanyReport> ReadUsageByCompanyAndGroups(UsageByCompanyAndGroupsRequest request)
        {
            var query = _dbContext.DeletedDocumentCollections
                .Where(dc => dc.CreationTime >= request.From && dc.CreationTime <= request.To);

            if (request.CompanyId != Guid.Empty)
            {
                query = query
                    
                    .Where(dc => dc.CompanyId == request.CompanyId);
            }

            if (request.GroupIds != null && request.GroupIds.Any())
            {
                query = query
                  
                    .Where(dc => request.GroupIds.Contains(dc.GroupId));
            }

            var groupedQuery = query
                .Where(dc => dc.DocumentStatus == DocumentStatus.Sent ||
                dc.DocumentStatus == DocumentStatus.Signed ||
                dc.DocumentStatus == DocumentStatus.Declined ||
                dc.DocumentStatus == DocumentStatus.Canceled)
                .GroupBy(dc => new { dc.CompanyName, dc.GroupId });
            return groupedQuery.Select(g => new UsageByCompanyReport()
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
            var report = _dbContext.DeletedDocumentCollections
                
                .Where(dc => dc.CompanyId == request.CompanyId && dc.CreationTime >= request.From && dc.CreationTime <= request.To)
                .Include(dc => dc.Documents)
                .SelectMany(dc => dc.Documents)
                .Include(d => d.Template.TemplateSignatureFields)
                .SelectMany(d => d.Template.TemplateSignatureFields)
                .Where(tsf => request.SignatureFieldTypes == null || !request.SignatureFieldTypes.Any() || request.SignatureFieldTypes.Contains(tsf.SignatureFieldType))
                .GroupBy(tsf => tsf.CompanyName)
                .Select(g => new UsageBySignatureTypeReport()
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

    }
}
