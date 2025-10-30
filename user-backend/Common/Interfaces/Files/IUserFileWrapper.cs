
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Files
{
    public interface IUserFileWrapper
    {
        void DeleteCertificate(User user);
        byte[] ReadCertificate(User user);
        void SetCompanyLogo(User user);
        void SaveCertificate(User user , byte[] cert);
        bool IsCertificateExist(User user);

    }
}
