using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Enums.Companies;
using Common.Models;
using Common.Models.Configurations;

namespace Common.Interfaces.DB
{
    public interface ICompanyConnector
    {
        Task<Company> Read(Company company);
        IEnumerable<Company> Read(string key, int offset, int limit, CompanyStatus? status, out int totalCount);
        IEnumerable<Company> Read(Guid programID, int offset, int limit, out int totalCount);
        IEnumerable<Company> Read(int docsUsagePercentage , int offset, int limit, out int totalCount);
        IEnumerable<Company> ReadWithReminders(string key, int offset, int limit, CompanyStatus? status, out int totalCount);
        Task Create(Company company);
        Task Update(Company company);
        /// <summary>
        /// Delete by changing status to deleted
        /// </summary>
        /// <param name="company"></param>
        Task Delete(Company company);
        Task DeleteRange(IEnumerable<Company> companies);
        Task Delete(Company company, Action<Company> cleanCompanyLogoAndEmailsFromFS);
        IEnumerable<Company> ReadAboutToBeExpired(DateTime from, DateTime to);
        IEnumerable<Company> ReadForProgramUtilization();
        Task<CompanyConfiguration> ReadConfiguration(Company company);
        IEnumerable<Company> ReadCompaniesByProgram(Program program);
       
    }
}
