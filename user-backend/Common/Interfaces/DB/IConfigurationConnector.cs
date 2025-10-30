namespace Common.Interfaces.DB
{
    using Common.Models.Configurations;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IConfigurationConnector
    {
       Task<Configuration> Read();
        Task Update(Configuration appConfiguration);
        IEnumerable<Tablet> ReadTablets(string key, Guid companyId);
       Task<bool> IsTabletExist(Tablet tablet);
        Task CreateTablet(Tablet tablet);       
    }
}
