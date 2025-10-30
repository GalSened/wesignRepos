using Common.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Configuration
{
    public class TabletsConfigurationDTO
    {
        public IEnumerable<Tablet> Tablets { get; set; }
    }
}
