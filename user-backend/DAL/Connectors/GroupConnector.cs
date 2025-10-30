namespace DAL.Connectors
{

    using Common.Enums.Groups;
    using Common.Enums.Results;
    using Common.Extensions;
    using Common.Interfaces.DB;
    using Common.Interfaces.ManagementApp;
    using Common.Models;
    using DAL.DAOs.Groups;
    using DAL.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class GroupConnector : IGroupConnector
    {

        private readonly IWeSignEntities _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        public GroupConnector(IWeSignEntities dbContext, IMemoryCache memoryCache, ILogger logger)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task Create(Group group)
        {
            try
            {
                var existGroup = await _dbContext.Groups.FirstOrDefaultAsync(g => g.CompanyId == group.CompanyId && g.Name == group.Name &&
                                                                                g.GroupStatus != GroupStatus.Deleted);
                if (existGroup != null)
                {
                    throw new InvalidOperationException(ResultCode.GroupAlreadyExistInCompany.GetNumericString());
                }
                var groupDAO = new GroupDAO(group);
                await _dbContext.Groups.AddAsync(groupDAO);
                await _dbContext.SaveChangesAsync();

                group.Id = groupDAO.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_Create = ");
                throw;
            }
        }

        public async Task Delete(List<Group> groups)
        {
            try
            {
                if (groups.Count > 0)
                {
                    groups.ForEach(g => _memoryCache.Remove(g.Id));

                    await _dbContext.Groups.Where(x => groups.Select(x => x.Id).Contains(x.Id)).ExecuteUpdateAsync(
                        setters => setters.SetProperty(x => x.GroupStatus, GroupStatus.Deleted));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_Delete = ");
                throw;
            }
        }
        public Task Delete(Group group)
        {
            try
            {
                _memoryCache.Remove(group.Id);

                return _dbContext.Groups.Where(g => g.Id == group.Id && g.CompanyId == group.CompanyId).ExecuteUpdateAsync(
                       setters => setters.SetProperty(x => x.GroupStatus, GroupStatus.Deleted));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_Delete = ");
                throw;
            }
        }

        public Task DeletePermanently(Group group)
        {
            try
            {
                return _dbContext.Groups.Where(x => x.Id == group.Id).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_DeletePermanently = ");
                throw;
            }

        }


        public Task<bool> IsGroupsExistInCompany(Company company)
        {
            try
            {
                return _dbContext.Groups.AnyAsync(x => x.CompanyId == company.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_IsGroupsExistInCompany = ");
                throw;
            }
        }
        public IEnumerable<Group> Read(Company company)
        {
            try
            {
                return _dbContext.Groups.Where(g => g.CompanyId == company.Id)
                                        .Select(g => g.ToGroup());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_ReadByCompany = ");
                throw;

            }
        }

        public IEnumerable<Group> Read(GroupStatus status, bool incluseUsers)
        {
            try
            {
                var groups = _dbContext.Groups.Where(g => g.GroupStatus == status);

                if (incluseUsers)
                    groups.Include(u => u.Users);

                return groups.Select(g => g.ToGroup());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_ReadByStatus = ");
                throw;
            }
        }
        public async Task<Group> Read(User user)
        {
            try
            {
                return (await _dbContext.Groups.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == user.GroupId && x.GroupStatus != GroupStatus.Deleted)).ToGroup();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_ReadByUser = ");
                throw;
            }
        }
        public async Task<Group> Read(Group group)
        {
            try
            {
                if (group.CompanyId != Guid.Empty)
                {
                    return (await _dbContext.Groups.FirstOrDefaultAsync(x => x.CompanyId == group.CompanyId && (x.Id == group.Id || x.Name == group.Name) &&
                    x.GroupStatus != GroupStatus.Deleted)).ToGroup();
                }
                var memGroup = _memoryCache.Get<Group>(group.Id);
                if (memGroup != null)
                {
                    return memGroup;
                }
                var groupDAO = (await _dbContext.Groups.FirstOrDefaultAsync(x => (x.Id == group.Id || x.Name == group.Name) && x.GroupStatus != GroupStatus.Deleted)).ToGroup();
                if (groupDAO != null)
                {
                    _memoryCache.Set(groupDAO.Id, groupDAO, TimeSpan.FromMinutes(3));
                }
                return groupDAO;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_ReadByGroup = ");
                throw;
            }
        }

        public async Task<Company> ReadCompany(Group group)
        {
            try
            {
                var company = _memoryCache.Get<Company>($"CompanyByGroupId_{group.Id}");
                if (company == null)
                {
                    company = (await _dbContext.Companies.Include(x => x.CompanyConfiguration).FirstOrDefaultAsync(x => x.Groups.Select(x => x.Id).Contains(group.Id))).ToCompany();
                    _memoryCache.Set<Company>($"CompanyByGroupId_{group.Id}", company, TimeSpan.FromMinutes(3));
                }


                return company;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_ReadCompany = ");
                throw;
            }

        }

        public async Task<List<Group>> ReadAllUserGroups(User user)
        {
            try
            {
                var userInfo = await _dbContext.Users.Include(x => x.AdditionalGroupsMapper).FirstOrDefaultAsync(x => x.Id == user.Id);
                HashSet<Guid> groupsIds = new HashSet<Guid>() {
                userInfo.GroupId
                };
                if (userInfo.AdditionalGroupsMapper != null && userInfo.AdditionalGroupsMapper.Count > 0)
                {
                    foreach (var groupMapper in userInfo.AdditionalGroupsMapper ?? Enumerable.Empty<AdditionalGroupMapperDAO>())
                    {
                        groupsIds.Add(groupMapper.GroupId);
                    }
                }
                return await _dbContext.Groups.Where(x => groupsIds.Contains(x.Id) && x.GroupStatus != GroupStatus.Deleted).Select(d => d.ToGroup()).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_ReadAllUserGroups = ");
                throw;
            }
        }

        public IEnumerable<Group> ReadMany(IEnumerable<Group> groups)
        {
            try
            {
                var existingGroupIds = groups.Select(g => g.Id).ToList();
                var existingGroups = _dbContext.Groups
                    .Where(g => existingGroupIds.Contains(g.Id) && g.GroupStatus != GroupStatus.Deleted)
                    .Select(g => g.ToGroup());
                foreach (var group in existingGroups)
                {
                    _memoryCache.Set(group.Id, group, TimeSpan.FromMinutes(3));
                }
                return existingGroups;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_ReadMany = ");
                throw;
            }

        }

        public IEnumerable<Group> ReadManyByGroupName(string groupName)
        {
            try
            {
                var groups = _dbContext.Groups
                    .Where(g => g.Name.ToLower().Contains(groupName.ToLower()))
                    .Select(g => g.ToGroup());
                return groups.AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_ReadManyByGroupName = ");
                throw;
            }
        }

        public async Task Update(Group group)
        {
            try
            {
                var groupDAO = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == group.Id && g.CompanyId == group.CompanyId);
                if (groupDAO == null)
                {
                    throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
                }
                var existGroup = await _dbContext.Groups.FirstOrDefaultAsync(g => g.CompanyId == group.CompanyId &&
                g.Name == group.Name && g.GroupStatus != GroupStatus.Deleted);
                if (existGroup != null)
                {
                    throw new InvalidOperationException(ResultCode.GroupAlreadyExistInCompany.GetNumericString());
                }
                groupDAO.Name = group.Name;
                _memoryCache.Remove(group.Id);
                _dbContext.Groups.Update(groupDAO);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_Update = ");
                throw;
            }

        }


        public Task DeleteAllAdditionalGroupsGroupConnection(Group group)
        {
            try
            {
                return _dbContext.AdditionalGroupsMapper.Where(g => g.GroupId == group.Id).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_DeleteAllAdditionalGroupsGroupConnection = ");
                throw;
            }
        }
        public async Task UpdateAdditionalGroups(User dbUser, List<AdditionalGroupMapper> additionalGroupsMapper)
        {
            try
            {
                var existAdditionalGroups = _dbContext.AdditionalGroupsMapper.Where(g => g.UserId == dbUser.Id);
                bool saveChanges = false;
                if (existAdditionalGroups != null && await existAdditionalGroups.AnyAsync())
                {
                    _dbContext.AdditionalGroupsMapper.RemoveRange(existAdditionalGroups);
                    saveChanges = true;
                }
                if (additionalGroupsMapper != null && additionalGroupsMapper.Count > 0)
                {
                    await _dbContext.AdditionalGroupsMapper.AddRangeAsync(additionalGroupsMapper.Select(x => new AdditionalGroupMapperDAO(x)));
                    saveChanges = true;
                }
                if (saveChanges)
                {
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_UpdateAdditionalGroups = ");
                throw;
            }

        }

        public bool ExistRecordsInGroup(Group group)
        {
            try
            {
                var item = _dbContext.Groups.Where(x => x.Id == group.Id).Select
                (groupRecord => new
                {
                    ContactExist = _dbContext.Contacts.Any(x => x.GroupId == groupRecord.Id),
                    TemplatesExist = _dbContext.Templates.Any(x => x.GroupId == groupRecord.Id),
                    CollectionExist = _dbContext.DocumentCollections.Any(x => x.GroupId == groupRecord.Id),
                    UserExist = _dbContext.Users.Any(x => x.GroupId == groupRecord.Id)
                }
                ).FirstOrDefault();

                return item.ContactExist || item.TemplatesExist || item.CollectionExist || item.UserExist;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_ExistRecordsInGroup = ");
                throw;
            }

        }

        public Dictionary<Guid, string> GetGroupIdNameDictionary(List<Guid> groupIds)
        {
            try
            {
                return _dbContext.Groups.Where(g => groupIds.Contains(g.Id)).Select(_ => new { _.Id, _.Name }).ToDictionary(x => x.Id, x => x.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GroupConnector_GetGroupIdNameDictionary = ");
                throw;
            }
        }
    }
}
