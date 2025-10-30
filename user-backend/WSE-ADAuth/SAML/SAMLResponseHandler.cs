using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using WSE_ADAuth.Models;
using Serilog;
using WSE_ADAuth.Extensions;
using System.Collections.Generic;
using System.Text;

namespace WSE_ADAuth.SAML
{
    public class SAMLResponseHandler : ISAMLResponse
    {

        private  XmlDocument _xmlDoc;
        private readonly X509Certificate2 _cert;
        private readonly IOptions<SAMLGeneralSettings> _samlGeneralSettings;
        private readonly ILogger _logger;
        private string _samlXmlString;

        public SAMLResponseHandler(IOptions<SAMLGeneralSettings> samlGeneralSettings, ILogger logger)
        {
            _samlGeneralSettings = samlGeneralSettings;
            _logger = logger;
            try
            {
                if (!string.IsNullOrWhiteSpace(_samlGeneralSettings.Value.AuthCertPath))
                {
                    _cert = new X509Certificate2(_samlGeneralSettings.Value.AuthCertPath);
                }
            }
            catch (Exception)
            {

            }
            
            _samlXmlString = "";


        }
       
        public string GetUserNameID()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace(Constants.SAML, Constants.XMLElenmentAssertion);                                          
            manager.AddNamespace(Constants.SAMLP, Constants.XMLElenmentProtocol);

            XmlNode node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:NameID", manager);
            return node.InnerText;
        }

        public bool IsValid()
        {
            bool status = true;

            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace(Constants.SAML, Constants.XMLElenmentAssertion);
            manager.AddNamespace(Constants.SAMLP, Constants.XMLElenmentProtocol);
            XmlNodeList nodeList = _xmlDoc.SelectNodes("//ds:Signature", manager);

            SignedXml signedXml = new SignedXml(_xmlDoc);
            signedXml.LoadXml((XmlElement)nodeList[0]);

            status &= signedXml.CheckSignature(_cert, true);

            // need to check the time - some will work with UTC and some with local time - need to add it to the configuration 
            var notBefore = NotBefore();
            status &= !notBefore.HasValue || (notBefore <= DateTime.Now);

            var notOnOrAfter = NotOnOrAfter();
            status &= !notOnOrAfter.HasValue || (notOnOrAfter > DateTime.Now);

            return status;
        }

        public void LoadXml(string xml)
        {
            _xmlDoc = new XmlDocument();
            _xmlDoc.PreserveWhitespace = true;
            _xmlDoc.XmlResolver = null;
            _xmlDoc.LoadXml(xml);
        }

        public void LoadXmlFromBase64(string response)
        {
            Encoding enc = _samlGeneralSettings.Value.UseUTF8 ? new System.Text.UTF8Encoding() : new System.Text.ASCIIEncoding();
            _samlXmlString = enc.GetString(Convert.FromBase64String(response));
            LoadXml(_samlXmlString);
        }

        public string GetSamlXml()
        {
            return _samlXmlString;
        }
        private DateTime? NotBefore()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace(Constants.SAML, Constants.XMLElenmentAssertion);
            manager.AddNamespace(Constants.SAMLP, Constants.XMLElenmentProtocol);

            var nodes = _xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:Conditions", manager);
            string value = null;
            if (nodes != null && nodes.Count > 0 && nodes[0] != null && nodes[0].Attributes != null && nodes[0].Attributes["NotBefore"] != null)
            {
                value = nodes[0].Attributes["NotBefore"].Value;
            }
            return value != null ? DateTime.Parse(value) : (DateTime?)null;
        }
        private DateTime? NotOnOrAfter()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace(Constants.SAML, Constants.XMLElenmentAssertion);
            manager.AddNamespace(Constants.SAMLP, Constants.XMLElenmentProtocol);

            var nodes = _xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:Conditions", manager);
            string value = null;
            if (nodes != null && nodes.Count > 0 && nodes[0] != null && nodes[0].Attributes != null && nodes[0].Attributes["NotOnOrAfter"] != null)
            {
                value = nodes[0].Attributes["NotOnOrAfter"].Value;
            }
            return value != null ? DateTime.Parse(value) : (DateTime?)null;
        }

        public string GetResponseId()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace(Constants.SAML, Constants.XMLElenmentAssertion);
            manager.AddNamespace(Constants.SAMLP, Constants.XMLElenmentProtocol);

            var nodes = _xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:Subject/saml:SubjectConfirmation/saml:SubjectConfirmationData", manager);
            string value = null;
            if (nodes != null && nodes.Count > 0 && nodes[0] != null && nodes[0].Attributes != null && nodes[0].Attributes["InResponseTo"] != null)
            {
                value = nodes[0].Attributes["InResponseTo"].Value;
            }
            return value ;
        }


        public string GetUserEmail()
        {
            string value = "";
            if (!string.IsNullOrWhiteSpace(_samlGeneralSettings.Value.EmailSAMLAttrName))
            {
                XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
                manager.AddNamespace(Constants.SAML, Constants.XMLElenmentAssertion);
                manager.AddNamespace(Constants.SAMLP, Constants.XMLElenmentProtocol);

                var nodes = _xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:AttributeStatement", manager);

                if (nodes != null && nodes.Count > 0 && nodes[0] != null && nodes[0].ChildNodes != null && nodes[0].ChildNodes.Count > 0)
                {
                    foreach (XmlNode child in nodes[0].ChildNodes)
                    {
                        if (child.Attributes["Name"]?.Value.ToLower() == _samlGeneralSettings.Value.EmailSAMLAttrName.ToLower())
                        {
                            value = child.ChildNodes[0].InnerText;
                            break;
                        }
                    }

                }
            }
            return value;
        }

        public List<string> GetUserPhoneNumbers()
        {
            string value = "";
            if (!string.IsNullOrWhiteSpace(_samlGeneralSettings.Value.PhoneNumberSAMLAttrName))
            {
                XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
                manager.AddNamespace(Constants.SAML, Constants.XMLElenmentAssertion);
                manager.AddNamespace(Constants.SAMLP, Constants.XMLElenmentProtocol);

                var nodes = _xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:AttributeStatement", manager);

                if (nodes != null && nodes.Count > 0 && nodes[0] != null && nodes[0].ChildNodes != null && nodes[0].ChildNodes.Count > 0)
                {
                    foreach(XmlNode child in nodes[0].ChildNodes)
                    {
                        if(child.Attributes["Name"]?.Value.ToLower() == _samlGeneralSettings.Value.PhoneNumberSAMLAttrName.ToLower())
                        {
                            value = child.ChildNodes[0].InnerText;
                            break;
                        }
                    }
                    
                }
            }
            return value.GetAllPhones(); 
        }

    }
   
}
