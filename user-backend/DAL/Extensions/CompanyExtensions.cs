using Common.Models;
using DAL.DAOs.Companies;
using System.Linq;

namespace DAL.Extensions
{
    public static class CompanyExtensions
    {
        public static Company ToCompany(this CompanyDAO companyDAO)
        {
            return companyDAO == null ? null : new Company()
            {
                Id = companyDAO.Id,
                Name = companyDAO.Name,
                ProgramId = companyDAO.ProgramId,
                Status = companyDAO.Status,
                TransactionId = companyDAO.TransactionId,
                ProgramUtilization = companyDAO.ProgramUtilization.ToProgramUtilization(),
                CompanyConfiguration = companyDAO.CompanyConfiguration.ToCompanyConfiguration(),
                CompanySigner1Details = companyDAO.CompanySigner1Details.ToCompanySigner1Details(),
            };
        }
    }
}
