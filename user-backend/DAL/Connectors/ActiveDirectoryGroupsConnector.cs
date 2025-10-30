using Common.Interfaces.DB;
using Common.Models;
using Common.Models.ActiveDirectory;
using DAL.DAOs.ActiveDirectory;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class ActiveDirectoryGroupsConnector : IActiveDirectoryGroupsConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ILogger _logger;

        public ActiveDirectoryGroupsConnector(IWeSignEntities dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Create(ActiveDirectoryGroup item)
        {
            try
            {
                ActiveDirectoryGroupDAO activeDirectoryGroupDAO = new ActiveDirectoryGroupDAO(item);
                await _dbContext.ActiveDirectoryGroups.AddAsync(activeDirectoryGroupDAO);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ActiveDirectoryGroupsConnector_Create = ");
                throw;
            }

        }

        public async Task<IEnumerable<ActiveDirectoryGroup>> GetAllGroupsForCompany(Company company)
        {
            try
            {
                List<ActiveDirectoryGroup> result = new List<ActiveDirectoryGroup>();
                List<ActiveDirectoryGroupDAO> items = await _dbContext.ActiveDirectoryGroups.Include(x => x.Group).Where(x => x.Group.CompanyId == company.Id).ToListAsync();

                foreach (var item in items)
                {
                    result.Add(item.ToGroup());
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ActiveDirectoryGroupsConnector_GetAllGroupsForCompany = ");
                throw;
            }
        }

        public async Task Remove(ActiveDirectoryGroup item)
        {
            try
            {
                var itemToDelete = await _dbContext.ActiveDirectoryGroups.FirstOrDefaultAsync(x => x.Id == item.Id);
                if (itemToDelete != null)
                {
                    _dbContext.ActiveDirectoryGroups.Remove(itemToDelete);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ActiveDirectoryGroupsConnector_Remove = ");
                throw;
            }
            
        }

        public async Task Update(ActiveDirectoryGroup groupToUpdate)
        {
            try
            {
                var itemToUpdate = await _dbContext.ActiveDirectoryGroups.FirstOrDefaultAsync(x => x.Id == groupToUpdate.Id);
                if (itemToUpdate != null)
                {
                    itemToUpdate.ActiveDirectoryContactsGroupName = groupToUpdate.ActiveDirectoryContactsGroupName;
                    itemToUpdate.ActiveDirectoryUsersGroupName = groupToUpdate.ActiveDirectoryUsersGroupName;
                    _dbContext.ActiveDirectoryGroups.Update(itemToUpdate);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ActiveDirectoryGroupsConnector_Update = ");
                throw;
            }
        }
    }
}
