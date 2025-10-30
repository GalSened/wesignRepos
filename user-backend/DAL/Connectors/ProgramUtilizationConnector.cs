namespace DAL.Connectors
{
    using Common.Consts;
    using Common.Enums;
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Models;
    using Common.Models.Programs;
    using Common.Models.Settings;
    using DAL.DAOs.Programs;
    using DAL.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ProgramUtilizationConnector : IProgramUtilizationConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IDater _dater;
        private readonly ILogger _logger;
        private readonly IOptions<GeneralSettings> _generalSettings;

        public ProgramUtilizationConnector(IWeSignEntities dbContext, IOptions<GeneralSettings> generalSettings, IDater dater, ILogger logger)
        {
            _dbContext = dbContext;
            _dater = dater;
            _logger = logger;
            _generalSettings = generalSettings;
        }

        public async Task Create(ProgramUtilization programUtilization)
        {
            try
            {
                var programUtilizationDAO = new ProgramUtilizationDAO(programUtilization);
                await _dbContext.ProgramUtilization.AddAsync(programUtilizationDAO);
                await _dbContext.SaveChangesAsync();

                programUtilization.Id = programUtilizationDAO.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_Create = ");
                throw;
            }
        }

        public async Task Delete(ProgramUtilization programUtilization)
        {
            try
            {
                var programUtilizationDAO = _dbContext.ProgramUtilization.Local.FirstOrDefault(x => x.Id == programUtilization.Id) ??
                await _dbContext.ProgramUtilization.FirstOrDefaultAsync(x => x.Id == programUtilization.Id);
                if (programUtilizationDAO != null)
                {
                    _dbContext.ProgramUtilization.Remove(programUtilizationDAO);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_Delete = ");
                throw;
            }
        }

        public async Task<ProgramUtilization> Read(ProgramUtilization programUtilization)
        {
            try
            {
                return programUtilization == null ? null :
                    (await _dbContext.ProgramUtilization.FirstOrDefaultAsync(x => x.Id == programUtilization.Id))
                                                .ToProgramUtilization();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_Read = ");
                throw;
            }
        }

        public async Task UpdateUsersAmount(User user, CalcOperation operation, int additionRangeSize = 1)
        {
            try
            {
                using var tempContext = new WeSignEntities(_generalSettings, _dater);
                var programUtilization = await GetProgramUtilization(user, tempContext);
                if (programUtilization != null)
                {
                    if (operation == CalcOperation.Add)
                    {
                        programUtilization.Users += additionRangeSize;
                    }
                    if (programUtilization.Users > 0 && operation == CalcOperation.Substruct)
                    {
                        programUtilization.Users -= additionRangeSize;
                    }

                    await tempContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_UpdateUsersAmount = ");
                throw;
            }
        }

        public async Task RemoveUser(User user)
        {
            try
            {
                using (var tempContext = new WeSignEntities(_generalSettings, _dater))
                {
                    var programUtilization = await GetProgramUtilization(user, tempContext);
                    if (programUtilization != null && programUtilization.Users > 0)
                    {
                        programUtilization.Users--;

                        await tempContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_RemoveUser = ");
                throw;
            }
        }
        public void FixUnsetProgramLastResetDate()
        {
            try
            {
                if (_dbContext.Companies.Include(x => x.ProgramUtilization).Any(x => x.ProgramUtilization.LastResetDate == DateTime.MinValue))
                {
                    var programItilizationsList = _dbContext.Companies.Include(x => x.ProgramUtilization).Where(x => x.ProgramUtilization.LastResetDate == DateTime.MinValue).Select(x => x.ProgramUtilization.Id).ToList();

                    _dbContext.ProgramUtilization.Where(x => programItilizationsList.Contains(x.Id)).ExecuteUpdate(
                        x => x.SetProperty(x => x.LastResetDate, x => x.StartDate)
                        );
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_FixUnsetProgramLastResetDate = ");
                throw;
            }
        }
        public async Task Update(ProgramUtilization programUtilization)
        {
            try
            {
                var programUtilizationDAO = _dbContext.ProgramUtilization.Local.FirstOrDefault(x => x.Id == programUtilization.Id) ??
                await _dbContext.ProgramUtilization.FirstOrDefaultAsync(x => x.Id == programUtilization.Id);
                if (programUtilizationDAO != null)
                {
                    programUtilizationDAO.DocumentsUsage = programUtilization.DocumentsUsage;
                    programUtilizationDAO.DocumentsSentNotifyCount = programUtilization.DocumentsSentNotifyCount;
                    programUtilizationDAO.Expired = programUtilization.Expired;
                    programUtilizationDAO.SMS = programUtilization.SMS;
                    programUtilizationDAO.SmsSentNotifyCount = programUtilization.SmsSentNotifyCount;
                    programUtilizationDAO.VisualIdentifications = programUtilization.VisualIdentifications;
                    programUtilizationDAO.VisualIdentificationUsedNotifyCount = programUtilization.VisualIdentificationUsedNotifyCount;
                    programUtilizationDAO.VideoConferenceUsedNotifyCount = programUtilization.VisualIdentificationUsedNotifyCount;
                    programUtilizationDAO.Templates = programUtilization.Templates;
                    programUtilizationDAO.Users = programUtilization.Users;
                    programUtilizationDAO.DocumentsLimit = programUtilization.DocumentsLimit;
                    programUtilizationDAO.ProgramResetType = programUtilization.ProgramResetType;
                    programUtilizationDAO.LastResetDate = programUtilization.LastResetDate;
                    _dbContext.ProgramUtilization.Update(programUtilizationDAO);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_Update = ");
                throw;
            }

        }

        public async Task UpdateTemplatesAmount(User user, CalcOperation operation, int additionRangeSize = 1)
        {
            try
            {
                using var tempContext = new WeSignEntities(_generalSettings, _dater);
                var programUtilization = await GetProgramUtilization(user, tempContext);
                if (programUtilization != null)
                {
                    if (operation == CalcOperation.Add)
                    {
                        programUtilization.Templates += additionRangeSize;
                    }
                    if (programUtilization.Templates > 0 && operation == CalcOperation.Substruct)
                    {
                        programUtilization.Templates -= additionRangeSize;
                    }

                    await tempContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_UpdateTemplatesAmount = ");
                throw;
            }
        }

        public async Task AddDocument(User user)
        {
            try
            {
                using (var tempContext = new WeSignEntities(_generalSettings, _dater))
                {

                    var programUtilization = await GetProgramUtilization(user, tempContext);
                    if (programUtilization != null)
                    {
                        programUtilization.DocumentsUsage++;

                        await tempContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_AddDocumentByUser = ");
                throw;
            }
        }

        public async Task AddDocument(User user, int additionRangeSize = 1)
        {
            try
            {
                using var tempContext = new WeSignEntities(_generalSettings, _dater);
                var programUtilization = await GetProgramUtilization(user, tempContext);
                if (programUtilization != null)
                {
                    programUtilization.DocumentsUsage += additionRangeSize;

                    await tempContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_AddDocumentByUser&AdditionalRangeSize = ");
                throw;
            }
        }

        public async Task AddSms(User user, int additionRangeSize = 1)
        {
            try
            {
                using (var tempContext = new WeSignEntities(_generalSettings, _dater))
                {
                    var programUtilization = await GetProgramUtilization(user, tempContext);
                    if (programUtilization != null)
                    {
                        programUtilization.SMS += additionRangeSize;
                        await tempContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_AddSms = ");
                throw;
            }
        }

        public async Task AddVideoConfrence(User user, int additionRangeSize = 1)
        {
            try
            {
                using (var tempContext = new WeSignEntities(_generalSettings, _dater))
                {
                    var programUtilization = await GetProgramUtilization(user, tempContext);
                    if (programUtilization != null)
                    {
                        programUtilization.VideoConference += additionRangeSize;
                        await tempContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_AddVideoConfrence = ");
                throw;
            }
        }

        public async Task AddVisualIdentification(User user, int additionRangeSize = 1)
        {
            try
            {
                using (var tempContext = new WeSignEntities(_generalSettings, _dater))
                {
                    var programUtilization = await GetProgramUtilization(user, tempContext);
                    if (programUtilization != null)
                    {
                        programUtilization.VisualIdentifications += additionRangeSize;
                        await tempContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_AddVisualIdentification = ");
                throw;
            }
        }

        public async Task RemoveVisualIdentification(User user, int additionRangeSize = 1)
        {
            try
            {
                using (var tempContext = new WeSignEntities(_generalSettings, _dater))
                {
                    var programUtilization = await GetProgramUtilization(user, tempContext);
                    if (programUtilization != null && programUtilization.VisualIdentifications > 0)
                    {
                        programUtilization.VisualIdentifications -= additionRangeSize;
                        await tempContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_RemoveVisualIdentification = ");
                throw;
            }
        }

        public IEnumerable<ProgramUtilization> Read(int offset, int limit, out int totalCount)
        {
            try
            {
                var result = new List<ProgramUtilization>();
                var query = _dbContext.ProgramUtilization.AsQueryable();
                totalCount = query.Count();
                query = limit != Consts.UNLIMITED ? query.Skip(offset).Take(limit) : query.Skip(offset);
                foreach (var row in query)
                {
                    result.Add(row.ToProgramUtilization());
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_Read = ");
                throw;
            }
        }


        private async Task<ProgramUtilizationDAO> GetProgramUtilization(User user, IWeSignEntities tempContext)
        {
            try
            {
                if (user.ProgramUtilization != null && user.CompanyId == Consts.FREE_ACCOUNTS_COMPANY_ID)
                {

                    return await tempContext.ProgramUtilization.AsTracking().FirstOrDefaultAsync(x => x.Id == user.ProgramUtilizationId);
                }

                var company = await tempContext.Companies.Include(c => c.ProgramUtilization).AsTracking().FirstOrDefaultAsync(c => c.Id == user.CompanyId);
                return company.ProgramUtilization;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramUtilizationConnector_GetProgramUtilization = ");
                throw;
            }
        }
    }
}