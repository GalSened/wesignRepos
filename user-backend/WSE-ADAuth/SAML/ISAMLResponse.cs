using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WSE_ADAuth.SAML
{
   public interface ISAMLResponse
    {
        void LoadXml(string xml);
        void LoadXmlFromBase64(string response);
        bool IsValid();
        string GetUserEmail();
        string GetResponseId();
        List<string> GetUserPhoneNumbers();
        string GetUserNameID();
        string GetSamlXml();

    }
}
