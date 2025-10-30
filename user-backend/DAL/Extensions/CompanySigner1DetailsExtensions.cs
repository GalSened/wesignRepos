using Common.Models.Configurations;
using DAL.DAOs.Companies;

namespace DAL.Extensions
{
    public static class CompanySigner1DetailsExtensions
    {
        public static CompanySigner1Details ToCompanySigner1Details(this CompanySigner1DetailDAO companySigner1DetailDAO)
        {
            return companySigner1DetailDAO == null ? null : new CompanySigner1Details()
            {
                CertId = companySigner1DetailDAO.Key1,
                CertPassword = companySigner1DetailDAO.Key2,
                ShouldShowInUI = companySigner1DetailDAO.ShouldShowInUI,
                ShouldSignAsDefaultValue = companySigner1DetailDAO.ShouldSignAsDefaultValue,
                Signer1Configuration = new Signer1Configuration()
                {
                    Endpoint = companySigner1DetailDAO.Signer1Endpoint,
                    User = companySigner1DetailDAO.Signer1User,
                    Password = companySigner1DetailDAO.Signer1Password
                }
                
            };
        }
    }
}
