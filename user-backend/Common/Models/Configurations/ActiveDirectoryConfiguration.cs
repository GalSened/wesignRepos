using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Configurations
{
    public class ActiveDirectoryConfiguration
    {
        public Guid Id { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Container { get; set; }
        public string Domain { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
