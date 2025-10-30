using Common.Models.ActiveDirectory;
using Common.Models.Configurations;
using DAL.DAOs.Companies;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DAL.DAOs.ActiveDirectory
{
    [Table("ActiveDirectoryConfigurations")]
    public class ActiveDirectoryConfigDAO
    {

        public Guid Id { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }
        public string Container { get; set; }
        public string Domain { get; set; }
        public string User { get; set; }
        public string Password { get; set; }


        public ActiveDirectoryConfigDAO() { }
        public ActiveDirectoryConfigDAO(ActiveDirectoryConfiguration activeDirectoryConfig)
        {
            Id = activeDirectoryConfig.Id == Guid.Empty ? default : activeDirectoryConfig.Id;
            Domain = activeDirectoryConfig.Domain;
            Host = activeDirectoryConfig.Host;
            Port = activeDirectoryConfig.Port;
            Container = activeDirectoryConfig.Container;
            User = activeDirectoryConfig.User;
            Password = activeDirectoryConfig.Password;

        }
    }
}
