namespace Common.Interfaces
{
    using Common.Models;
    using Common.Models.Configurations;
    using System.Security.Cryptography.X509Certificates;

    public interface ICertificate
    {
        void Create(Contact contact, CompanyConfiguration companyConfiguration);
        X509Certificate2 Get(Contact contact, CompanyConfiguration companyConfiguration);
        void Delete(Contact contact);
        void Create(User user, CompanyConfiguration companyConfiguration);
        X509Certificate2 Get(User user, CompanyConfiguration companyConfiguration);
        void Delete(User user);
        //(string, string) GetPfxInfo(Contact contact);
        
        
    }
}
