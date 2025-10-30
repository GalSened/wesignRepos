using Common.Enums.Program;
using Common.Models;
using Common.Models.Documents;
using Common.Models.ManagementApp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.ManagementApp
{
    public interface ICompanies
    {
        Task<(IEnumerable<CompanyBaseDetails>, int totalCount)>  Read(string key, int offset, int limit);
        Task<CompanyExpandedDetails> Read(Company company, User user);
        Task<DocumentDeletionConfiguration> ReadCompanyDeletionConfiguration(Company company);
        Task Create(Company company, Group group, User user);
        Task Update(Company company, Group group, User user);
        Task Delete(Company company);
        Task ResendResetPasswordMail(User user);
        Task AddProgramUtilizationCompanyToHistory(Company company, ProgramUtilizationHistoryResourceMode mode);
        Task UpdateTransactionId(Company company);
        Task UpdateCompanyTransactionAndExpirationTime(Company company);
    }
}
