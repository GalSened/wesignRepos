namespace DAL.Connectors
{
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Models;
    using Common.Models.Users;
    using DAL.DAOs.Users;
    using DAL.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class UserTokenConnector : IUserTokenConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IDater _dater;
        private readonly ILogger _logger;

        public UserTokenConnector(IWeSignEntities dbContext, IDater dater, ILogger logger)
        {
            _dbContext = dbContext;
            _dater = dater;
            _logger = logger;
        }

        public async Task UpdateRefreshToken(UserTokens userTokens)
        {
            try
            {
                var userTokensDAO = _dbContext.UserTokens.Local.FirstOrDefault(u => u.UserId == userTokens.UserId) ??
                await _dbContext.UserTokens.FirstOrDefaultAsync(u => u.UserId == userTokens.UserId);
                if (userTokensDAO != null)
                {
                    if (userTokensDAO.RefreshToken == userTokens.RefreshToken &&
                        userTokensDAO.RefreshTokenExpiredTime == userTokens.RefreshTokenExpiredTime)
                    {
                        return;
                    }
                    userTokensDAO.RefreshToken = userTokens.RefreshToken;
                    userTokensDAO.RefreshTokenExpiredTime = userTokens.RefreshTokenExpiredTime;
                    userTokensDAO.LastLoginGroupId = userTokens.LastLoginGroupId;
                    userTokensDAO.AuthToken = userTokens.AuthToken;
                    _dbContext.UserTokens.Update(userTokensDAO);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    userTokensDAO = new UserTokensDAO(userTokens);
                    await _dbContext.UserTokens.AddAsync(userTokensDAO);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserTokenConnector_UpdateRefreshToken = ");
                throw;
            }
        }

        public async Task CreateResetPasswordToken(UserTokens userTokens)
        {
            try
            {
                var userTokensDAO = _dbContext.UserTokens.Local.FirstOrDefault(u => u.UserId == userTokens.UserId) ??
                                await _dbContext.UserTokens.FirstOrDefaultAsync(u => u.UserId == userTokens.UserId);
                if (userTokensDAO != null)
                {
                    await _dbContext.UserTokens.Where(u => u.UserId == userTokens.UserId).ExecuteUpdateAsync(
                       x => x.SetProperty(property => property.ResetPasswordToken, userTokens.ResetPasswordToken));
                }
                else
                {
                    userTokensDAO = new UserTokensDAO(userTokens);
                    await _dbContext.UserTokens.AddAsync(userTokensDAO);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserTokenConnector_CreateResetPasswordToken = ");
                throw;
            }
        }

        public Task DeleteResetPasswordToken(UserTokens userTokens)
        {
            try
            {
                return _dbContext.UserTokens.Where(u => u.UserId == userTokens.UserId).ExecuteUpdateAsync(
                        x => x.SetProperty(property => property.ResetPasswordToken, (string)null));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserTokenConnector_DeleteResetPasswordToken = ");
                throw;
            }
        }

        public async Task<UserTokens> Read(User user)
        {
            try
            {
                var dbUserTokens = await _dbContext.UserTokens.FirstOrDefaultAsync(u => u.UserId == user.Id);
                return dbUserTokens.ToUserTokens();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserTokenConnector_ReadByUser = ");
                throw;
            }
        }

        public async Task<UserTokens> Read(UserTokens userTokens)
        {
            try
            {
                return (await _dbContext.UserTokens.FirstOrDefaultAsync(u => u.ResetPasswordToken == userTokens.ResetPasswordToken)).ToUserTokens();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserTokenConnector_ReadByUserTokens = ");
                throw;
            }
        }

        public async Task<UserTokens> ReadTokenByRefreshToken(UserTokens userTokens, bool returnExpired = false)
        {
            try
            {
                UserTokens dbUserToken = (_dbContext.UserTokens.Local.FirstOrDefault(u => u.RefreshToken == userTokens.RefreshToken) ??
                    await _dbContext.UserTokens.FirstOrDefaultAsync(u => u.RefreshToken == userTokens.RefreshToken)).ToUserTokens();
                if (dbUserToken == null || returnExpired)
                {
                    return dbUserToken;
                }
                return _dater.UtcNow() <= dbUserToken.RefreshTokenExpiredTime ? dbUserToken : null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserTokenConnector_ReadTokenByRefreshToken = ");
                throw;
            }
        }
    }
}