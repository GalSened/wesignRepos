using Common.Models;
using Common.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Files
{
    public interface IContactFileWrapper
    {
        void SaveSeals(Contact contact);
        void DeleteSeals(Contact contact);
        void SetSealsData(Contact contact);
        void UpdateSeals(Contact contact, List<string> signaturesImages);
        List<string> ReadSeals(Contact contact);
        bool IsCertificateExist(Contact contact);
        void DeleteCertificate(Contact contact);
        byte[] ReadCertificate(Contact contact, CompanyConfiguration companyConfiguration);
        void SaveCertificate(Contact contact, byte[] cert);
    }
}
