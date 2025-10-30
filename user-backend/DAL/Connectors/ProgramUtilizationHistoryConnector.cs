using Common.Consts;
using Common.Interfaces.DB;
using Common.Models.Programs;
using DAL.DAOs.Programs;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class ProgramUtilizationHistoryConnector : IProgramUtilizationHistoryConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ILogger _logger;
        public ProgramUtilizationHistoryConnector(IWeSignEntities dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Create(ProgramUtilizationHistory programUtilizationHistory)
        {
            try
            {
                var programUtilizationHistoryDAO = new ProgramUtilizationHistoryDAO(programUtilizationHistory);
                await _dbContext.ProgramUtilizationHistories.AddAsync(programUtilizationHistoryDAO);
                await _dbContext.SaveChangesAsync();

                programUtilizationHistory.Id = programUtilizationHistoryDAO.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationHistoryConnector_Create = ");
                throw;
            }
        }

        public IEnumerable<ProgramUtilizationHistory> Read(string key, int offset, int limit, out int totalCount, Guid companyId = default)
        {
            try
            {
                var result = new List<ProgramUtilizationHistory>();
                IEnumerable<ProgramUtilizationHistoryDAO> query;
                if (!string.IsNullOrWhiteSpace(key))
                {
                    query = companyId == default ? _dbContext.ProgramUtilizationHistories.Where(x => x.CompanyName.Contains(key)) :
                                                       _dbContext.ProgramUtilizationHistories.Where(x => x.CompanyId == companyId && x.CompanyName.Contains(key));
                }
                else
                {
                    query = companyId == default ? _dbContext.ProgramUtilizationHistories.AsEnumerable() :
                                                       _dbContext.ProgramUtilizationHistories.Where(x => x.CompanyId == companyId);
                }
                totalCount = query.Count();
                query = limit != Consts.UNLIMITED ? query.Skip(offset).Take(limit) : query.Skip(offset);
                foreach (var row in query)
                {
                    result.Add(row.ToProgramUtilizationHistory());
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationHistoryConnector_ReadByKey&Offset&Limit = ");
                throw;
            }
        }

        public IEnumerable<ProgramUtilizationHistory> Read(bool? isExpired, Guid? programId, DateTime? expiredFrom, DateTime? expiredTo, Guid companyId = default)
        {
            try
            {
                var result = new List<ProgramUtilizationHistory>();
                var query = _dbContext.ProgramUtilizationHistories
                    .Where(c => c.Id != Consts.GHOST_USERS_COMPANY_ID);

                if (companyId != default)
                    query = query.Where(c => c.CompanyId == companyId);
                if (isExpired.HasValue)
                {
                    query = isExpired.Value ? query.Where(p => p.Expired <= DateTime.Now) : query.Where(p => p.Expired > DateTime.Now);
                }
                if (programId.HasValue && programId.Value != Guid.Empty)
                {
                    var companyIds = _dbContext.Companies
                        .Include(c => c.ProgramUtilization)
                        .Where(c => c.ProgramId == programId.Value)
                        .Select(_ => _.Id);
                    query = query.Where(p => companyIds.Contains(p.CompanyId));
                }
                DateTime from = expiredFrom.HasValue ? expiredFrom.Value : default;
                DateTime to = expiredTo.HasValue ? expiredTo.Value : default;
                query = query.Where(p => p.Expired >= from);
                query = query.Where(p => p.Expired <= to);

                return query.Select(p => p.ToProgramUtilizationHistory());

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationHistoryConnector_ReadByIsExpired&ProgramId&ExpiredFrom&ExpiredTo&CompanyId = ");
                throw;
            }
        }
    }
}