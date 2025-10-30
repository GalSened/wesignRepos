/*
   ProgramUtilizationId will set only for Free_Account users , users for demo (Program ~ Trial)
 */
using Common.Enums;
using Common.Enums.Users;
using Common.Models;
using DAL.DAOs.Companies;
using DAL.DAOs.Configurations;
using DAL.DAOs.Documents;
using DAL.DAOs.Groups;
using DAL.DAOs.Management;
using DAL.DAOs.Programs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DAL.DAOs.Users
{
    public class UserDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
        public UserStatus Status { get; set; }
        public CreationSource CreationSource { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? ProgramUtilizationId { get; set; }
        public virtual ProgramUtilizationDAO ProgramUtilization { get; set; }
        public virtual UserConfigurationDAO UserConfiguration { get; set; }
        public virtual UserTokensDAO UserTokens { get; set; }
        public virtual CompanyDAO Company { get; set; }
        public virtual ICollection<DocumentCollectionDAO> DocumentCollections { get; set; }
        public virtual ICollection<AdditionalGroupMapperDAO> AdditionalGroupsMapper { get; set; }
        public virtual ICollection<UserPeriodicReportDAO> UserPeriodicReports { get; set; }
        public virtual ICollection<ManagementPeriodicReportDAO> ManagementPeriodicReports { get; set; }
        public DateTime LastSeen { get; set; }
        public string Username { get; set; }
        public DateTime PasswordSetupTime { get; set; }

        public UserDAO() { }

        public UserDAO(User user)
        {
            Id = user.Id == Guid.Empty ? default : user.Id;
            CompanyId = user.CompanyId == Guid.Empty ? default : user.CompanyId;
            GroupId = user.GroupId == Guid.Empty ? default : user.GroupId;
            ProgramUtilizationId = user.ProgramUtilizationId == Guid.Empty ? default : user.ProgramUtilizationId;
            Name = user.Name;
            Email = user.Email;
            Password = user.Password;
            CreationTime = user.CreationTime;
            Status = user.Status;
            Type = user.Type;
            CreationSource = user.CreationSource;
            UserConfiguration = new UserConfigurationDAO(user.UserConfiguration);
            //ProgramUtilization = new ProgramUtilizationDAO(user.ProgramUtilization);
            LastSeen = user?.LastSeen ?? DateTime.MinValue;
            Username = user.Username;
            PasswordSetupTime = user.PasswordSetupTime;
            Phone = user.Phone;
            AdditionalGroupsMapper = user.AdditionalGroupsMapper == null || user.AdditionalGroupsMapper.Count == 0 ? null : user.AdditionalGroupsMapper.Select(x => new AdditionalGroupMapperDAO(x)).ToList();
            UserPeriodicReports = user.UserPeriodicReports == null || !user.UserPeriodicReports.Any() ? null : user.UserPeriodicReports.Select(_ => new UserPeriodicReportDAO(_)).ToList();
            ManagementPeriodicReports = user.ManagementPeriodicReports == null || !user.ManagementPeriodicReports.Any() ? null : user.ManagementPeriodicReports.Select(_ => new ManagementPeriodicReportDAO(_)).ToList();
        }
    }
}
