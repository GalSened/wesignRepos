using Common.Interfaces.License;
using Common.Models.License;

namespace Common.Interfaces.UserApp
{
    public interface ILicense
    {
        /// <summary>
        /// Get license info, if license not active application will throw exception 
        /// </summary>
        /// <returns></returns>
        IWeSignLicense GetLicenseInformation();
    }
}
