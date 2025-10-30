using Common.Consts;
using Common.Enums.Companies;
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
    public class CompaniesDeleter : IDeleter
    {
        
        private readonly ILogger _logger;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFilesWrapper _filesWrapper;
        public CompaniesDeleter(ILogger logger, 
            IServiceScopeFactory scopeFactory, IFilesWrapper filesWrapper)            
        {
            
            _logger = logger;
            _scopeFactory = scopeFactory;
            _filesWrapper = filesWrapper;
        }

        public async Task<bool> DeleteProcess()
        {


            try
            {
                using var scope = _scopeFactory.CreateScope();
                ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();

                IEnumerable<Company> companies = companyConnector.Read(key: Consts.EMPTY, offset: 0, limit: Consts.UNLIMITED, status: CompanyStatus.Deleted, out _);
                foreach (var company in companies)
                {
                    try
                    {
                        IEnumerable<Group> groups = groupConnector.Read(company);
                        if (!groups.Any())
                        {

                            using (var innerScope = _scopeFactory.CreateScope())
                            {
                                ICompanyConnector dependencyService = scope.ServiceProvider.GetService<ICompanyConnector>();
                                await dependencyService.Delete(company, CleanCompanyLogoAndEmailsFromFS);
                            }

                        }
                        else
                        {
                            groups = groups.Where(x => x.GroupStatus != Common.Enums.Groups.GroupStatus.Deleted);
                            if (groups.Any())
                            {
                                using (var innerScope = _scopeFactory.CreateScope())
                                {
                                    IGroupConnector dependencyService = scope.ServiceProvider.GetService<IGroupConnector>();
                                    await dependencyService.Delete(groups.ToList());
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to clean company {CompanyId}", company.Id);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to clean deleted companies");
            }

            return true;
        }

        private void CleanCompanyLogoAndEmailsFromFS(Company company)
        {
            _filesWrapper.Configurations.DeleteCompanyResorces(company);
            

        }
    
    }
}
