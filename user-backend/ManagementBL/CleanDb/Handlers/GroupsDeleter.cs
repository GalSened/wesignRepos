using Common.Enums.Groups;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.PDF;
using Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.CleanDb.Handlers
{
    public class GroupsDeleter : IDeleter
    {
        
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public GroupsDeleter( ILogger logger, IServiceScopeFactory scopeFactory)
        
        {
        
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task<bool> DeleteProcess()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
                IContactConnector contactConnector = scope.ServiceProvider.GetService<IContactConnector>();
                ITemplateConnector templateConnector = scope.ServiceProvider.GetService<ITemplateConnector>();
                IDocumentCollectionConnector documentCollectionConnector =scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();


                IEnumerable<Group> groups = groupConnector.Read(GroupStatus.Deleted, incluseUsers: true);
                foreach (var group in groups)
                {
                    try
                    {
                 
                        bool existUndeletedContactsInGroup = await contactConnector.ExistNotDeletedContactInGroup(group);
                        if (existUndeletedContactsInGroup)
                        {

                            using (var innerScope = _scopeFactory.CreateScope())
                            {
                                IContactConnector dependencyService = scope.ServiceProvider.GetService<IContactConnector>();
                                await dependencyService.DeleteNotDeletedContactAndContactsGroupInGroup(group);

                            }
                        }
                        
                        bool existUndeletedtemplates =await templateConnector.ExistNotDeletedTemplatesInGroup(group);
                        if (existUndeletedtemplates)
                        {
                            using (var innerScope = _scopeFactory.CreateScope())
                            {
                                ITemplateConnector dependencyService = scope.ServiceProvider.GetService<ITemplateConnector>();
                                await dependencyService.DeleteUnDeletedTemplatesInGroup(group);

                            }
                        }

                        bool existDocumetCollection = await documentCollectionConnector.ExistNotDeletedCollectionsInGroup(group);
                        if (existDocumetCollection)
                        {
                            using (var innerScope = _scopeFactory.CreateScope())
                            {
                                IDocumentCollectionConnector dependencyService = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                                await dependencyService.DeleteAllUndeletedCollectionsInGroup(group);

                            }
                        }

                        


                        using (var innerScope = _scopeFactory.CreateScope())
                        {
                            IGroupConnector dependencyService = scope.ServiceProvider.GetService<IGroupConnector>();
                            await dependencyService.DeleteAllAdditionalGroupsGroupConnection(group);

                        }

                        List<User> notDeletedUsers = userConnector.Read(group).ToList();
                        HashSet<Guid> removeDeletedUserFromDeleteList = new HashSet<Guid>();
                        foreach (var user in notDeletedUsers)
                        {
                            if (user.GroupId == group.Id)
                            {
                                if (user.AdditionalGroupsMapper != null && user.AdditionalGroupsMapper.Count > 0)
                                {
                                    var mapper = user.AdditionalGroupsMapper.Find(x => x.GroupId != group.Id);
                                    if (mapper != null)
                                    {
                                        removeDeletedUserFromDeleteList.Add(user.Id);
                                        using (var innerScope = _scopeFactory.CreateScope())
                                        {
                                            IUserConnector dependencyService = scope.ServiceProvider.GetService<IUserConnector>();
                                            user.GroupId = mapper.GroupId;
                                           await dependencyService.UpdateUserMainGroup(user);

                                        }
                                    }
                                }
                            }
                        }
                        if (notDeletedUsers.Count > 0 && removeDeletedUserFromDeleteList.Count > 0)
                        {

                            foreach (var id in removeDeletedUserFromDeleteList)
                            {
                                var userToRemoveFromList = notDeletedUsers.FirstOrDefault(x => x.Id == id);
                                if (userToRemoveFromList != null)
                                {
                                    notDeletedUsers.Remove(userToRemoveFromList);

                                }
                            }

                        }

                        await DeleteUnDeletedUsers(notDeletedUsers);
                        bool existRecordInGroup = groupConnector.ExistRecordsInGroup(group);

                        if (!existRecordInGroup)
                        {
                            using (var innerScope = _scopeFactory.CreateScope())
                            {
                                IGroupConnector dependencyService = scope.ServiceProvider.GetService<IGroupConnector>();
                                await dependencyService.DeletePermanently(group);
                            }

                        }
                    }
                    catch (Exception ex)
                    {

                        _logger.Error(ex, "Failed to clean internal data for group {GroupId}", group.Id);
                    }

                }
            }
            catch (Exception ex)
            {

                _logger.Error(ex, "Failed to clean deleted contacts");
            }

            return true;

        }

        private async Task DeleteUnDeletedUsers(IEnumerable<User> users)
        {
            var notDeletedUser = users.Where(x => x.Status != Common.Enums.Users.UserStatus.Deleted);
            foreach(var user in notDeletedUser?? Enumerable.Empty<User>())
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    IUserConnector dependencyService = scope.ServiceProvider.GetService<IUserConnector>();
                    await dependencyService.Delete(user);
                }
            }
    
        }

       
    }
}
