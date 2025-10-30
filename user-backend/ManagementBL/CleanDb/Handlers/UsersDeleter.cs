
using Common.Consts;
using Common.Enums.Users;
using Common.Extensions;
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
    public class UsersDeleter : IDeleter
    {
        
        private readonly ICertificate _certificate;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public UsersDeleter(ILogger logger,  ICertificate certificate, IServiceScopeFactory scopeFactory) 
        {
            
            _certificate = certificate;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task<bool> DeleteProcess()
        {
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            IProgramConnector programConnector = scope.ServiceProvider.GetService<IProgramConnector>();
            IContactConnector contactConnector = scope.ServiceProvider.GetService<IContactConnector>();
            ITemplateConnector templateConnector = scope.ServiceProvider.GetService<ITemplateConnector>();
            IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();



            IEnumerable<User> users = userConnector.ReadDeletedUsers();
            foreach(var user in users)
            {

                try
                {
                    bool freeTrialUser = programConnector.IsFreeTrialUser(user);
                    if (freeTrialUser)
                    {

                        IEnumerable<Contact> contacts = contactConnector.ReadAllContactInGroup(new Group() { Id = user.GroupId });
                        IEnumerable<Template> templates = templateConnector.Read(new Group() { Id = user.GroupId });
                        IEnumerable<DocumentCollection> documetCollection = documentCollectionConnector.Read(new Group() { Id = user.GroupId });
                        Group group = await groupConnector.Read(new Group() { Id = user.GroupId });
                        // what about the programutilizetion
                        if (!contacts.Any() && !templates.Any()  && !documetCollection.Any() )
                        {
                            using (var innerScope = _scopeFactory.CreateScope())
                            {
                                IUserConnector userConnectorDependencyService = innerScope.ServiceProvider.GetService<IUserConnector>();
                                IUserPasswordHistoryConnector userPasswordHistoryConnectorDependencyService = innerScope.ServiceProvider.GetService<IUserPasswordHistoryConnector>();
                                await userConnectorDependencyService.Delete(user, CleanUserCertificate);
                                await userPasswordHistoryConnectorDependencyService.DeleteAllByUserId(user.Id);
                                _logger.Debug("user {UserId} deleted successfully", user.Id);
                            }
                        }

                        if ((group != null) && (group.GroupStatus != Common.Enums.Groups.GroupStatus.Deleted))
                        {
                            using (var innerScope = _scopeFactory.CreateScope())
                            {
                                IGroupConnector dependencyService = scope.ServiceProvider.GetService<IGroupConnector>();
                                await dependencyService.Delete(group);
                            }

                        }                        


                    }
                    else
                    {
                        using (var innerScope = _scopeFactory.CreateScope())
                        {
                            IUserConnector uesrDependencyService = scope.ServiceProvider.GetService<IUserConnector>();
                            IUserPasswordHistoryConnector userPassworkHistorydependencyService = scope.ServiceProvider.GetService<IUserPasswordHistoryConnector>();
                            await uesrDependencyService.Delete(user, CleanUserCertificate);
                            await userPassworkHistorydependencyService.DeleteAllByUserId(user.Id);
                        }

                    }
         
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to clean user form DB {UserEmail}", user.Email );
                }


            }
            // TODO : remove 
            return true;
        }

        private void CleanUserCertificate(User user)
        {
            _certificate.Delete(user);
        }
      
    }
}
