using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Models.Configurations
{
    public class ActiveDirecrotyConfigurationDTO
    {

        public string Host { get; set; }
        public int Port { get; set; }
        public string Container { get; set; }
        public string Domain { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    } 
}
