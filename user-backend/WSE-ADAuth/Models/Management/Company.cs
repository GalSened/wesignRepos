using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WSE_ADAuth.Models.Management
{
    public class Company
    {

        public string CompanyName { get; set; }
        public string LogoBase64String { get; set; }
        public string SignatureColor { get; set; }
        public Language Language { get; set; }
        public BaseUser User { get; set; }
        public string ProgramId { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
