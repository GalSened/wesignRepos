using Common.Consts;
using Common.Enums;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.PDF;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using Common.Models.ManagementApp.Reports;
using Common.Models.Reports;
using DAL.DAOs.Documents;
using DAL.DAOs.Documents.Signers;
using DAL.DAOs.Templates;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class DocumentCollectionConnector : IDocumentCollectionConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IDater _dater;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEncryptor _encryptor;
        private readonly ILogger _logger;
        private const int UNLIMITED = -1;

        public DocumentCollectionConnector(IWeSignEntities dbContext,
            IDater dater, IEncryptor encryptor, IServiceScopeFactory scopeFactory, ILogger logger)
        {
            _dbContext = dbContext;
            _dater = dater;
            _scopeFactory = scopeFactory;
            _encryptor = encryptor;
            _logger = logger;
        }

        public async Task Create(DocumentCollection documentCollection)
        {
            try
            {
                var documentGroupDAO = new DocumentCollectionDAO(documentCollection);
                List<Guid> templateIds = documentGroupDAO.Documents.Select(x => x.TemplateId).ToList();
                IEnumerable<TemplateDAO> templates = _dbContext.Templates.Where(x => templateIds.Contains(x.Id)).AsEnumerable();
                foreach (var document in documentGroupDAO.Documents)
                {
                    var template = templates.FirstOrDefault(x => x.Id == document.TemplateId);
                    document.Name = template != null ? template.Name : documentCollection.Name;
                }

                await _dbContext.DocumentCollections.AddAsync(documentGroupDAO);

                await _dbContext.SaveChangesAsync();

                documentCollection.Id = documentGroupDAO.Id;

                foreach (var document in documentCollection.Documents)
                {
                    document.Id = documentGroupDAO.Documents.FirstOrDefault(d => d.TemplateId == document.TemplateId).Id;
                }

                foreach (var signer in documentCollection.Signers)
                {
                    var signerDAOItems = documentGroupDAO.Signers.Where(d => d.ContactId == signer.Contact?.Id).ToList();
                    var signerDAO = signerDAOItems[0];
                    if (signerDAOItems.Count > 1)
                    {

                        signerDAO = documentGroupDAO.Signers.FirstOrDefault(d => d.ContactId == signer.Contact?.Id &&
                        !documentCollection.Signers.Any(a => a.Id == d.Id));
                    }
                    signer.Id = signerDAO.Id;
                    for (int i = 0; i < signerDAO.SignerFields.Count; i++)
                    {
                        signer.SignerFields.ElementAt(i).Id = signerDAO.SignerFields.ElementAt(i).Id;
                    }
                    for (int i = 0; i < signerDAO.SignerAttachments.Count; i++)
                    {
                        signer.SignerAttachments.ElementAt(i).Id = signerDAO.SignerAttachments.ElementAt(i).Id;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_Create = ");
                throw;
            }
        }

        public async Task Delete(DocumentCollection documentCollection)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();
                    await dependencyService.DocumentCollections.Where(x => x.Id == documentCollection.Id).ExecuteUpdateAsync(
                     setters => setters.SetProperty(x => x.Status, DocumentStatus.Deleted));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_DeleteByDocumentCollection = ");
                throw;
            }
        }

        public async Task Delete(DocumentCollection documentGroup, Action<DocumentCollection> deleteAction)
        {
            try
            {
                var documentGroupDAO = new DocumentCollectionDAO(documentGroup);
                if (documentGroupDAO.Signers != null && documentGroupDAO.Signers.Count > 0)
                {
                    await _dbContext.Signers.Where(x => documentGroupDAO.Signers.Select(x => x.Id).Contains(x.Id)).ExecuteDeleteAsync();
                }

                await _dbContext.DocumentCollections.Where(x => x.Id == documentGroupDAO.Id).ExecuteDeleteAsync();

                deleteAction.Invoke(documentGroup);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_DeleteByDocumentGroup&DeleteAction = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> ReadDeletedCollections()
        {
            try
            {
                var documentGroupDAO = _dbContext.DocumentCollections.Include(d => d.Documents)
                                                                .Include(d => d.Signers)
                                                                .ThenInclude(s => s.Contact)
                                                                .Include(d => d.Signers)
                                                                .ThenInclude(s => s.Notes)
                                                                .Include(d => d.User)
                                   .Where(d => d.Status == DocumentStatus.Deleted);

                return documentGroupDAO.Select(d => d.ToDocumentCollection()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadDeletedCollections = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> ReadCollectionsInList(List<Guid> ids)
        {
            try
            {
                return _dbContext.DocumentCollections.Include(d => d.Documents).Where(x => ids.Contains(x.Id)).Select(d => d.ToDocumentCollection()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadCollectionsInList = ");
                throw;
            }
        }

        public Task<bool> Exists(DocumentCollection documentCollection)
        {
            try
            {
                return _dbContext.DocumentCollections.AnyAsync(d => d.Id == documentCollection.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_Exists = ");
                throw;
            }
        }

        public async Task<DocumentCollection> ReadWithTemplateInfo(DocumentCollection documentCollection)
        {
            try
            {
                var documentCollectionDAO = await _dbContext.DocumentCollections.Include(d => d.Documents).
                        ThenInclude(x => x.Template).FirstOrDefaultAsync(x => x.Id == documentCollection.Id);
                return documentCollectionDAO.ToDocumentCollection();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadWithTemplateInfo = ");
                throw;
            }
        }

        public async Task<IEnumerable<DocumentCollection>> ReadDocumentsForRemainder(Company company)
        {
            try
            {
                var userHashSet = new HashSet<Guid>();
                (await _dbContext.Users.Where(x => x.CompanyId == company.Id).ToListAsync()).ForEach(a => userHashSet.Add(a.Id));

                var docCollectionDAO = _dbContext.DocumentCollections.Include(d => d.Signers)
                                                                    .ThenInclude(s => s.Contact)
                                                                    .Include(d => d.User)
                                                                    .ThenInclude(u => u.UserConfiguration).Where(x => userHashSet.Contains(x.UserId)
                                                                    && (x.Status == DocumentStatus.Created || x.Status == DocumentStatus.Sent ||
                                                                    x.Status == DocumentStatus.Viewed));
                return docCollectionDAO.Select(d => d.ToDocumentCollection());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadDocumentsForRemainder = ");
                throw;
            }
        }

        public async Task<DocumentCollection> Read(DocumentCollection documentCollection)
        {
            try
            {
                var documentCollectionDAO = await _dbContext.DocumentCollections.Include(d => d.Documents)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.Contact).ThenInclude(e => e.Seals)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.SignerFields)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.SignerAttachments)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.Notes)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.OtpDetails)
                                                            .Include(d => d.User)
                                                            .ThenInclude(u => u.UserConfiguration)
                                                            .FirstOrDefaultAsync(x => x.Id == documentCollection.Id);

                var result = documentCollectionDAO.ToDocumentCollection();

                if (result != null)
                {
                    Dictionary<Guid, Guid> documentTemplateMapper = new Dictionary<Guid, Guid>();

                    Dictionary<Guid, List<DocumentSignatureFieldDAO>> documentSignatureFieldsListDictionary =
                        new Dictionary<Guid, List<DocumentSignatureFieldDAO>>();

                    foreach (var signer in result.Signers)
                    {

                        foreach (var field in signer.SignerFields ?? Enumerable.Empty<SignerField>())
                        {
                            if (field.DocumentId == Guid.Empty)
                            {
                                continue;
                            }

                            if (!documentTemplateMapper.ContainsKey(field.DocumentId))
                            {
                                documentTemplateMapper.Add(field.DocumentId,
                                    result.Documents.FirstOrDefault(x => x.Id == field.DocumentId)?.TemplateId ?? Guid.Empty);

                                documentSignatureFieldsListDictionary.Add(field.DocumentId,
                                   await _dbContext.DocumentsSignatureFields.Where(x => x.DocumentId == field.DocumentId).ToListAsync());
                            }

                            field.TemplateId = documentTemplateMapper[field.DocumentId];

                            var documentsSignatureFields = documentSignatureFieldsListDictionary[field.DocumentId];

                            if (documentsSignatureFields != null)
                            {
                                var signatureField = documentsSignatureFields.FirstOrDefault(x => x.FieldName == field.FieldName);
                                if (signatureField != null)
                                {
                                    field.FieldValue = signatureField.Image;
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadByDocumentCollection = ");
                throw;
            }
        }

        public async Task<DeletedDocumentCollection> Read(DeletedDocumentCollection documentCollection)
        {
            try
            {
                var documentCollectionDAO = await _dbContext.DocumentCollections.Include(d => d.Documents)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.Contact).ThenInclude(e => e.Seals)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.SignerFields)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.SignerAttachments)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.Notes)
                                                            .Include(d => d.Signers)
                                                                .ThenInclude(x => x.OtpDetails)
                                                            .Include(d => d.User)
                                                            .ThenInclude(u => u.UserConfiguration)


                                                            .FirstOrDefaultAsync(x => x.Id == documentCollection.Id);
                var result = documentCollectionDAO.ToDeletedDocumentCollection();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadByDeletedDocumentCollection = ");
                throw;
            }
        }

        public async Task<DocumentCollection> ReadBySignerId(Signer signer)
        {
            try
            {
                var documentCollectionDAO = await _dbContext.DocumentCollections
                                                            .Include(d => d.Signers)
                                                            .Where(x => x.Signers.FirstOrDefault(s => s.Id == signer.Id) != null)
                                                            .FirstOrDefaultAsync();

                var result = documentCollectionDAO.ToDocumentCollection();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadBySignerId = ");
                throw;
            }
        }

        public async Task ReadDistributionItemCounters(User user, Guid distributionId, Dictionary<DocumentStatus, int> statusCounts)
        {
            try
            {
                if (statusCounts == null)
                {
                    return;
                }
                statusCounts.Clear();

                var counters = await _dbContext.DocumentCollections.Where(x => x.DistributionId == distributionId && x.GroupId == user.GroupId &&
                 (x.Status != DocumentStatus.Deleted || x.Status != DocumentStatus.Canceled)).GroupBy
                     (x => 1).Select(x =>
                    new
                    {
                        SendingFailed = x.Count(x => x.Status == DocumentStatus.SendingFailed),
                        Declined = x.Count(x => x.Status == DocumentStatus.Declined),
                        Signed = x.Count(x => x.Status == DocumentStatus.Signed),
                        ExtraServerSigned = x.Count(x => x.Status == DocumentStatus.ExtraServerSigned),
                        Sent = x.Count(x => x.Status == DocumentStatus.Sent),
                        Created = x.Count(x => x.Status == DocumentStatus.Created),
                        Viewed = x.Count(x => x.Status == DocumentStatus.Viewed),
                    }
                    ).SingleOrDefaultAsync();

                if (counters != null)
                {
                    statusCounts.Add(DocumentStatus.SendingFailed, counters.SendingFailed);
                    statusCounts.Add(DocumentStatus.Declined, counters.Declined);
                    statusCounts.Add(DocumentStatus.Signed, counters.Signed);
                    statusCounts.Add(DocumentStatus.ExtraServerSigned, counters.ExtraServerSigned);
                    statusCounts.Add(DocumentStatus.Created, counters.Created);
                    statusCounts.Add(DocumentStatus.Viewed, counters.Viewed);
                    statusCounts.Add(DocumentStatus.Sent, counters.Sent);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadDistributionItemCounters = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> ReadByDistributionId(User user, Guid distributionId, int offset, int limit, out int totalCount)
        {
            try
            {
                var documentCollectionNewItem = _dbContext.DocumentCollections.Include(d => d.Documents)
                                                           .Include(d => d.Signers)
                                                           .ThenInclude(x => x.Contact)
                                                           .Include(d => d.Signers)
                                                                .ThenInclude(x => x.SignerAttachments)
                                                           .Include(d => d.User).Where(x => x.DistributionId == distributionId && x.GroupId == user.GroupId &&
                                                           (x.Status != DocumentStatus.Deleted && x.Status != DocumentStatus.Canceled));

                totalCount = documentCollectionNewItem.Count();
                documentCollectionNewItem = limit != Consts.UNLIMITED ?
                        documentCollectionNewItem.Skip(offset).Take(limit) :
                        documentCollectionNewItem.Skip(offset);

                return documentCollectionNewItem.Select(x => x.ToDocumentCollection()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadByDistributionId = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> ReadDistribution(User user, string key, int offset, int limit, out int totalCount)
        {
            try
            {
                var query = _dbContext.DocumentCollections.Include(x => x.User).Where(x => x.GroupId == user.GroupId && x.DistributionId != Guid.Empty
            && x.Status != DocumentStatus.Deleted);

                if (!string.IsNullOrWhiteSpace(key))
                {
                    query = query.Where(x => x.Name.Contains(key));
                }

                query = query.GroupBy(x => x.DistributionId).Select(x => x.First());
                totalCount = query.Count();
                var result = query.AsEnumerable();
                result = result.OrderByDescending(x => x.CreationTime);
                result = limit != Consts.UNLIMITED ?
                        result.Skip(offset).Take(limit) :
                        result.Skip(offset);

                return result.Select(x => x.ToDocumentCollection()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadDistribution = ");
                throw;
            }
        }

        public IEnumerable<UsageByUserReport> ReadUsageByUserDetails(string userEmail, Company company, IEnumerable<Guid> groupIds, DateTime from, DateTime to)
        {
            try
            {
                var query = _dbContext.DocumentCollections
                .Where(dc => dc.CreationTime >= from && dc.CreationTime <= to);

                if (!string.IsNullOrEmpty(userEmail))
                {
                    query = query
                        .Include(dc => dc.User)
                        .Where(dc => dc.User.Email == userEmail);
                }

                if (company != null && company.Id != Guid.Empty)
                {
                    query = query
                        .Include(dc => dc.User)
                        .Where(dc => dc.User.CompanyId == company.Id);
                }

                if (groupIds != null && groupIds.Any())
                {
                    query = query
                        .Include(dc => dc.User)
                        .Where(dc => groupIds.Contains(dc.GroupId));
                }

                var groupedQuery = query
                    .Where(dc => dc.Status == DocumentStatus.Sent ||
                                         dc.Status == DocumentStatus.Signed ||
                                         dc.Status == DocumentStatus.Declined ||
                                         dc.Status == DocumentStatus.Canceled ||
                                         dc.Status == DocumentStatus.Deleted) 
                    .GroupBy(dc => new { dc.User.Company.Name, dc.GroupId, dc.User.Email });

                return groupedQuery.Select(g => new UsageByUserReport()
                {
                    CompanyName = g.Key.Name,
                    GroupId = g.Key.GroupId,
                    Email = g.Key.Email,
                    SentDocumentsCount = g.Count(d => d.Status == DocumentStatus.Sent),
                    SignedDocumentsCount = g.Count(d => d.Status == DocumentStatus.Signed),
                    DeclinedDocumentsCount = g.Count(d => d.Status == DocumentStatus.Declined),
                    CanceledDocumentsCount = g.Count(d => d.Status == DocumentStatus.Canceled),
                    DeletedDocumentsCount = g.Count(d => d.Status == DocumentStatus.Deleted)
                }).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadUsageByUserDetails = ");
                throw;
            }
        }

        public IEnumerable<UsageByCompanyReport> ReadUsageByCompanyAndGroups(Company company, IEnumerable<Guid> groupIds, DateTime from, DateTime to)
        {
            try
            {
                var query = _dbContext.DocumentCollections
                .Where(dc => dc.CreationTime >= from && dc.CreationTime <= to);

                if (company != null)
                {
                    query = query
                        .Include(dc => dc.User)
                        .Where(dc => dc.User.CompanyId == company.Id);
                }

                if (groupIds != null && groupIds.Any())
                {
                    query = query
                        .Include(dc => dc.User)
                        .Where(dc => groupIds.Contains(dc.GroupId));
                }

                var groupedQuery = query
                    .Where(dc => dc.Status == DocumentStatus.Sent ||
                                         dc.Status == DocumentStatus.Signed ||
                                         dc.Status == DocumentStatus.Declined ||
                                         dc.Status == DocumentStatus.Canceled)
                    .GroupBy(dc => new { dc.User.Company.Name, dc.GroupId });

                return groupedQuery.Select(g => new UsageByCompanyReport()
                {
                    CompanyName = g.Key.Name,
                    GroupId = g.Key.GroupId,
                    SentDocumentsCount = g.Count(d => d.Status == DocumentStatus.Sent),
                    SignedDocumentsCount = g.Count(d => d.Status == DocumentStatus.Signed),
                    DeclinedDocumentsCount = g.Count(d => d.Status == DocumentStatus.Declined),
                    CanceledDocumentsCount = g.Count(d => d.Status == DocumentStatus.Canceled)
                }).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadUsageByCompanyAndGroups = ");
                throw;
            }
        }

        public async Task<UsageBySignatureTypeReport> ReadUsageByCompanyAndSignatureTypes(Company company, IEnumerable<SignatureFieldType> signatureTypes, DateTime from, DateTime to)
        {
            try
            {
                var report = await _dbContext.DocumentCollections
            .Include(dc => dc.User)
            .Where(dc => dc.User.CompanyId == company.Id && dc.CreationTime >= from && dc.CreationTime <= to)
            .Include(dc => dc.Documents)
            .SelectMany(dc => dc.Documents)
            .Include(d => d.Template.TemplateSignatureFields)
            .SelectMany(d => d.Template.TemplateSignatureFields)
            .Where(tsf => signatureTypes == null || !signatureTypes.Any() || signatureTypes.Contains(tsf.SignaturFieldType))
            .GroupBy(tsf => tsf.Template.Documents.FirstOrDefault().DocumentCollection.User.Company.Name)
            .Select(g => new UsageBySignatureTypeReport
            {
                CompanyName = g.Key,
                ServerFieldsCount = g.Count(tsf => tsf.SignaturFieldType == SignatureFieldType.Server),
                GraphicFieldsCount = g.Count(tsf => tsf.SignaturFieldType == SignatureFieldType.Graphic),
                SmartCardFieldsCount = g.Count(tsf => tsf.SignaturFieldType == SignatureFieldType.SmartCard),
            }).FirstOrDefaultAsync();

                if (report != null)
                {
                    report.ServerFieldsCount = signatureTypes != null && !signatureTypes.Contains(SignatureFieldType.Server) ? -1 : report.ServerFieldsCount;
                    report.SmartCardFieldsCount = signatureTypes != null && !signatureTypes.Contains(SignatureFieldType.SmartCard) ? -1 : report.SmartCardFieldsCount;
                    report.GraphicFieldsCount = signatureTypes != null && !signatureTypes.Contains(SignatureFieldType.Graphic) ? -1 : report.GraphicFieldsCount;
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadUsageByCompanyAndSignatureTypes = ");
                throw;
            }
        }

        // ### TODO - change to enum list instead of bools
        public IEnumerable<DocumentCollection> Read(User user, string key, bool sent, bool viewed, bool signed, bool declined,
            bool sendingFailed, bool canceled, string from, string to, int offset, int limit, out int totalCount, bool isDistribution = false, Guid distributionId = default, SearchParameter searchParameter = SearchParameter.DocumentName)
        {
            try
            {
                var query = AddDocumentsContainTitle(key, user, isDistribution, searchParameter);

                if (distributionId != Guid.Empty)
                {
                    query = query.Where(x => x.DistributionId == distributionId);
                }

                if (!string.IsNullOrWhiteSpace(from))
                {
                    query = query.Where(t => t.CreationTime.Date >= DateTime.Parse(from).Date);
                }

                if (!string.IsNullOrWhiteSpace(to))
                {
                    query = query.Where(t => t.CreationTime.Date <= DateTime.Parse(to).Date);
                }

                query = query.Where(x => (x.Status == DocumentStatus.Created && sent && viewed && signed && declined && sendingFailed) ||
                                         (x.Status == DocumentStatus.Sent && sent) ||
                                         (x.Status == DocumentStatus.Viewed && viewed) ||
                                         ((x.Status == DocumentStatus.Signed || x.Status == DocumentStatus.ExtraServerSigned) && signed) ||
                                         (x.Status == DocumentStatus.Declined && declined) ||
                                         (x.Status == DocumentStatus.SendingFailed && sendingFailed) ||
                                         x.Status == DocumentStatus.Canceled && canceled);

                totalCount = query.Count();

                query = limit != UNLIMITED ?
                        query.OrderByDescending(x => x.CreationTime).Skip(offset).Take(limit) :
                        query.OrderByDescending(x => x.CreationTime).Skip(offset);

                return query.Select(x => x.ToDocumentCollection()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadByUser&Key&Sent&Viewed&Signed&Declined&Failed&Canceled = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> ReadByStatusAndDate(Company company, DateTime notifyWhatBeforeDate)
        {
            try
            {
                DateTime diff = notifyWhatBeforeDate.AddHours(-48);
                var documentCollectionDAO = _dbContext.DocumentCollections
                                             .Include(d => d.User).ThenInclude(c => c.UserConfiguration)
                                             .Where(x =>
                                             x.User.Company.Id == company.Id &&
                                             (x.Status == DocumentStatus.Viewed || x.Status == DocumentStatus.Sent ||
                                             x.Status == DocumentStatus.Created)
                                             && x.CreationTime < notifyWhatBeforeDate && x.CreationTime > diff)
                                             .OrderBy(x => x.CreationTime);

                List<DocumentCollection> result = new List<DocumentCollection>();

                if (documentCollectionDAO.Any())
                {
                    foreach (var item in documentCollectionDAO)
                    {
                        result.Add(item.ToDocumentCollection());
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadByStatusAndDate = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> ReadUnsigned()
        {
            try
            {
                var query = _dbContext.DocumentCollections
                    .Where(dc => dc.Status == DocumentStatus.Sent || dc.Status == DocumentStatus.Viewed)
                    .Select(dc => dc.ToDocumentCollection());
                return query.AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadUnsigned = ");
                throw;
            }
        }

        public async Task DocumentDeclined(DocumentCollection dbDocumentCollection, Signer dbSigner)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();

                    var dbCollection = await dependencyService.DocumentCollections.Include(x => x.Signers.Where(x => x.Id == dbSigner.Id)).
                            ThenInclude(x => x.Notes).AsTracking().FirstOrDefaultAsync(x => x.Id == dbDocumentCollection.Id);

                    if (dbCollection == null)
                    {
                        throw new InvalidOperationException(ResultCode.InvalidDocumentId.GetNumericString());
                    }

                    dbCollection.Status = dbDocumentCollection.DocumentStatus;
                    if (dbCollection.Signers.FirstOrDefault() != null)
                    {
                        dbCollection.Signers.FirstOrDefault().Notes.SignerNote = dbSigner.Notes.SignerNote;

                        dbCollection.Signers.FirstOrDefault().Status = dbSigner.Status;
                        dbCollection.Signers.FirstOrDefault().TimeRejected = dbSigner.TimeRejected;
                        dbCollection.Signers.FirstOrDefault().IPAddress = dbSigner.IPAddress;
                        dbCollection.Signers.FirstOrDefault().DeviceInformation = dbSigner.DeviceInformation;
                    }

                    await dependencyService.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_DocumentDeclined = ");
                throw;
            }
        }

        public async Task ReactivateCollection(DocumentCollection documentCollection, List<Guid> signersId)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();

                    var dbCollection = await dependencyService.DocumentCollections.Include(x => x.Signers.Where(x => signersId.Contains(x.Id))).ThenInclude(x => x.Notes).Include(x =>
                    x.Signers.Where(x => signersId.Contains(x.Id))).ThenInclude(x => x.OtpDetails).
                    AsTracking().FirstOrDefaultAsync(x => x.Id == documentCollection.Id);

                    dbCollection.Status = DocumentStatus.Sent;
                    Parallel.ForEach(dbCollection.Signers, rejectedSigner =>
                    {
                        rejectedSigner.Status = SignerStatus.Sent;
                        rejectedSigner.TimeRejected = _dater.MinValue();
                        rejectedSigner.TimeSent = _dater.UtcNow();
                        rejectedSigner.TimeViewed = _dater.MinValue();
                        rejectedSigner.TimeLastSent = _dater.UtcNow();
                        rejectedSigner.Notes.SignerNote = string.Empty;

                        if (rejectedSigner.OtpDetails != null)
                        {
                            rejectedSigner.OtpDetails.Attempts = 0;
                        }

                    });

                    await dependencyService.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReactivateCollection = ");
                throw;
            }
        }

        public Task Update(DocumentCollection documentCollection)
        {
            try
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();

                return strategy.ExecuteAsync(async
                     () =>
                {
                    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var documentCollectionDAO = _dbContext.DocumentCollections.Local.FirstOrDefault(d => d.Id == documentCollection.Id) ??
                                                   await _dbContext.DocumentCollections.Include(d => d.Documents).
                                                   FirstOrDefaultAsync(d => d.Id == documentCollection.Id);

                            await UpdateSignersInCollection(documentCollection);

                            documentCollectionDAO.GroupId = documentCollection.GroupId;

                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();

                                var signers =
                                    dependencyService.Signers.Include(x => x.SignerFields).Where(x => x.DocumentCollectionId == documentCollection.Id);
                                await SaveSignatureImages(documentCollection, signers);
                                dependencyService.Signers.UpdateRange(signers);
                                await dependencyService.SaveChangesAsync();
                            }

                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();
                                var documentCollectionDAOToCommit = await dependencyService.DocumentCollections.
                                FirstOrDefaultAsync(d => d.Id == documentCollection.Id);
                                documentCollectionDAOToCommit.GroupId = documentCollection.GroupId;
                                documentCollectionDAOToCommit.Status = documentCollection.DocumentStatus;
                                documentCollectionDAOToCommit.SignedTime = documentCollection.SignedTime;
                                documentCollectionDAOToCommit.Name = documentCollection.Name;
                                documentCollectionDAOToCommit.ShouldEnableMeaningOfSignature = documentCollection.ShouldEnableMeaningOfSignature;
                                dependencyService.DocumentCollections.Update(documentCollectionDAOToCommit);
                                await dependencyService.SaveChangesAsync();
                            }

                            await transaction.CommitAsync();
                        }

                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_Update = ");
                throw;
            }
        }

       

        public async Task UpdateStatus(DocumentCollection documentCollection, DocumentStatus documentStatus)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();
                    documentCollection.DocumentStatus = documentStatus;
                    await dependencyService.DocumentCollections.Where(d => d.Id == documentCollection.Id).ExecuteUpdateAsync
                    (setters => setters.SetProperty(x => x.Status, documentStatus));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_UpdateStatus = ");
                throw;
            }
        }

        public async Task UpdateStatusAllDistributionCollenttion(Guid distributionId, DocumentStatus documentStatus)
        {
            try
            {
                if (await _dbContext.DocumentCollections.AnyAsync(x => x.DistributionId == distributionId))
                {
                    await _dbContext.DocumentCollections.Where(x => x.DistributionId == distributionId).ExecuteUpdateAsync
                    (setters => setters.SetProperty(x => x.Status, documentStatus));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_UpdateStatusAllDistributionCollenttion = ");
                throw;
            }
        }

        public Task<bool> ExistNotDeletedCollectionsInGroup(Group group)
        {
            try
            {
                return _dbContext.DocumentCollections.AnyAsync(x => x.GroupId == group.Id && x.Status != DocumentStatus.Deleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ExistNotDeletedCollectionsInGroup = ");
                throw;
            }
        }

        public Task DeleteAllUndeletedCollectionsInGroup(Group group)
        {
            try
            {
                return _dbContext.DocumentCollections.Where(x => x.GroupId == group.Id && x.Status != DocumentStatus.Deleted).ExecuteUpdateAsync(
                (setters => setters.SetProperty(x => x.Status, DocumentStatus.Deleted)));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_DeleteAllUndeletedCollectionsInGroup = ");
                throw;
            }
        }

        public async Task UpdateSignerSendingTime(DocumentCollection documentCollection, Signer signer)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();

                    var dbSigner = await dependencyService.Signers.AsTracking().FirstOrDefaultAsync(s => s.Id == signer.Id);
                    if (dbSigner != null)
                    {


                        DateTime dt = _dater.UtcNow();
                        if (dbSigner.TimeSent == null)
                        {
                            dbSigner.TimeSent = dt;
                        }
                        signer.TimeSent = dbSigner.TimeSent;
                        dbSigner.TimeLastSent = dt;
                        signer.TimeLastSent = dt;

                        await dependencyService.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_UpdateSignerSendingTime = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> ReadDocumentsCollectionToDeleteByInterval(Company company, int signedInterval, int unsignedInterval)
        {
            try
            {
                List<DocumentCollection> documentCollections = new List<DocumentCollection>();
                var goupsList = _dbContext.Groups.Where(x => x.CompanyId == company.Id);

                if (signedInterval > 0 || unsignedInterval > 0)
                {
                    var docCollectionDAO = _dbContext.DocumentCollections.Include(d => d.Documents).
                        Include(d => d.Signers)
                        .ThenInclude(s => s.Contact)
                        .Include(d => d.Signers)
                        .ThenInclude(s => s.Notes)
                        .Include(d => d.User).Where(x => goupsList.Select(x => x.Id).Contains(x.GroupId)
                         && ((x.Status == DocumentStatus.Signed && signedInterval > 0
                         && x.CreationTime.AddDays(signedInterval) <= _dater.UtcNow()) ||
                         (x.Status != DocumentStatus.Signed && unsignedInterval > 0
                         && x.CreationTime.AddDays(unsignedInterval) <= _dater.UtcNow())));
                    documentCollections = docCollectionDAO.Select(x => x.ToDocumentCollection()).ToList();
                }

                return documentCollections;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadDocumentsCollectionToDeleteByInterval = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> Read(Company company)
        {
            try
            {
                var userList = _dbContext.Users.Where(x => x.CompanyId == company.Id).Select(a => new Guid(a.Id.ToString()))
                     .ToList();


                var docCollectionDAO = _dbContext.DocumentCollections.Include(d => d.Documents).Include(d => d.Signers)
                                                                    .ThenInclude(s => s.Contact)
                                                                    .Include(d => d.Signers)
                                                                    .ThenInclude(s => s.Notes)
                                                                    .Include(d => d.User).Where(x => userList.Contains(x.UserId));
                return docCollectionDAO.Select(d => d.ToDocumentCollection()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadByCompany = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> Read(HashSet<Guid> companyIds)
        {
            try
            {
                var userHashSet = new HashSet<Guid>();
                _dbContext.Users.Where(x => companyIds.Contains(x.CompanyId)).ToList().ForEach(a => userHashSet.Add(a.Id));

                var docCollectionDAO = _dbContext.DocumentCollections.Include(d => d.Signers)
                                                                    .ThenInclude(s => s.Contact)
                                                                    .Include(d => d.User)
                                                                    .ThenInclude(u => u.UserConfiguration).Where(x => userHashSet.Contains(x.UserId));
                return docCollectionDAO.Select(d => d.ToDocumentCollection());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadByCompanyIds = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> Read(Group group)
        {
            try
            {
                var docCollectionDAO = _dbContext.DocumentCollections.Include(d => d.Documents).Include(d => d.Signers)
                                                                .ThenInclude(s => s.Contact)
                                                                .Include(d => d.Signers)
                                                                .ThenInclude(s => s.Notes)
                                                                .Include(d => d.User).Where(x => x.GroupId == group.Id);
                return docCollectionDAO.Select(d => d.ToDocumentCollection()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadByGroup = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> ReadByGroups(IEnumerable<Group> groups)
        {
            try
            {
                var groupIds = groups.Select(_ => _.Id).ToList();
                var docCollectionDAO = _dbContext.DocumentCollections.Include(_ => _.Documents).Include(_ => _.Signers)
                    .ThenInclude(_ => _.Contact)
                    .Include(_ => _.Signers)
                    .ThenInclude(_ => _.Notes)
                    .Include(_ => _.User).Where(_ => groupIds.Contains(_.GroupId));
                return docCollectionDAO.Select(_ => _.ToDocumentCollection());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadByGroups = ");
                throw;
            }
        }

        public IEnumerable<DocumentCollection> ReadBySignerEmail(string email, string key, int offset, int limit, out int totalCount)
        {
            try
            {
                var signers = _dbContext.Signers.Where(x => (x.Status == SignerStatus.Sent || x.Status == SignerStatus.Viewed) &&
                                                    (x.Contact.Email == email && x.SendingMethod == SendingMethod.Email) &&
                                                    ((_dbContext.DocumentCollections.Where(y => y.Id == x.DocumentCollectionId).FirstOrDefault().Mode != DocumentMode.OrderedGroupSign) ||
                                                    (x.Status == SignerStatus.Viewed || (x.Status == SignerStatus.Sent && x.TimeSent.HasValue && x.TimeSent > DateTime.MinValue)))
                                                    );



                var docCollectionDAO = string.IsNullOrWhiteSpace(key) ?
                                                    _dbContext.DocumentCollections.Include(d => d.Documents)
                                                                                  .Include(d => d.Signers)
                                                                                  .ThenInclude(s => s.Contact)
                                                                                  .Include(d => d.Signers)
                                                                                  .ThenInclude(s => s.Notes)
                                                                                  .Include(d => d.User)
                                                                                  .Where(x => x.Mode != DocumentMode.SelfSign &&
                                                                                             (x.Status == DocumentStatus.Created || x.Status == DocumentStatus.Sent || x.Status == DocumentStatus.Viewed || x.Status == DocumentStatus.SendingFailed) &&
                                                                                              x.Signers.Any(x => signers.Any(y => y.Id == x.Id))) :
                                                    _dbContext.DocumentCollections.Include(d => d.Documents)
                                                                                  .Include(d => d.Signers)
                                                                                  .ThenInclude(s => s.Contact)
                                                                                  .Include(d => d.Signers)
                                                                                  .ThenInclude(s => s.Notes)
                                                                                  .Include(d => d.User)
                                                                                  .Where(x => x.Name.Contains(key) &&
                                                                                              x.Mode != DocumentMode.SelfSign &&
                                                                                             (x.Status == DocumentStatus.Created || x.Status == DocumentStatus.Sent || x.Status == DocumentStatus.Viewed || x.Status == DocumentStatus.SendingFailed) &&
                                                                                              x.Signers.Any(x => signers.Any(y => y.Id == x.Id)));

                totalCount = docCollectionDAO.Count();

                return docCollectionDAO.OrderByDescending(x => x.CreationTime).Skip(offset).Take(limit).Select(d => d.ToDocumentCollection()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadBySignerEmail = ");
                throw;
            }
        }

        public IEnumerable<UsageDataReport> ReadUserUsageDataReports(Guid userId, DateTime from, DateTime to, List<DocumentStatus> documentStatuses, List<Guid> groupIds, bool includeDistributionDocs)
        {
            try
            {
                if (documentStatuses != null && documentStatuses.Contains(DocumentStatus.Sent))
                {
                    documentStatuses.Add(DocumentStatus.Viewed);
                }

                var reports = _dbContext.DocumentCollections.AsQueryable()
                    .Where(dc => dc.UserId == userId)
                    .Where(dc => dc.CreationTime >= from && dc.CreationTime <= to)
                    .Where(dc => documentStatuses == null || documentStatuses.Contains(dc.Status))
                    .Where(dc => groupIds == null || groupIds.Contains(dc.GroupId))
                    .Where(dc => !includeDistributionDocs ? dc.DistributionId == Guid.Empty : true)
                    .GroupBy(dc => dc.GroupId)
                    .Select(g => new UsageDataReport()
                    {
                        GroupId = g.Key,
                        PendingDocumentsCount = documentStatuses == null || documentStatuses.Contains(DocumentStatus.Sent) ?
                        g.Count(dc => dc.Status == DocumentStatus.Sent || dc.Status == DocumentStatus.Viewed) : -1,
                        SignedDocumentsCount = documentStatuses == null || documentStatuses.Contains(DocumentStatus.Signed) ?
                        g.Count(dc => dc.Status == DocumentStatus.Signed || dc.Status == DocumentStatus.ExtraServerSigned) : -1,
                        DeclinedDocumentsCount = documentStatuses == null || documentStatuses.Contains(DocumentStatus.Declined) ?
                        g.Count(dc => dc.Status == DocumentStatus.Declined) : -1,
                        CanceledDocumentsCount = documentStatuses == null || documentStatuses.Contains(DocumentStatus.Canceled) ?
                        g.Count(dc => dc.Status == DocumentStatus.Canceled) : -1,
                        DistributionDocumentsCount = includeDistributionDocs ?
                        g.Count(dc => dc.DistributionId != Guid.Empty) : -1
                    }).ToList();

                return reports;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DocumentCollectionConnector_ReadUserUsageDataReports = ");
                throw;
            }
        }

        #region Private Functions

        private async Task SaveSignatureImages(DocumentCollection documentCollection, IEnumerable<SignerDAO> signersDAO)
        {
            Dictionary<Guid, List<DocumentSignatureFieldDAO>> documentsSignatureFieldsListDictionary = new Dictionary<Guid, List<DocumentSignatureFieldDAO>>();
            if (documentCollection.Mode == DocumentMode.SelfSign)
            {
                var doc = documentCollection.Documents.FirstOrDefault();
                foreach (var field in doc.Fields?.SignatureFields ?? Enumerable.Empty<SignatureField>())
                {
                    var documentSignatureField = new DocumentSignatureFieldDAO
                    {
                        DocumentId = doc.Id,
                        FieldName = field.Name,
                        Image = field.Image
                    };
                    await UpdateOrCreateDocSignature(documentSignatureField, documentsSignatureFieldsListDictionary);
                }
                return;
            }

            Dictionary<Guid, List<TemplateSignatureFieldDAO>> templatesSignatureFieldsListDictionary =
                  new Dictionary<Guid, List<TemplateSignatureFieldDAO>>();

            foreach (var signer in documentCollection.Signers ?? Enumerable.Empty<Signer>())
            {
                foreach (var signerField in signer.SignerFields)
                {

                    if (!templatesSignatureFieldsListDictionary.ContainsKey(signerField.TemplateId))
                    {
                        List<TemplateSignatureFieldDAO> tempalteSignFields = await _dbContext.TemplatesSignatureFields.Where(x => x.TemplateId == signerField.TemplateId).ToListAsync();
                        templatesSignatureFieldsListDictionary.Add(signerField.TemplateId, tempalteSignFields);
                    }

                    TemplateSignatureFieldDAO templateSigField = templatesSignatureFieldsListDictionary[signerField.TemplateId]?.
                        Find(x => x.Name == signerField.FieldName);

                    if (templateSigField != null) // signer field is signature field
                    {
                        var signerFieldDAO = signersDAO.FirstOrDefault(x => x.Id == signer.Id)?.SignerFields.FirstOrDefault(x => x.FieldName == signerField.FieldName);
                        if (signerFieldDAO != null)
                        {
                            var documentSignatureField = new DocumentSignatureFieldDAO
                            {
                                DocumentId = signerFieldDAO.DocumentId,
                                FieldName = signerField.FieldName,
                                Image = signerField.FieldValue
                            };

                            await UpdateOrCreateDocSignature(documentSignatureField, documentsSignatureFieldsListDictionary);
                        }
                    }
                }
            }
        }

        private async Task UpdateOrCreateDocSignature(DocumentSignatureFieldDAO documentSignatureField, Dictionary<Guid, List<DocumentSignatureFieldDAO>> documentsSignatureFieldsListDictionary)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();

                if (!documentsSignatureFieldsListDictionary.ContainsKey(documentSignatureField.DocumentId))
                {
                    documentsSignatureFieldsListDictionary.Add(documentSignatureField.DocumentId,
                        await dependencyService.DocumentsSignatureFields.Where(x => x.DocumentId == documentSignatureField.DocumentId).ToListAsync());
                }

                DocumentSignatureFieldDAO dbDocumentsSignatureField = documentsSignatureFieldsListDictionary[documentSignatureField.DocumentId].Find(x => x.FieldName == documentSignatureField.FieldName);

                if (dbDocumentsSignatureField != null)
                {
                    if (dbDocumentsSignatureField.Image != documentSignatureField.Image)
                    {
                        dbDocumentsSignatureField.Image = documentSignatureField.Image;
                        dependencyService.DocumentsSignatureFields.Update(dbDocumentsSignatureField);
                        await dependencyService.SaveChangesAsync();
                    }
                }

                else
                {
                    await dependencyService.DocumentsSignatureFields.AddAsync(documentSignatureField);
                    await dependencyService.SaveChangesAsync();
                }
            }
        }

        private IQueryable<DocumentCollectionDAO> AddDocumentsContainTitle(string key, User user, bool isDistribution, SearchParameter searchParameter)
        {
            IQueryable<DocumentCollectionDAO> documentGroupDAO;
            documentGroupDAO = isDistribution ? _dbContext.DocumentCollections.Include(d => d.Documents)
                                                                .Include(d => d.Signers)
                                                                .ThenInclude(s => s.Contact)
                                                                .Include(d => d.Signers)
                                                                .ThenInclude(s => s.Notes)
                                                                .Include(d => d.Signers)
                                                                .ThenInclude(s => s.SignerAttachments)
                                                                .Include(d => d.User)
                                   .Where(d => (d.GroupId == user.GroupId) && d.DistributionId != Guid.Empty) :
                                   _dbContext.DocumentCollections.Include(d => d.Documents)
                                                                .Include(d => d.Signers)
                                                                .ThenInclude(s => s.Contact)
                                                                .Include(d => d.Signers)
                                                                .ThenInclude(s => s.Notes)
                                                                .Include(d => d.Signers)
                                                                .ThenInclude(s => s.SignerAttachments)
                                                                .Include(d => d.User)
                                   .Where(d => (d.GroupId == user.GroupId) && d.DistributionId == Guid.Empty);

            if (!string.IsNullOrEmpty(key))
            {
                switch (searchParameter)
                {
                    case SearchParameter.DocumentName:
                        documentGroupDAO = documentGroupDAO.Where(d => d.Name.Contains(key));
                        break;

                    case SearchParameter.SignerDetails:
                        documentGroupDAO = documentGroupDAO.Where(d => d.Signers.Any(s => s.Contact.Name.Contains(key) || s.Contact.Email.Contains(key) || s.Contact.Phone.Contains(key)));
                        break;

                    case SearchParameter.SenderDetails:
                        documentGroupDAO = documentGroupDAO.Where(d => d.User.Email.Contains(key) || d.User.Name.Contains(key));
                        break;
                }
            }

            return documentGroupDAO;
        }

        private void UpdateOtpDetails(SignerDAO signerDAO, Signer signer)
        {
            if (signerDAO.OtpDetails == null)
            {
                signerDAO.OtpDetails = new SignerOtpDetailsDAO(signer?.SignerAuthentication?.OtpDetails);
            }

            else
            {
                if (signer?.SignerAuthentication?.OtpDetails?.Identification != signerDAO.OtpDetails.Identification)
                {
                    signerDAO.OtpDetails.Identification = _encryptor.Encrypt(signer?.SignerAuthentication?.OtpDetails?.Identification);
                }

                signerDAO.OtpDetails.ExpirationTime = signer?.SignerAuthentication?.OtpDetails?.ExpirationTime ?? DateTime.MinValue;
                signerDAO.OtpDetails.Code = signer?.SignerAuthentication?.OtpDetails?.Code;
                signerDAO.OtpDetails.Mode = signer?.SignerAuthentication?.OtpDetails?.Mode ?? OtpMode.None;
                signerDAO.OtpDetails.Attempts = signer?.SignerAuthentication?.OtpDetails?.Attempts ?? 0;
            }
        }

        private async Task UpdateSignersInCollection(DocumentCollection documentCollection)
        {
            if (documentCollection.Signers == null || !documentCollection.Signers.Any())
            {
                return;
            }

            List<Guid> signersIds = documentCollection.Signers.Select(a => a.Id).ToList();
            signersIds = signersIds.Distinct().ToList();

            IEnumerable<SignerDAO> dbSigners = _dbContext.Signers.Include(x => x.SignerFields).Include(x => x.SignerAttachments).
                    Include(x => x.Notes).Include(x => x.OtpDetails).Where(x => signersIds.Contains(x.Id));

            using (var scope = _scopeFactory.CreateScope())
            {
                foreach (var signer in documentCollection.Signers ?? Enumerable.Empty<Signer>())
                {
                    var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();

                    var signerDAO = dbSigners.FirstOrDefault(x => x.Id == signer.Id);

                    if (signerDAO == null)
                    {
                        continue;
                    }

                    signerDAO.TimeSent = signer.TimeSent;
                    signerDAO.TimeViewed = signer.TimeViewed;
                    signerDAO.TimeSigned = signer.TimeSigned;
                    signerDAO.TimeRejected = signer.TimeRejected;
                    signerDAO.TimeLastSent = signer.TimeLastSent;
                    signerDAO.Status = signer.Status;
                    signerDAO.SendingMethod = signer.SendingMethod;

                    if (signer.SignerAuthentication != null)
                    {
                        if (signer.SignerAuthentication.OtpDetails != null)
                        {
                            signerDAO.OtpDetails.Mode = signer.SignerAuthentication.OtpDetails.Mode;
                            signerDAO.OtpDetails.Identification = signer.SignerAuthentication.OtpDetails.Identification;
                        }

                        signerDAO.AuthMode = signer.SignerAuthentication.AuthenticationMode;
                    }

                    signerDAO.IdentificationAttempts = signer.IdentificationAttempts;

                    foreach (var signerFieldDAO in signerDAO.SignerFields ?? Enumerable.Empty<SignerFieldDAO>())
                    {
                        signerFieldDAO.DocumentId = signer?.SignerFields.FirstOrDefault(x => x.Id == signerFieldDAO.Id)?.DocumentId ?? Guid.Empty;
                    }

                    signerDAO.DeviceInformation = signer?.DeviceInformation;
                    signerDAO.IPAddress = signer?.IPAddress;
                    signerDAO.FirstViewIPAddress = signer?.FirstViewIPAddress;
                    signerDAO.Notes.SignerNote = signer?.Notes?.SignerNote;
                    signerDAO.Notes.UserNote = signer?.Notes?.UserNote;

                    if (signerDAO.Contact == null)
                    {
                        signerDAO.Contact = new DAOs.Contacts.ContactDAO(signer?.Contact);
                    }

                    UpdateOtpDetails(signerDAO, signer);
                    dependencyService.Signers.Update(signerDAO);
                    await dependencyService.SaveChangesAsync();
                }
            }
        }
        #endregion
    }
}