using Common.Interfaces.DB;
using Common.Models.Configurations;
using DAL.DAOs.ActiveDirectory;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class ActiveDirectoryConfigConnector : IActiveDirectoryConfigConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ILogger _logger;

        public ActiveDirectoryConfigConnector(IWeSignEntities dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Create(ActiveDirectoryConfiguration activeDirectoryConfig)
        {
            try
            {
                var activeDirectoryConfigDAO = new ActiveDirectoryConfigDAO(activeDirectoryConfig);
                await _dbContext.ActiveDirectoryConfigs.AddAsync(activeDirectoryConfigDAO);
                await _dbContext.SaveChangesAsync();

                activeDirectoryConfig.Id = activeDirectoryConfigDAO.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ActiveDirectoryConfigConnector_Create = ");
                throw;
            }
        }

      

        public async Task<ActiveDirectoryConfiguration> Read()
        {
            try
            {
                var activeDirectoryConfigDAO = await _dbContext.ActiveDirectoryConfigs.FirstOrDefaultAsync();
                return activeDirectoryConfigDAO.ToActiveDirectoryConfiguration();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ActiveDirectoryConfigConnector_Read = ");
                throw;
            }
        }

        public async Task Update(ActiveDirectoryConfiguration activeDirectoryConfig)
        {
            try
            {
                var activeDirectory = await _dbContext.ActiveDirectoryConfigs.FirstOrDefaultAsync();
                activeDirectory.Container = activeDirectoryConfig.Container;
                activeDirectory.Domain = activeDirectoryConfig.Domain;
                activeDirectory.Host = activeDirectoryConfig.Host;
                activeDirectory.Password = activeDirectoryConfig.Password;
                activeDirectory.Port = activeDirectoryConfig.Port;
                activeDirectory.User = activeDirectoryConfig.User;
                _dbContext.ActiveDirectoryConfigs.Update(activeDirectory);
                await _dbContext.SaveChangesAsync();
                activeDirectoryConfig.Id = activeDirectory.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ActiveDirectoryConfigConnector_Update = ");
                throw;
            }
        }
    }
}
