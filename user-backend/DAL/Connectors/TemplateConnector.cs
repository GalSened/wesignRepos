
using Common.Consts;
using Common.Enums.PDF;
using Common.Enums.Templates;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Files.PDF;
using Common.Models.ManagementApp.Reports;
using DAL.Extensions;
using DAL.DAOs.Templates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using Serilog;

namespace DAL.Connectors
{
    public class TemplateConnector : ITemplateConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IDater _dater;
        private readonly ILogger _logger;
        public TemplateConnector(IWeSignEntities dbContext, IDater dater, ILogger logger)
        {
            _dbContext = dbContext;
            _dater = dater;
            _logger = logger;
        }

        public Task Create(Template template)
        {
            try
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();

                return strategy.ExecuteAsync(async
                    () =>
                {
                    using var transaction = _dbContext.Database.BeginTransaction();
                    try
                    {
                        var templateDAO = new TemplateDAO(template);
                        await _dbContext.Templates.AddAsync(templateDAO);
                        await _dbContext.SaveChangesAsync();

                        template.Id = templateDAO.Id;
                        await CreateFields(template);
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_Create = ");
                throw;
            }

        }

        public Task Delete(Template template)
        {
            try
            {
                return _dbContext.Templates.Where(t => t.Id == template.Id).ExecuteUpdateAsync(
                   setters => setters.SetProperty(x => x.Status, TemplateStatus.Deleted)
                   );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_DeleteByTemplate = ");
                throw;
            }
        }

        public async Task Delete(List<Template> templates)
        {
            try
            {
                if (templates != null && templates.Count > 0)
                {
                    await _dbContext.Templates.Where(x => templates.Select(x => x.Id).Contains(x.Id)).ExecuteUpdateAsync(
                        setters => setters.SetProperty(x => x.Status, TemplateStatus.Deleted)
                        );
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_DeleteByTemplates = ");
                throw;
            }
        }

        public async Task Delete(List<Template> templates, Action<List<Template>> deleteTemplateFromFS)
        {

            try
            {
                await _dbContext.Templates.Where(x => templates.Select(x => x.Id).Contains(x.Id)).ExecuteDeleteAsync();
                await _dbContext.SingleLinkAdditionalResources.Where(x => templates.Select(x => x.Id).Contains(x.TemplateId)).ExecuteDeleteAsync();
                deleteTemplateFromFS.Invoke(templates);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_DeleteByTemplates&DeleteTemplateFromFS = ");
                throw;
            }
        }

        public Task<bool> Exists(Template template)
        {
            try
            {
                return _dbContext.Templates.AnyAsync(t => t.Id == template.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_Exists = ");
                throw;
            }
        }

        public IEnumerable<Template> Read(User user, string key, string from, string to, int offset, int limit, bool popular, bool recent, out int totalCount)
        {
            try
            {
                var query = AddTemplatesNamesContainKey(key, user);

                if (!string.IsNullOrWhiteSpace(from))
                {
                    query = query.Where(t => t.CreationTime.Date >= DateTime.Parse(from).Date);
                }
                if (!string.IsNullOrWhiteSpace(to))
                {
                    query = query.Where(t => t.CreationTime.Date <= DateTime.Parse(to).Date);
                }
                totalCount = query.Count();
                query = OrderQuery(popular, recent, query);
                query = limit != Consts.UNLIMITED ? query.Skip(offset).Take(limit) : query.Skip(offset);

                return query.Select(x => x.ToTemplate()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadByUser&Key&From&To&Offset&Limit&Popular&Recent = ");
                throw;
            }
        }

        public async Task<Template> Read(Template template)
        {
            try
            {
                var templateDAO = await _dbContext.Templates.Include(x => x.TemplateSignatureFields)
                                                       .Include(x => x.TemplateTextFields)
                                                       .FirstOrDefaultAsync(t => t.Id == template.Id);
                return templateDAO.ToTemplate();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadByTemplate = ");
                throw;
            }
        }

        public IEnumerable<Template> Read(List<Guid> Ids)
        {
            try
            {
                return _dbContext.Templates.Where(x => Ids.Contains(x.Id)).Select(x => x.ToTemplate()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadByIds = ");
                throw;
            }
        }

        public IEnumerable<Template> ReadWithFieldsData(List<Guid> Ids)
        {
            try
            {
                return _dbContext.Templates.Include(x => x.TemplateSignatureFields).Where(x => Ids.Contains(x.Id)).Select(x => x.ToTemplate()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadWithFieldsData = ");
                throw;
            }
        }

        public IEnumerable<Template> Read(Group group)
        {
            try
            {
                return _dbContext.Templates.Include(x => x.TemplateSignatureFields)
                                                        .Include(x => x.TemplateTextFields)
                                                        .Where(t => t.GroupId == group.Id).Select(t => t.ToTemplate()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadByGroup = ");
                throw;
            }
        }

        public IEnumerable<Template> ReadDeletedTemplates()
        {
            try
            {
                return _dbContext.Templates.Where(t => t.Status == TemplateStatus.Deleted).Select(t => t.ToTemplate()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadDeletedTemplates = ");
                throw;
            }
        }

        public bool ReadTemplateSignatureFieldMandatory(string fieldName)
        {
            try
            {
                return _dbContext.TemplatesSignatureFields.Where(sf => sf.Name == fieldName).Select(sf => sf.Mandatory).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadTemplateSignatureFieldMandatory = ");

                throw;
            }
        }

        public IEnumerable<TemplatesByUsageReport> ReadTemplatesByUsage(Guid companyId, IEnumerable<Guid> groupIds, DateTime from, DateTime to, int offset, int limit, out int totalCount)
        {
            try
            {
                IQueryable<TemplateDAO> query;
                if (groupIds != null && groupIds.Any())
                {
                    query = _dbContext.Templates
                        .Where(t => groupIds.Contains(t.GroupId));
                }
                else
                {
                    query = from template in _dbContext.Templates
                            join company in _dbContext.Companies on companyId equals company.Id
                            from g in company.Groups
                            where template.GroupId == g.Id
                            select template;
                }
                query = query.Where(t => t.CreationTime >= from && t.CreationTime <= to);
                totalCount = query.Count();
                query = query
                .Skip(offset)
                .Take(limit);
                var reports = query.Select(_ => new TemplatesByUsageReport()
                {
                    TemplateName = _.Name,
                    GroupId = _.GroupId,
                    UsageCount = _.UsedCount
                }).AsEnumerable();

                return reports;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadTemplatesByUsage = ");
                throw;
            }
        }

        public IEnumerable<Template> ReadOneTimeTemplatesWithoutConnectedDocuments()
        {
            try
            {
                return _dbContext.Templates.Where(c => c.Status == TemplateStatus.OneTimeUse && c.CreationTime.AddDays(2) < _dater.UtcNow()
                                                             && !_dbContext.Documents.Select(x => x.TemplateId).Contains(c.Id)).Select(x => x.ToTemplate());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadOneTimeTemplatesWithoutConnectedDocuments = ");
                throw;
            }
        }
        public IEnumerable<Template> ReadDeletedTemplatesWithoutConnectedDocuments()
        {
            try
            {
                return _dbContext.Templates.Where(c => c.Status == TemplateStatus.Deleted
                                                              && !_dbContext.Documents.Select(x => x.TemplateId).Contains(c.Id)).Select(x => x.ToTemplate());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadDeletedTemplatesWithoutConnectedDocuments = ");
                throw;
            }
        }

        public IEnumerable<Guid> ReadAllIds()
        {
            try
            {
                return _dbContext.Templates.Select(_ => _.Id).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadAllIds = ");
                throw;
            }
        }


        public Task<bool> ExistNotDeletedTemplatesInGroup(Group group)
        {
            try
            {
                return _dbContext.Templates.AnyAsync(x => x.GroupId == group.Id && x.Status != TemplateStatus.Deleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ExistNotDeletedTemplatesInGroup = ");
                throw;
            }
        }

        public Task DeleteUnDeletedTemplatesInGroup(Group group)
        {
            try
            {
                return _dbContext.Templates.Where(x => x.GroupId == group.Id && x.Status != TemplateStatus.Deleted).ExecuteUpdateAsync(
                  setters => setters.SetProperty(x => x.Status, TemplateStatus.Deleted));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_DeleteUnDeletedTemplatesInGroup = ");
                throw;
            }
        }

        public Task RemoveAllOneTimeTemplatesFromCollectionGroup(DocumentCollection documentGroup)
        {
            try
            {
                return _dbContext.Templates.Where(x => documentGroup.Documents.Select(x => x.TemplateId).Contains(x.Id) &&
            x.Status == TemplateStatus.OneTimeUse).ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.Status, TemplateStatus.Deleted));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_RemoveAllOneTimeTemplatesFromCollectionGroup = ");
                throw;
            }
        }

        public IEnumerable<Template> ReadOneTimeTemplates()
        {
            try
            {
                return _dbContext.Templates.Include(x => x.TemplateSignatureFields)
                                                           .Include(x => x.TemplateTextFields)
                                                           .Where(t => t.Status == TemplateStatus.OneTimeUse && t.CreationTime.AddDays(2) <
                                                           _dater.UtcNow()).Select(t => t.ToTemplate()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadOneTimeTemplates = ");
                throw;
            }
        }

        public Task UpdateLastUsed(Template template)
        {
            try
            {
                return _dbContext.Templates.Where(t => t.Id == template.Id).ExecuteUpdateAsync(
                 setters => setters.SetProperty(x => x.LastUsedTime, template.LastUsedTime)
                 );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_UpdateLastUsed = ");
                throw;
            }
        }

        public async Task Update(Template template)
        {
            try
            {
                var templateDAO = _dbContext.Templates.Local.FirstOrDefault(t => t.Id == template.Id) ??
                             await _dbContext.Templates.FirstOrDefaultAsync(t => t.Id == template.Id);
                if (templateDAO == null)
                {
                    throw new Exception($"Failed to find the template [{templateDAO.Id}] in db");
                }
                var strategy = _dbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async
                    () =>
                {
                    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            templateDAO.Name = !string.IsNullOrWhiteSpace(template.Name) ? template.Name : templateDAO.Name;
                            templateDAO.LastUpdatetime = _dater.UtcNow();
                            templateDAO.LastUsedTime = _dater.UtcNow();
                            templateDAO.UsedCount = template.UsedCount;
                            templateDAO.GroupId = template.GroupId;
                            _dbContext.Templates.Update(templateDAO);
                            await _dbContext.SaveChangesAsync();

                            await DeleteFields(template);
                            await CreateFields(template);

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
                _logger.Error(ex, "Error in TemplateConnecter_Update = ");
                throw;
            }
        }

        public IEnumerable<TextField> GetTextFieldsByType(Template template, TextFieldType textFieldType)
        {
            try
            {
                return _dbContext.TemplatesTextFields.Where(x => x.TemplateId == template.Id && x.TextFieldType == textFieldType).Select(x => x.ToTextFields()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_GetTextFieldsByType = ");
                throw;
            }
        }

        public async Task IncrementUseCount(Template template, int delta)
        {
            try
            {
                var templateDAO = _dbContext.Templates.Local.FirstOrDefault(t => t.Id == template.Id) ??
                         await _dbContext.Templates.FirstOrDefaultAsync(t => t.Id == template.Id);
                if (templateDAO == null)
                {
                    throw new Exception($"Failed to find the template [{templateDAO.Id}] in db");
                }
                var strategy = _dbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async
                    () =>
                {
                    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            templateDAO.UsedCount += delta;
                            templateDAO.LastUsedTime = _dater.UtcNow();
                            _dbContext.Templates.Update(templateDAO);
                            await _dbContext.SaveChangesAsync();
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
                _logger.Error(ex, "Error in TemplateConnecter_IncrementUseCount = ");
                throw;
            }
        }

        public List<SingleLinkAdditionalResource> ReadSingleLink(Template template)
        {
            try
            {
                return _dbContext.SingleLinkAdditionalResources.Where(t => t.TemplateId == template.Id).Select(x => x.ToSingleLinkAdditionalResource()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_ReadSingleLink = ");
                throw;
            }
        }


        public async Task UpdateCreateSingleLink(TemplateSingleLink resource)
        {
            try
            {
                await _dbContext.SingleLinkAdditionalResources.Where(t => t.TemplateId == resource.TemplateId).ExecuteDeleteAsync();
                await _dbContext.SingleLinkAdditionalResources.AddRangeAsync(resource.SingleLinkAdditionalResources.Select(x =>

                new SingleLinkAdditionalResourceDAO
                {
                    IsMandatory = x.IsMandatory,
                    TemplateId = resource.TemplateId,
                    Data = x.Data,
                    Type = x.Type
                }
                ).ToList());

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_UpdateCreateSingleLink = ");
                throw;
            }
        }

        public Task DeleteUnusedTemplates(DateTime dateTime)
        {
            try
            {
                return _dbContext.Templates.Where(c => c.LastUsedTime <= dateTime && c.Status == TemplateStatus.MultipleUse
                                                              && !_dbContext.Documents.Select(x => x.TemplateId).Contains(c.Id)).ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.Status, TemplateStatus.Deleted));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_DeleteUnusedTemplates = ");
                throw;
            }
        }

        public List<Template> GetUnusedTemplates(DateTime dateTime)
        {
            try
            {
                var templets = _dbContext.Templates.Include(x => x.Documents).Where(x => x.LastUsedTime <= dateTime &&
                x.Status == TemplateStatus.MultipleUse);
                templets = templets.Where(x => x.Documents == null || x.Documents.Count == 0); ;
                return templets.Select(t => t.ToTemplate()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TemplateConnecter_GetUnusedTemplates = ");
                throw;
            }
        }

        #region Private Functions

        private async Task DeleteFields(Template template)
        {

            await _dbContext.TemplatesTextFields.Where(t => t.TemplateId == template.Id).ExecuteDeleteAsync();
            await _dbContext.TemplatesSignatureFields.Where(t => t.TemplateId == template.Id).ExecuteDeleteAsync();

        }

        private async Task CreateFields(Template template)
        {
            foreach (SignatureField field in template.Fields.SignatureFields)
            {
                await _dbContext.TemplatesSignatureFields.AddAsync(new TemplateSignatureFieldDAO()
                {
                    TemplateId = template.Id,
                    SignaturFieldType = field.SigningType,
                    Name = field.Name,
                    SignatureKind = field.SignatureKind,
                    Mandatory = field.Mandatory
                });
            }

            foreach (TextField field in template.Fields.TextFields)
            {
                await _dbContext.TemplatesTextFields.AddAsync(new TemplateTextFieldDAO()
                {
                    TemplateId = template.Id,
                    Name = field.Name,
                    TextFieldType = field.TextFieldType,
                    Regex = field.CustomerRegex
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        private IQueryable<TemplateDAO> OrderQuery(bool popular, bool recent, IQueryable<TemplateDAO> query)
        {
            if (popular && recent)
            {
                return query.OrderBy(x => x.UsedCount).ThenBy(x => x.LastUpdatetime);
            }
            if (popular)
            {
                return query.OrderBy(x => x.UsedCount);
            }
            if (recent)
            {
                return query.OrderBy(x => x.LastUpdatetime);
            }

            return query.OrderByDescending(x => x.CreationTime);
        }

        private IQueryable<TemplateDAO> AddTemplatesNamesContainKey(string key, User user)
        {
            IQueryable<TemplateDAO> query;
            query = string.IsNullOrWhiteSpace(key) ?
                           _dbContext.Templates.Where(t => t.GroupId == user.GroupId) :
                           _dbContext.Templates.Where(t => t.GroupId == user.GroupId && t.Name.Contains(key));
            query = query.Where(x => x.Status == TemplateStatus.MultipleUse);
            return query;
        }



        #endregion
    }
}