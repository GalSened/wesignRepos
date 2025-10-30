using Common.Models;
using Common.Models.ActiveDirectory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.DB
{

    public interface IActiveDirectoryGroupsConnector 
    {
        Task<IEnumerable<ActiveDirectoryGroup>> GetAllGroupsForCompany(Company company);
        Task Remove(ActiveDirectoryGroup item);
        Task Create(ActiveDirectoryGroup item);
        Task Update(ActiveDirectoryGroup groupToUpdate);
    }

}