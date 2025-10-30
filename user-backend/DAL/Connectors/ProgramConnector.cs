using Common.Consts;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models;
using DAL.DAOs.Programs;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class ProgramConnector : IProgramConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IDater _dater;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        public ProgramConnector(IWeSignEntities dbContext, IDater dater, IMemoryCache memoryCache, ILogger logger)
        {
            _dbContext = dbContext;
            _dater = dater;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task Create(Program program)
        {
            try
            {
                ProgramDAO programDAO = new ProgramDAO(program);
                await _dbContext.Programs.AddAsync(programDAO);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_Create = ");
                throw;
            }
        }

        public async Task<bool> Exists(Program program)
        {
            try
            {
                if (program == null)
                {
                    return false;
                }
                return await _dbContext.Programs.AnyAsync(p => p.Id == program.Id ||
                                                               p.Name == program.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_Exists = ");
                throw;
            }
        }

        public async Task<bool> CanAddUser(User user) // Why by user???
        {
            try
            {
                var program = await GetProgram(user);
                var programUtilization = await GetProgramUtilization(user);
                if (programUtilization != null && program != null)
                {
                    return program.Users == Consts.UNLIMITED || program.Users > programUtilization.Users;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_CanAddUser = ");
                throw;
            }
        }

        public async Task<bool> CanAddTemplate(User user)
        {
            try
            {
                var program = await GetProgram(user);
                var programUtilization = await GetProgramUtilization(user);
                if (programUtilization != null && program != null)
                {
                    return program.Templates == Consts.UNLIMITED || program.Templates > programUtilization.Templates;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_CanAddTemplate = ");
                throw;
            }
        }

        public async Task<bool> CanAddDocument(User user, int additionRangeSize = 1)
        {
            try
            {
                var program = await GetProgram(user);
                var programUtilization = await GetProgramUtilization(user);
                if (programUtilization != null && program != null)
                {
                    if ((programUtilization.ProgramResetType == Common.Enums.Program.ProgramResetType.DocumentsLimitOnly) ||
                            (programUtilization.ProgramResetType == Common.Enums.Program.ProgramResetType.TimeAndDocumentsLimit))
                    {
                        return (programUtilization.DocumentsLimit > programUtilization.DocumentsUsage + additionRangeSize - 1) &&
                            (_dater.UtcNow() <= programUtilization.Expired);
                    }

                    return program.DocumentsPerMonth == Consts.UNLIMITED || program.DocumentsPerMonth >
                                        programUtilization.DocumentsUsage + additionRangeSize - 1;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_CanAddDocument = ");
                throw;
            }
        }

        public async Task<bool> CanAddSms(User user, int additionRangeSize = 1)
        {
            try
            {
                var program = await GetProgram(user);
                var programUtilization = await GetProgramUtilization(user);
                if (programUtilization != null && program != null)
                {


                    return program.SmsPerMonth == Consts.UNLIMITED || program.SmsPerMonth > (programUtilization.SMS + additionRangeSize - 1);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_CanAddSms = ");
                throw;
            }
        }

        public async Task<bool> CanAddVisualIdentifications(User user, int additionRangeSize = 1)
        {
            try
            {
                var program = await GetProgram(user);
                var programUtilization = await GetProgramUtilization(user);
                if (programUtilization != null && program != null)
                {
                    return program.VisualIdentificationsPerMonth == Consts.UNLIMITED ||
                        program.VisualIdentificationsPerMonth >=
                        (programUtilization.VisualIdentifications + additionRangeSize);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_CanAddVisualIdentifications = ");
                throw;
            }
        }

        public async Task<bool> CanAddVideoConference(User user, int additionRangeSize = 1)
        {
            try
            {
                var program = await GetProgram(user);
                var programUtilization = await GetProgramUtilization(user);
                if (programUtilization != null && program != null)
                {
                    return program.VideoConferencePerMonth == Consts.UNLIMITED || program.VideoConferencePerMonth >= (programUtilization.VideoConference + additionRangeSize);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_CanAddVideoConference = ");
                throw;
            }
        }

        public async Task<bool> IsProgramExpired(User user)
        {
            try
            {
                var programUtilization = await GetProgramUtilization(user);

                return programUtilization != null && _dater.UtcNow() > programUtilization.Expired;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_IsProgramExpired = ");
                throw;
            }
        }

        public async Task Delete(Program program)
        {
            try
            {
                var programDAO = new ProgramDAO(program);
                _dbContext.Programs.Remove(programDAO);
                await _dbContext.SaveChangesAsync();
                _memoryCache.Remove(programDAO.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_Delete = ");
                throw;
            }
        }

        public async Task Update(Program program)
        {
            try
            {
                var programDAO = await _dbContext.Programs
                .Include(x => x.ProgramUIView)
                .FirstOrDefaultAsync(x => x.Id == program.Id);
                if (programDAO != null)
                {
                    programDAO.Name = program.Name;
                    programDAO.Note = program.Note;
                    programDAO.ServerSignature = program.ServerSignature;
                    programDAO.SmartCard = program.SmartCard;
                    programDAO.SmsPerMonth = program.SmsPerMonth;
                    programDAO.VisualIdentificationsPerMonth = program.VisualIdentificationsPerMonth;
                    programDAO.VideoConferencePerMonth = program.VideoConferencePerMonth;
                    programDAO.Templates = program.Templates;
                    programDAO.DocumentsPerMonth = program.DocumentsPerMonth;
                    programDAO.Users = program.Users;
                    programDAO.ProgramUIView.ShouldShowSelfSign = program.UIViewLicense?.ShouldShowSelfSign ?? true;
                    programDAO.ProgramUIView.ShouldShowGroupSign = program.UIViewLicense?.ShouldShowGroupSign ?? true;
                    programDAO.ProgramUIView.ShouldShowLiveMode = program.UIViewLicense?.ShouldShowLiveMode ?? true;
                    programDAO.ProgramUIView.ShouldShowContacts = program.UIViewLicense?.ShouldShowContacts ?? true;
                    programDAO.ProgramUIView.ShouldShowAddNewTemplate = program.UIViewLicense?.ShouldShowAddNewTemplate ?? true;
                    programDAO.ProgramUIView.ShouldShowTemplates = program.UIViewLicense?.ShouldShowTemplates ?? true;
                    programDAO.ProgramUIView.ShouldShowDocuments = program.UIViewLicense?.ShouldShowDocuments ?? true;
                    programDAO.ProgramUIView.ShouldShowProfile = program.UIViewLicense?.ShouldShowProfile ?? true;
                    programDAO.ProgramUIView.ShouldShowUploadAndsign = program.UIViewLicense?.ShouldShowUploadAndsign ?? true;
                    programDAO.ProgramUIView.ShouldShowDistribution = program.UIViewLicense?.ShouldShowDistribution ?? false;

                    programDAO.ProgramUIView.ShouldShowEditCheckboxField = program.UIViewLicense?.ShouldShowEditCheckboxField ?? true;
                    programDAO.ProgramUIView.ShouldShowEditDateField = program.UIViewLicense?.ShouldShowEditDateField ?? true;
                    programDAO.ProgramUIView.ShouldShowEditEmailField = program.UIViewLicense?.ShouldShowEditEmailField ?? true;
                    programDAO.ProgramUIView.ShouldShowEditListField = program.UIViewLicense?.ShouldShowEditListField ?? true;
                    programDAO.ProgramUIView.ShouldShowEditNumberField = program.UIViewLicense?.ShouldShowEditNumberField ?? true;
                    programDAO.ProgramUIView.ShouldShowEditPhoneField = program.UIViewLicense?.ShouldShowEditPhoneField ?? true;
                    programDAO.ProgramUIView.ShouldShowEditRadioField = program.UIViewLicense?.ShouldShowEditRadioField ?? true;
                    programDAO.ProgramUIView.ShouldShowEditSignatureField = program.UIViewLicense?.ShouldShowEditSignatureField ?? true;
                    programDAO.ProgramUIView.ShouldShowEditTextField = program.UIViewLicense?.ShouldShowEditTextField ?? true;
                    programDAO.ProgramUIView.ShouldShowMultilineText = program.UIViewLicense?.ShouldShowMultilineText ?? true;
                    _dbContext.Programs.Update(programDAO);
                    await _dbContext.SaveChangesAsync();
                    _memoryCache.Remove(programDAO.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_Update = ");
                throw;
            }
        }

        public bool IsFreeTrialUser(User user)
        {
            try
            {
                return user?.ProgramUtilization != null && user?.CompanyId == Consts.FREE_ACCOUNTS_COMPANY_ID;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_IsFreeTrialUser = ");
                throw;
            }
        }

        public async Task<Program> Read(Program program)
        {
            try
            {
                if (program == null)
                {
                    return null;
                }

                var programResult = _memoryCache.Get<Program>($"Program_{program.Id}");
                if (programResult == null)
                {
                    programResult = (await _dbContext.Programs.Include(x => x.ProgramUIView)
                                                    .FirstOrDefaultAsync(x => x.Id == program.Id))
                                                    .ToProgram();
                    _memoryCache.Set<Program>($"Program_{program.Id}", programResult, TimeSpan.FromMinutes(7));
                }

                return programResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_ReadByProgram = ");
                throw;
            }
        }

        public IEnumerable<Program> Read(string key, int offset, int limit, out int totalCount)
        {
            try
            {
                var result = new List<Program>();
                var query = string.IsNullOrWhiteSpace(key) ?
                                                    _dbContext.Programs
                                                    .Include(x => x.ProgramUIView)
                                                    .Select(x => x) :
                                                    _dbContext.Programs
                                                    .Include(x => x.ProgramUIView)
                                                    .Where(c => c.Name.Contains(key));

                totalCount = query.Count();
                query = limit != Consts.UNLIMITED ? query.Skip(offset).Take(limit) : query.Skip(offset);
                foreach (var row in query)
                {
                    result.Add(row.ToProgram());
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_ReadByKey&Offset&Limit = ");
                throw;
            }
        }

        public IEnumerable<Program> Read(int minDocs, int minSMS, bool? isUsed, int offset, int limit, out int totalCount) // used to get programs filtered by useage
        {
            try
            {
                var query = _dbContext.Programs
                .Where(c => c.DocumentsPerMonth >= minDocs)
                .Where(c => c.SmsPerMonth >= minSMS)
                .OrderByDescending(c => c.DocumentsPerMonth)
                .ThenByDescending(c => c.SmsPerMonth)
                .AsEnumerable();

                if (isUsed.HasValue)
                    query = isUsed.Value ? query.Where(p => p.Users > 0) : query.Where(p => p.Users <= 0);

                totalCount = query.Count();
                query = limit != Consts.UNLIMITED ? query.Skip(offset).Take(limit) : query.Skip(offset);
                return query.Select(x => x.ToProgram());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ProgramConnector_ReadByMinDocs&MinSms&IsUsed&Offset&Limit = ");
                throw;
            }
        }

        private async Task<ProgramDAO> GetProgram(User user)
        {
            string dictName = $"Program_ByCompanyId_{user.CompanyId}";
            ProgramDAO programResult = _memoryCache.Get<ProgramDAO>(dictName);
            if(programResult == null)
            {
                var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == user.CompanyId);
                programResult =  await _dbContext.Programs.FirstOrDefaultAsync(p => p.Id == company.ProgramId);
                _memoryCache.Set<ProgramDAO>(dictName, programResult, TimeSpan.FromMinutes(10));

            }

          
            return programResult;
        }

        private async Task<ProgramUtilizationDAO> GetProgramUtilization(User user)
        {
            if (IsFreeTrialUser(user))
            {
                return   await   _dbContext.ProgramUtilization.FirstOrDefaultAsync(x => x.Id == user.ProgramUtilization.Id) ;
            }

            var companyProgramUtilization = await _dbContext.Companies.Include(c => c.ProgramUtilization)
                .FirstOrDefaultAsync(c => c.Id == user.CompanyId);
            return companyProgramUtilization.ProgramUtilization;
        }
    }
}
