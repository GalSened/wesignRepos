using Common.Models.Configurations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.DAOs.Companies
{
    [Table("CompanySigner1Details")]
    public class CompanySigner1DetailDAO
    {


        [Key]
        public Guid CompanyId { get; set; }
        //CertId
        public string Key1 { get; set; }
        //CertPassword
        public string Key2 { get; set; }
        public bool ShouldSignAsDefaultValue { get; set; }
        public bool ShouldShowInUI { get; set; }
        public virtual CompanyDAO Company { get; set; }
        public string Signer1Endpoint { get; set; }
        public string Signer1Password { get; set; }
        public string Signer1User { get; set; }

        public CompanySigner1DetailDAO() {}
        public CompanySigner1DetailDAO(CompanySigner1Details companySigner1Details)
        {
            Key1 = companySigner1Details.CertId;
            Key2 = companySigner1Details.CertPassword;
            ShouldShowInUI = companySigner1Details.ShouldShowInUI;
            ShouldSignAsDefaultValue = companySigner1Details.ShouldSignAsDefaultValue;
            Signer1Endpoint = companySigner1Details.Signer1Configuration?.Endpoint ?? String.Empty;
            Signer1Password = companySigner1Details.Signer1Configuration?.Password ?? String.Empty;
            Signer1User = companySigner1Details.Signer1Configuration?.User ?? String.Empty;
        }

    }
}
