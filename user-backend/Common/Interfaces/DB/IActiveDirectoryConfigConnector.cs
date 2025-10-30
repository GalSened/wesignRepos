using Common.Models.ActiveDirectory;
using Common.Models.Configurations;
using System.Threading.Tasks;

namespace Common.Interfaces.DB
{
    public interface IActiveDirectoryConfigConnector
    {
        Task<ActiveDirectoryConfiguration> Read();
        Task Create(ActiveDirectoryConfiguration activeDirectoryConfig); 
        Task Update(ActiveDirectoryConfiguration activeDirectoryConfig);
    }
}
