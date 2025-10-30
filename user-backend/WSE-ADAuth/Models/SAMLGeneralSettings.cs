using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WSE_ADAuth.Models
{
    public class SAMLGeneralSettings
    {
        public string AssertionConsumerServiceURL { get; set; }
        public string Issuer { get; set; }
        public string SAML_IDPBaseURL { get; set; }
        public string AuthCertPath { get; set; }
        //public string PhoneNumberSAMLXMLPath { get; set; }
        public string PhoneNumberSAMLAttrName { get; set; }
        public string EmailSAMLAttrName { get; set; }
        public string InternalSAMLShortURL { get; set; }
        public string HostedAppHeaderKey { get; set; }
        public string HostedAppHeaderFirstName { get; set; }
        public string HostedAppHeaderLastName { get; set; }
        public string AssertionHeaderKey { get; set; }
        public string CertIDPrimer { get; set; }
        public bool UseUTF8 { get; set; }
        public bool SaveAuthToken { get; set; }
        public List<Guid> ApprovedCompanies { get; set; }





    }
}
