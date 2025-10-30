using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WSE_ADAuth.Models;

namespace WSE_ADAuth.SAML
{
    public class SAMLRequestHandler : ISAMLRequest
    {
        private SAMLGeneralSettings _samlGeneralSettings;
        private string id;
         
      
        public SAMLRequestHandler(IOptions<SAMLGeneralSettings> samlGeneralSettings)
        {

            _samlGeneralSettings = samlGeneralSettings.Value;
            id = "_" + System.Guid.NewGuid().ToString();            
        }
        public string GetRequestId()
        {
            return id;
        }

        public string GetRequest(AuthRequestFormat format)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                using (XmlWriter xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("samlp", "AuthnRequest", Constants.XMLElenmentProtocol);
                    xw.WriteAttributeString("ID", id);
                    
                    xw.WriteAttributeString("Version", "2.0");
                    string issue_instant =  DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                    xw.WriteAttributeString("IssueInstant", issue_instant);
                    xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");

                       xw.WriteAttributeString("AssertionConsumerServiceURL", @_samlGeneralSettings.AssertionConsumerServiceURL);

                  

                    xw.WriteStartElement("saml", "Issuer", Constants.XMLElenmentAssertion);                    
                    xw.WriteString(_samlGeneralSettings.Issuer);
                    xw.WriteEndElement();

                    
                    xw.WriteStartElement("samlp", "RequestedAuthnContext", Constants.XMLElenmentProtocol);
                    xw.WriteAttributeString("Comparison", "exact");

                    xw.WriteStartElement("saml", "AuthnContextClassRef", Constants.XMLElenmentAssertion);
                    xw.WriteString("urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport");
                    xw.WriteEndElement();

                    xw.WriteEndElement(); 

                    xw.WriteEndElement();
                }

                if (format == AuthRequestFormat.Base64)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(new DeflateStream(memoryStream, CompressionMode.Compress, true), new UTF8Encoding(false));
                    writer.Write(sw);
                    writer.Close();
                    string result = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length, Base64FormattingOptions.None);
                    return result;
                }

                return null;
            }
        }
    }
}
