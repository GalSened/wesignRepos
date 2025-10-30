using Common.Enums.Users;
using Common.Models.ActiveDirectory;
using Common.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.ManagementApp
{
    public class CompanyExpandedDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<(Guid, string)> Groups { get; set; }
        public User User { get; set; }
        public Guid ProgramId { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string TransactionId { get; set; }
        public CompanyConfiguration CompanyConfiguration { get; set; }
        public IEnumerable<ActiveDirectoryGroup> ActiveDirectoryGroups { get; set; }
        public CompanySigner1Details CompanySigner1Details { get; set; }

    }
}
