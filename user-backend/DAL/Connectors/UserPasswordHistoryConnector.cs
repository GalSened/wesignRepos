using Common.Interfaces.DB;
using Common.Models;
using DAL.DAOs.Users;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class UserPasswordHistoryConnector : IUserPasswordHistoryConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ILogger _logger;

        public UserPasswordHistoryConnector(IWeSignEntities dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Create(UserPasswordHistory uph)
        {
            try
            {
                var userPasswordHistoryDAO = await _dbContext.UserPasswordHistory.FirstOrDefaultAsync(_ => _.UserId == uph.UserId && _.Password == uph.Password);
                if (userPasswordHistoryDAO == null)
                {
                    userPasswordHistoryDAO = new UserPasswordHistoryDAO(uph);
                    await _dbContext.UserPasswordHistory.AddAsync(userPasswordHistoryDAO);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserPasswordHistoryConnector_Create = ");
                throw;
            }
        }

        public IEnumerable<UserPasswordHistory> ReadAllByUserId(Guid userId)
        {
            try
            {
                var dbUserPasswordsHistory = _dbContext.UserPasswordHistory.Where(uph => uph.UserId == userId);
                return dbUserPasswordsHistory.Select(uph => uph.ToUserPasswordHistory()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserPasswordHistoryConnector_ReadAllUserId = ");
                throw;
            }
        }

        public Task DeleteOldestPasswordsByUserId(Guid userId, int count)
        {
            try
            {
                var oldestUserRecord = _dbContext.UserPasswordHistory
                                 .Where(uph => uph.UserId == userId)
                                 .OrderBy(uph => uph.CreationTime)
                                 .Take(count);
                _dbContext.UserPasswordHistory.RemoveRange(oldestUserRecord);
                return _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserPasswordHistoryConnector_DeleteOldestPasswordsByUserId = ");
                throw;
            }
        }

        public Task DeleteAllByUserId(Guid userId)
        {
            try
            {
                return _dbContext.UserPasswordHistory.Where(uph => uph.UserId == userId).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserPasswordHistoryConnector_DeleteAllByUserId = ");
                throw;
            }
        }
    }
}