/*
 * ProgramUtilizationId in Free_Accounts Company will be null
 */
namespace DAL.DAOs.Companies
{
    using Common.Enums.Companies;
    using Common.Models;
    using DAL.DAOs.ActiveDirectory;
    using DAL.DAOs.Groups;
    using DAL.DAOs.Programs;
    using DAL.DAOs.Users;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Companies")]
    public class CompanyDAO
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ProgramId { get; set; }
        public Guid? ProgramUtilizationId { get; set; }
        public CompanyStatus Status { get; set; }
        public string TransactionId { get; set; }
        public virtual ICollection<UserDAO> Users { get; set; }
        public virtual ICollection<GroupDAO> Groups { get; set; }
        public virtual CompanyConfigurationDAO CompanyConfiguration { get; set; }
        public virtual ProgramDAO Program { get; set; }
        public virtual ProgramUtilizationDAO ProgramUtilization { get; set; }
        public virtual ActiveDirectoryConfigDAO ActiveDirectoryConfig { get; set; }
        public virtual CompanySigner1DetailDAO CompanySigner1Details { get; set; }

        public CompanyDAO() { }

        public CompanyDAO(Company company)
        {
            Id = company.Id == Guid.Empty ? default : company.Id;
            ProgramId = company.ProgramId == Guid.Empty ? default : company.ProgramId;
            Name = company.Name;
            Status = company.Status;
            ProgramUtilizationId = company.ProgramUtilization?.Id ?? null;
            CompanyConfiguration = new CompanyConfigurationDAO(company.CompanyConfiguration);
            ProgramUtilization = new ProgramUtilizationDAO(company.ProgramUtilization);
            CompanySigner1Details = new CompanySigner1DetailDAO(company.CompanySigner1Details);
            TransactionId = company.TransactionId;
        }
    }
}
