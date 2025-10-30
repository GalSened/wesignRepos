using Common.Consts;
using Common.Enums.Users;
using Common.Interfaces.DB;
using Common.Models;
using DAL.DAOs.Programs;
using DAL.DAOs.Users;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models.Users;
using Serilog;

namespace DAL.Connectors
{
    public class UserConnector : IUserConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ISymmetricEncryptor _symmetricEncryptor;
        private readonly ILogger _logger;

        public UserConnector(IWeSignEntities dbContext, ISymmetricEncryptor symmetricEncryptor, ILogger logger)
        {
            _dbContext = dbContext;
            _symmetricEncryptor = symmetricEncryptor;
            _logger = logger;
        }
        public IEnumerable<User> Read(Group group)
        {
            try
            {
                return _dbContext.Users.Include(x => x.AdditionalGroupsMapper).Where(x => x.GroupId == group.Id
            || x.AdditionalGroupsMapper.Any(x => x.GroupId == group.Id)).Select(u => u.ToUser());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadByGroup = ");
                throw;
            }
        }

        public async Task<User> ReadWithUserToken(User user)
        {
            try
            {
                if (user == null)
                {
                    return user;
                }
                UserDAO userDAO = null;
                if (user.Id != Guid.Empty)
                {
                    userDAO = _dbContext.Users.Local.FirstOrDefault(u => u.Id == user?.Id && u.Status != UserStatus.Deleted)
                                   ?? await _dbContext.Users
                                                      .Include(u => u.UserTokens).Include(c => c.UserConfiguration)
                                                      .FirstOrDefaultAsync(u => u.Id == user.Id && u.Status != UserStatus.Deleted);
                }
                if (userDAO == null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    userDAO = _dbContext.Users.Local.FirstOrDefault(u => u.Email.ToLower() == user.Email.ToLower() && u.Status != UserStatus.Deleted)
                                    ?? await _dbContext.Users
                                              .Include(u => u.UserTokens).Include(c => c.UserConfiguration)
                                    .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower() && u.Status != UserStatus.Deleted);
                }
                if (userDAO == null && !string.IsNullOrWhiteSpace(user.Username))
                {
                    var decrypderUserName = await _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username.ToLower());
                    userDAO = _dbContext.Users.Local.FirstOrDefault(u => user.Username != null && u.Username == decrypderUserName && u.Status != UserStatus.Deleted)
                                    ?? await _dbContext.Users
                                              .Include(u => u.UserTokens).Include(c => c.UserConfiguration)
                                    .FirstOrDefaultAsync(u => u.Username == decrypderUserName && u.Status != UserStatus.Deleted);
                }

                if (userDAO != null && !string.IsNullOrWhiteSpace(userDAO.Username))
                {
                    userDAO.Username = await _symmetricEncryptor.DecryptString(Consts.symmetricKey, userDAO.Username);
                }

                return userDAO.ToUser();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadWithUserToken = ");
                throw;
            }
        }

        /// <summary>
        /// No link to the ProgramUtilization table is made here because the package utilization can be done simultaneously by several different users
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<User> Read(User user)
        {
            try
            {
                if (user == null)
                {
                    return user;
                }
                UserDAO userDAO = null;
                if (user.Id != Guid.Empty)
                {
                    userDAO = _dbContext.Users.Local.FirstOrDefault(u => u.Id == user?.Id && u.Status != UserStatus.Deleted)
                                   ?? await _dbContext.Users.Include(u => u.UserConfiguration)
                                                      .Include(u => u.Company)
                                                      .Include(u => u.UserTokens)
                                                      .Include(u => u.ProgramUtilization).
                                                      Include(u => u.AdditionalGroupsMapper)
                                                      .FirstOrDefaultAsync(u => u.Id == user.Id && u.Status != UserStatus.Deleted);
                }
                if (userDAO == null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    userDAO = _dbContext.Users.Local.FirstOrDefault(u => u.Email.ToLower() == user.Email.ToLower() && u.Status != UserStatus.Deleted)
                                    ?? await _dbContext.Users.Include(u => u.UserConfiguration)
                                              .Include(u => u.Company)
                                              .Include(u => u.ProgramUtilization)
                                              .Include(u => u.UserTokens)
                                              .Include(u => u.AdditionalGroupsMapper)
                                    .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower() && u.Status != UserStatus.Deleted);
                }
                if (userDAO == null && !string.IsNullOrWhiteSpace(user.Username))
                {
                    var decrypderUserName = await _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username.ToLower());
                    userDAO = _dbContext.Users.Local.FirstOrDefault(u => user.Username != null && u.Username == decrypderUserName && u.Status != UserStatus.Deleted)
                                    ?? await _dbContext.Users.Include(u => u.UserConfiguration)
                                              .Include(u => u.Company)
                                              .Include(u => u.ProgramUtilization)
                                              .Include(u => u.UserTokens)
                                              .Include(u => u.AdditionalGroupsMapper)
                                    .FirstOrDefaultAsync(u => u.Username == decrypderUserName && u.Status != UserStatus.Deleted);
                }

                if (userDAO != null && !string.IsNullOrWhiteSpace(userDAO.Username))
                {
                    userDAO.Username = await _symmetricEncryptor.DecryptString(Consts.symmetricKey, userDAO.Username);
                }

                return userDAO.ToUser();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadByUser = ");
                throw;
            }
        }

        public IEnumerable<User> Read(string key, int offset, int limit, UserStatus? status, out int totalCount, Guid companyId = default, IEnumerable<Guid> groupIds = null)
        {
            try
            {
                var query = _dbContext.Users
                            .Include(u => u.UserConfiguration)
                            .Include(u => u.UserTokens)
                            .Include(u => u.ProgramUtilization)
                             .Include(u => u.AdditionalGroupsMapper)
                            .Where(x => x.Id != Consts.GHOST_USER_ID && x.Status != UserStatus.Deleted &&
                            x.Id != Consts.PAYMENT_ADMIN_ID && x.Type != UserType.PaymentAdmin && x.Type != UserType.Ghost);

                if (status.HasValue)
                {
                    query = query.Where(x => x.Status == status);
                }

                if (companyId != default)
                {
                    query = query.Where(u => u.CompanyId == companyId);
                }

                if (!string.IsNullOrWhiteSpace(key))
                {
                    if (groupIds != null)
                    {
                        query = query.Where(u => u.Name.Contains(key) || u.Email.Contains(key)
                        || groupIds.Contains(u.GroupId) || u.AdditionalGroupsMapper.Any(agm => groupIds.Contains(agm.GroupId)));

                    }
                    else
                    {
                        query = query.Where(u => u.Name.Contains(key) || u.Email.Contains(key));
                    }
                }

                totalCount = query.Count();

                query = query.OrderByDescending(x => x.CreationTime).Skip(offset);

                if (limit != Consts.UNLIMITED)
                {
                    query = query.Take(limit);
                }

                return query.Select(u => u.ToUser());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadByKey&Offset&Limit&Status = ");
                throw;
            }
        }

        public async Task<User> ReadAllStatuesById(Guid UserId)
        {
            try
            {
                var userDAO = _dbContext.Users.Local.FirstOrDefault(u => u.Id == UserId)
                           ?? await _dbContext.Users.Include(u => u.UserConfiguration)
                                              .Include(u => u.Company)
                                              .Include(u => u.UserTokens)
                                              .Include(u => u.ProgramUtilization)
                                               .Include(u => u.AdditionalGroupsMapper)
                                              .FirstOrDefaultAsync(u => u.Id == UserId);

                if (userDAO != null && !string.IsNullOrWhiteSpace(userDAO.Username))
                {
                    userDAO.Username = await _symmetricEncryptor.DecryptString(Consts.symmetricKey, userDAO.Username);
                }

                return userDAO.ToUser();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadAllStatusesById = ");
                throw;
            }
        }


        public async Task<User> ReadAsync(User user)
        {
            try
            {
                var userDAO = await _dbContext.Users.Include(u => u.UserConfiguration)
                                                .Include(u => u.Company)
                                                .Include(u => u.UserTokens)
                                                 .Include(u => u.AdditionalGroupsMapper)
                                                .FirstOrDefaultAsync(u => (u.Id == user.Id || u.Email == user.Email.ToLower() || u.Username == user.Username) && u.Status != UserStatus.Deleted);
                return userDAO.ToUser();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadAsyncByUser = ");
                throw;
            }
        }

        public IEnumerable<User> ReadAdminUsersInCompany(Company company)
        {
            try
            {
                return _dbContext.Users
                               .Where(x => x.Status != UserStatus.Deleted
                               && x.CompanyId == company.Id && x.Type == UserType.CompanyAdmin).Select(x => x.ToUser());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadAdminUsersInCompany = ");
                throw;
            }
        }
        public int UsersInCompanyCount(Company company)
        {
            try
            {
                return _dbContext.Users.Where(x => x.Id != Consts.GHOST_USER_ID && x.Status != UserStatus.Deleted &&
                            x.Id != Consts.PAYMENT_ADMIN_ID && x.Type != UserType.PaymentAdmin && x.Type != UserType.Ghost &&
                            x.CompanyId == company.Id).Count();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_UsersInCompanyCount = ");
                throw;
            }
        }

        public async Task Create(User user)
        {
            try
            {
                var userDAO = new UserDAO(user);
                if (!string.IsNullOrEmpty(user.Username))
                {
                    userDAO.Username = await _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username.ToLower());
                }
                await _dbContext.Users.AddAsync(userDAO);
                await _dbContext.SaveChangesAsync();

                user.Id = userDAO.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_Create = ");
                throw;
            }
        }

        public async Task<bool> Exists(User user)
        {
            try
            {
                if (user == null)
                {
                    return false;
                }

                var encryptedUserName = "";
                if (!string.IsNullOrWhiteSpace(user.Username))
                {
                    encryptedUserName = await _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username.ToLower());
                }
                return
                    await _dbContext.Users.FirstOrDefaultAsync(u => ((user.Email != null && u.Email.ToLower() == user.Email.ToLower()) ||
                    u.Id == user.Id ||
                    (!string.IsNullOrWhiteSpace(user.Username) && (u.Username == encryptedUserName || u.Username == user.Username)))
                    && u.Status != UserStatus.Deleted) != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_Exists = ");
                throw;
            }
        }

        public async Task<bool> ExistsByEmail(User user)
        {
            try
            {
                return user != null
                && await _dbContext.Users.FirstOrDefaultAsync(u =>
                (!string.IsNullOrWhiteSpace(user.Email) && u.Email.ToLower() == user.Email.ToLower())
                && u.Status != UserStatus.Deleted) != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ExistsByEmail = ");
                throw;
            }
        }

        public async Task<bool> ExistsByUsername(User user)
        {
            try
            {
                if (user == null)
                {
                    return false;
                }
                var encryptedUserName = "";
                if (!string.IsNullOrWhiteSpace(user.Username))
                {
                    encryptedUserName = await _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username.ToLower());
                }
                return
                    await _dbContext.Users.FirstOrDefaultAsync(u => (!string.IsNullOrWhiteSpace(user.Username)
                && (u.Username == encryptedUserName || u.Username == user.Username))
                && u.Status != UserStatus.Deleted) != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ExistsByUsername = ");
                throw;
            }
        }
        public async Task UpdateUserMainGroup(User user)
        {
            try
            {
                var userDAO = _dbContext.Users.Local.FirstOrDefault(u => u.Id == user.Id) ?? await _dbContext.Users.Include(x => x.UserConfiguration)
                                         .FirstOrDefaultAsync(u => u.Id == user.Id);
                if (userDAO != null)
                {
                    userDAO.GroupId = user.GroupId;
                    _dbContext.Users.Update(userDAO);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_UpdateUserMainGroup = ");
                throw;
            }
        }

        public Task UpdateLastSeen(User user)
        {
            try
            {
                return _dbContext.Users.Where(u => u.Id == user.Id).ExecuteUpdateAsync
                 (setters => setters.SetProperty(x => x.LastSeen, user.LastSeen));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_UpdateLastSeen = ");
                throw;
            }
        }

        public async Task Update(User user, bool adminUpdate)
        {
            try
            {
                var userDAO = _dbContext.Users.Local.FirstOrDefault(u => u.Id == user.Id) ?? await _dbContext.Users.Include(x => x.UserConfiguration)
                                              .FirstOrDefaultAsync(u => u.Id == user.Id);
                if (userDAO != null)
                {
                    if (adminUpdate)
                    {
                        userDAO.GroupId = user.GroupId;
                    }
                    userDAO.Name = user.Name;
                    userDAO.Password = user.Password;
                    userDAO.PasswordSetupTime = user.PasswordSetupTime;
                    userDAO.ProgramUtilizationId = user.ProgramUtilizationId;
                    userDAO.Status = user.Status;
                    userDAO.Type = user.Type;
                    userDAO.CompanyId = user.CompanyId;
                    userDAO.CreationTime = user.CreationTime;
                    userDAO.Email = user.Email;
                    if (userDAO.UserConfiguration != null)
                    {
                        userDAO.UserConfiguration.ShouldSendSignedDocument = user.UserConfiguration?.ShouldSendSignedDocument ?? userDAO.UserConfiguration.ShouldSendSignedDocument;
                        userDAO.UserConfiguration.ShouldNotifyWhileSignerSigned = user.UserConfiguration?.ShouldNotifyWhileSignerSigned ?? userDAO.UserConfiguration.ShouldNotifyWhileSignerSigned;
                        userDAO.UserConfiguration.shouldNotifyWhileSignerViewed = user.UserConfiguration?.ShouldNotifyWhileSignerViewed ?? userDAO.UserConfiguration.shouldNotifyWhileSignerViewed;
                        userDAO.UserConfiguration.ShouldDisplayNameInSignature = user.UserConfiguration?.ShouldDisplayNameInSignature ?? userDAO.UserConfiguration.ShouldDisplayNameInSignature;
                        userDAO.UserConfiguration.ShouldNotifySignReminder = user.UserConfiguration?.ShouldNotifySignReminder ?? userDAO.UserConfiguration.ShouldNotifySignReminder;
                        userDAO.UserConfiguration.SignReminderFrequencyInDays = user.UserConfiguration?.SignReminderFrequencyInDays ?? userDAO.UserConfiguration.SignReminderFrequencyInDays;
                        userDAO.UserConfiguration.Language = user.UserConfiguration?.Language ?? userDAO.UserConfiguration.Language;
                        userDAO.UserConfiguration.SignatureColor = user.UserConfiguration?.SignatureColor;
                    }
                    userDAO.LastSeen = user?.LastSeen ?? DateTime.MinValue;
                    if (!string.IsNullOrEmpty(user.Username) && userDAO.Username != user.Username &&
                        userDAO.Username != await _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username.ToLower()))
                    {
                        userDAO.Username = await _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username.ToLower());
                    }
                    else if (string.IsNullOrEmpty(user.Username))
                    {
                        userDAO.Username = null;
                    }

                    _dbContext.Users.Update(userDAO);
                    await _dbContext.SaveChangesAsync();
                    RemoveUserFromCache(userDAO.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_Update = ");
                throw;
            }
        }

        public Task UpdateUserPhone(User user)
        {
            try
            {
                return _dbContext.Users.Where(u => u.Id == user.Id).ExecuteUpdateAsync
                (setters => setters.SetProperty(x => x.Phone, user.Phone));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_UpdateUserPhone = ");
                throw;
            }
        }

        public async Task UpdateAsync(User user)
        {
            try
            {
                var userDAO = await _dbContext.Users.Include(u => u.UserConfiguration)
                                                 .Include(u => u.Company)
                                                 .Include(u => u.UserTokens)
                                                 .Include(u => u.ProgramUtilization)
                                                 .FirstOrDefaultAsync(u => u.Id == user.Id);


                userDAO.GroupId = user.GroupId;
                userDAO.Name = user.Name;
                userDAO.Password = user.Password;
                userDAO.PasswordSetupTime = user.PasswordSetupTime;
                userDAO.ProgramUtilization = user.ProgramUtilization == null ? null : new ProgramUtilizationDAO(user.ProgramUtilization);
                userDAO.Status = user.Status;
                userDAO.Type = user.Type;
                userDAO.CompanyId = user.CompanyId;
                userDAO.CreationTime = user.CreationTime;
                userDAO.Email = user.Email;
                userDAO.UserConfiguration.ShouldSendSignedDocument = user.UserConfiguration.ShouldSendSignedDocument;
                userDAO.UserConfiguration.ShouldNotifyWhileSignerSigned = user.UserConfiguration.ShouldNotifyWhileSignerSigned;
                userDAO.UserConfiguration.ShouldNotifySignReminder = user.UserConfiguration.ShouldNotifySignReminder;
                userDAO.UserConfiguration.SignReminderFrequencyInDays = user.UserConfiguration.SignReminderFrequencyInDays;
                userDAO.UserConfiguration.Language = user.UserConfiguration.Language;
                userDAO.UserConfiguration.SignatureColor = user.UserConfiguration.SignatureColor;
                userDAO.LastSeen = user?.LastSeen ?? DateTime.MinValue;
                if (!string.IsNullOrEmpty(user.Username) && userDAO.Username != user.Username && userDAO.Username != await _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username))
                {
                    userDAO.Username = await _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username.ToLower());
                }
                else if (string.IsNullOrEmpty(user.Username))
                {
                    userDAO.Username = "";
                }

                _dbContext.Users.Update(userDAO);
                await _dbContext.SaveChangesAsync();
                RemoveUserFromCache(userDAO.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_UpdateAsync = ");
                throw;
            }
        }

        public Task Delete(User user)
        {
            try
            {
                return _dbContext.Users.Where(u => u.Id == user.Id).ExecuteUpdateAsync(
                 (setters => setters.SetProperty(x => x.Status, UserStatus.Deleted).SetProperty(x => x.ProgramUtilizationId, (Guid?)null)));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_DeleteByUser = ");
                throw;
            }
        }

        public async Task Delete(User user, Action<User> cleanUserCertificate)
        {
            try
            {
                await _dbContext.Users.Where(u => u.Id == user.Id).ExecuteDeleteAsync();
                await _dbContext.UserOtpDetails.Where(u => u.UserId == user.Id).ExecuteDeleteAsync();
                cleanUserCertificate(user);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_DeleteByUser&CleanUserCertificate = ");
                throw;
            }
        }

        public IEnumerable<User> GetAllUsersInGroup(Group group)
        {
            try
            {
                var usersList = _dbContext.Users.Include(u => u.AdditionalGroupsMapper).Include(u => u.UserConfiguration).
                Where(x => (x.GroupId == group.Id || x.AdditionalGroupsMapper.Any(x => x.GroupId == group.Id)) && x.Status != UserStatus.Deleted).Select(x => x.ToUser());

                return usersList;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_GetAllUsersInGroup = ");
                throw;
            }
        }

        public IEnumerable<User> GetAllUsersInCompany(Company company)
        {
            try
            {
                return _dbContext.Users.Where(x => x.CompanyId == company.Id && x.Status != UserStatus.Deleted).Select(x => x.ToUser());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_GetAllUsersInCompany = ");
                throw;
            }
        }

        public IEnumerable<User> ReadDeletedUsers()
        {
            try
            {
                var query = _dbContext.Users
                            .Include(u => u.UserConfiguration)
                            .Include(u => u.UserTokens)
                            .Include(u => u.ProgramUtilization)
                            //.Include(u => u.Company)
                            .Where(x => x.Id != Consts.GHOST_USER_ID).Where(x => x.Status == UserStatus.Deleted);
                return query.Select(u => u.ToUser()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadDeletedUsers = ");
                throw;
            }
        }

        public IEnumerable<User> ReadFreeTrialUsers()
        {
            try
            {
                var query = _dbContext.Users
                .Include(u => u.ProgramUtilization)
                .Where(x => x.CompanyId == Consts.FREE_ACCOUNTS_COMPANY_ID);
                return query.Select(u => u.ToUser()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadFreeTrialUsers = ");
                throw;
            }
        }

        public IEnumerable<User> ReadUsersByType(UserType systemAdmin)
        {
            try
            {
                var query = _dbContext.Users
                          .Include(u => u.UserConfiguration)
                          .Include(u => u.UserTokens)
                          .Include(u => u.ProgramUtilization)
                           .Include(u => u.AdditionalGroupsMapper)
                          .Where(x => x.Id != Consts.GHOST_USER_ID && x.Id != Consts.SYSTEM_ADMIN_ID
                          && x.Status != UserStatus.Deleted && x.Id != Consts.GHOST_USERS_COMPANY_ID &&
                          x.Id != Consts.DEV_USER_ADMIN_ID &&
                          x.Id != Consts.PAYMENT_ADMIN_ID && x.Type == systemAdmin).Take(50);
                return query.Select(x => x.ToUser()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadUsersByType = ");
                throw;
            }
        }

        private void RemoveUserFromCache(Guid userId)
        {
            try
            {
                var userDAO = _dbContext.Users.Local.SingleOrDefault(_ => _.Id == userId);
                _dbContext.Users.Local.Remove(userDAO);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_RemoveUserFromCache = ");
                throw;
            }
        }

        public UserOtpDetails ReadOtpDitails(User user)
        {
            try
            {
                return _dbContext.UserOtpDetails.FirstOrDefault(x => x.UserId == user.Id).ToUserOtpDetails();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_ReadOtpDetails = ");
                throw;
            }
        }

        public async Task SetUserOtpDitails(User user, UserOtpDetails userOtpDetails)
        {
            try
            {
                if (!await _dbContext.UserOtpDetails.AnyAsync(x => x.UserId == user.Id))
                {
                    var userOtpDetailsDAO = new UserOtpDetailsDAO
                    {
                        UserId = user.Id,
                        Code = userOtpDetails.Code,
                        ExpirationTime = userOtpDetails.ExpirationTime,
                        OtpMode = userOtpDetails.OtpMode,
                        AdditionalInfo = userOtpDetails.AdditionalInfo
                    };
                    await _dbContext.UserOtpDetails.AddAsync(userOtpDetailsDAO);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    await _dbContext.UserOtpDetails.Where(u => u.UserId == user.Id).ExecuteUpdateAsync
                     (setters => setters.SetProperty(x => x.Code, userOtpDetails.Code).SetProperty(x => x.OtpMode, userOtpDetails.OtpMode).
                     SetProperty(x => x.ExpirationTime, userOtpDetails.ExpirationTime).SetProperty(x => x.AdditionalInfo, userOtpDetails.AdditionalInfo));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserConnecter_SetUserOtpDetails = ");
                throw;
            }
        }
    }
}