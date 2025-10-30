using Common.Enums.License;
using Common.Interfaces.License;
using Common.Models;
using Common.Models.License;
using Common.Models.ManagementApp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.ManagementApp
{
    public interface ILicense
    {
        Task<GenerateLicenseKeyResponse> GenerateLicense(UserInfo userInfo);
        
        LicenseStatus ActivateLicense(string license);
        
        /// <summary>
        /// Get license info, if license not active application will throw exception 
        /// </summary>
        /// <returns></returns>
        Task<(IWeSignLicense, LicenseCounters licenseUsage)> GetLicenseInformationAndUsing( bool readUsage);

        Task ValidateProgramAddition(Program companyProgram, IEnumerable<Company> companies);
        IWeSignLicense ReadLicenseInformation();
    }
}
