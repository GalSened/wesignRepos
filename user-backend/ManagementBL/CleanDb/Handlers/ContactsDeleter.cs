using Common.Consts;
using Common.Enums.Contacts;
using Common.Extensions;
using Common.Handlers.Files;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.CleanDb.Handlers
{
    public class ContactsDeleter : IDeleter
    {
        
        private readonly ILogger _logger;
        private readonly ICertificate _certificate;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFilesWrapper _filesWrapper;

        public ContactsDeleter(ILogger logger, ICertificate certificate, IServiceScopeFactory scopeFactory,
            IFilesWrapper filesWrapper)

        {
            
            _logger = logger;
            _certificate = certificate;

            _scopeFactory = scopeFactory;
            _filesWrapper = filesWrapper;
        }

        public async Task<bool> DeleteProcess()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                IContactConnector contactConnector = scope.ServiceProvider.GetService<IContactConnector>();
                IEnumerable<Contact> contacts = contactConnector.ReadDeletedWithNoSignerConnected();
               
                if (contacts.Any())
                {
                    try
                    {
                        using (var innerScope = _scopeFactory.CreateScope())
                        {
                            IContactConnector dependencyService = scope.ServiceProvider.GetService<IContactConnector>();
                            await dependencyService.Delete(contacts.ToList(), DeleteContactSealsFromFSAndDeleteCert);
                        }

                    }

                    catch (Exception ex)
                    {

                        _logger.Error(ex, "Failed to clean contacts ");
                    }

                }
            }
            catch (Exception ex)
            {

                _logger.Error(ex, "Failed to clean deleted contacts");
            }
            return true;
        }


        private void DeleteContactSealsFromFSAndDeleteCert(List<Contact> contacts)
        {
            foreach(Contact contact in contacts)
            {
                try
                {
                    _filesWrapper.Contacts.DeleteSeals(contact);
                    _certificate.Delete(contact);
                }
                catch { 
                    // do nothing
                }
            }
          
        }
       
    }
}
