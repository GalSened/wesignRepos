namespace Common.Models
{
    using Common.Enums.Companies;
    using Common.Models.Configurations;
    using Common.Models.Programs;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CompanyStatus Status { get; set; }
        public Guid ProgramId { get; set; }
        public ProgramUtilization ProgramUtilization { get; set; }
        public CompanyConfiguration CompanyConfiguration { get; set; }
        public CompanySigner1Details CompanySigner1Details { get; set; }
        public string TransactionId { get;  set; }

        public Company()
        {
            Status = CompanyStatus.Created;
            CompanyConfiguration = new CompanyConfiguration();
            CompanySigner1Details = new CompanySigner1Details();
        }
    }
}
