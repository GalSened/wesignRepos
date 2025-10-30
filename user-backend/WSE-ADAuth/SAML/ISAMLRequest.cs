using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static WSE_ADAuth.SAML.SAMLRequestHandler;

namespace WSE_ADAuth.SAML
{
    public interface ISAMLRequest
    {
      
        string GetRequest(AuthRequestFormat format);
        string GetRequestId();

    }
}
