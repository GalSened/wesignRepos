namespace Common.Models
{
    using Common.Enums;
    using Common.Enums.Users;
    using Common.Models.Configurations;
    using Common.Models.ManagementApp.Reports;
    using Common.Models.Programs;
    using Common.Models.Users;
    using System;
    using System.Collections.Generic;

    public class User
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string GroupName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
        public UserStatus Status { get; set; }
        public CreationSource CreationSource { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid GroupId { get; set; }
        public Guid? ProgramUtilizationId { get; set; }
        public ProgramUtilization ProgramUtilization { get; set; }
        //TODO validate while create user in some company , that that user will get hiw initil companyConfiguration
        //in his user configuration,
        //After that the user can change his owm config 
        public UserConfiguration UserConfiguration { get; set; }
        public UserTokens UserTokens { get; set; }
        public ProfileProgram ProfileProgram { get; set; }
        public string CompanyLogo { get; set; }
        public DateTime LastSeen { get; set; }
        public string Username { get; set; }
        public DateTime PasswordSetupTime { get; set; }

        public List<AdditionalGroupMapper> AdditionalGroupsMapper { get; set; }
        public List<UserPeriodicReport> UserPeriodicReports { get; set; }
        public List<ManagementPeriodicReport> ManagementPeriodicReports { get; set; }
        public User()
        {
            Status = UserStatus.Created;
            UserConfiguration = new UserConfiguration();
            UserTokens = new UserTokens();
            ProfileProgram = new ProfileProgram();
            ProgramUtilization = new ProgramUtilization();
            UserPeriodicReports = new List<UserPeriodicReport>();
            ManagementPeriodicReports = new List<ManagementPeriodicReport>();
        }
    }
}
