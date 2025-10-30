using Common.Enums.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.ManagementApp
{
    public class UserDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreationTime { get; set; }
        public Language Language { get; set; }
        public UserStatus UserStatus { get; set; }
        public UserType Type { get; set; }
        public string CompanyName { get; set; }
        public string GroupName { get; set; }
        public string ProgramName { get; set; }
        public DateTime LastSeen { get; set; }
        public string Username { get; set; }
    }
}
