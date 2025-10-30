using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace WeSignSetup.Handlers
{
    public class WebConfigsHandler
    {
        private readonly LogHandler _logHandler;

        public WebConfigsHandler(LogHandler logHandler)
        {
            _logHandler = logHandler;
        }

        public void UpdateSoapServiceConfig(string fileName, string baseUrl)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    _logHandler.Debug($"WebConfigsHandler - UpdateSoapServiceConfig - File [{fileName}] exist, start updating...");
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(fileName);
                    XmlNode node = xmlDocument.SelectSingleNode("configuration/appSettings/add");
                    var nodes = xmlDocument.SelectNodes("configuration/appSettings/add");
                    foreach (XmlNode add in nodes)
                    {
                        var attribute = add.Attributes["key"];
                        if (attribute.Value == "BaseUrl")                            
                        {
                            add.Attributes["value"].Value = baseUrl;
                        }
                    }

                    xmlDocument.Save(fileName);
                    _logHandler.Debug($"WebConfigsHandler - UpdateSoapServiceConfig - Successfully update File [{fileName}]");
                }
                else
                {
                    _logHandler.Debug($"WebConfigsHandler - UpdateSoapServiceConfig - File [{fileName}] not exist");
                }
            }
            catch (Exception ex)
            {
                _logHandler.Error($"WebConfigsHandler - UpdateSoapServiceConfig - Error ", ex);
            }
        }

        public void UpdateBackendWebConfig(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    _logHandler.Debug($"WebConfigsHandler - UpdateBackendWebConfig - File [{filename}] exist, start updating...");
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(filename);
                    var securityEl = xmlDocument.CreateElement("security");
                    var requestFilteringEl = xmlDocument.CreateElement("requestFiltering");
                    XmlAttribute attr = xmlDocument.CreateAttribute("removeServerHeader");
                    attr.Value = "true";
                    requestFilteringEl.Attributes.Append(attr);
                    securityEl.AppendChild(requestFilteringEl);

                    var httpProtocolEl = xmlDocument.CreateElement("httpProtocol");
                    var customHeadersEl = xmlDocument.CreateElement("customHeaders");
                    var removeEl = xmlDocument.CreateElement("remove");
                    XmlAttribute nameAttr = xmlDocument.CreateAttribute("name");
                    nameAttr.Value = "X-Powered-By";
                    removeEl.Attributes.Append(nameAttr);
                    customHeadersEl.AppendChild(removeEl);
                    httpProtocolEl.AppendChild(customHeadersEl);


                    var main = xmlDocument.SelectSingleNode("//system.webServer");

                    var sec = main.SelectSingleNode("//security");
                    if (sec == null)
                    {
                        main.PrependChild(securityEl);
                    }

                    var httpProtocol = main.SelectSingleNode("//httpProtocol");
                    if (httpProtocol == null)
                    {
                        main.AppendChild(httpProtocolEl);
                    }

                    xmlDocument.Save(filename);
                    _logHandler.Debug($"WebConfigsHandler - UpdateBackendWebConfig - Successfully update File [{filename}]");
                }
                else
                {
                    _logHandler.Debug($"WebConfigsHandler - UpdateBackendWebConfig - File [{filename}] not exist");
                }
            }
            catch (Exception ex)
            {
                _logHandler.Error($"WebConfigsHandler - UpdateBackendWebConfig - Error ", ex);
            }
        }

        public void UpdateFrontendWebConfig(string filename, IEnumerable<string> routes)
        {
            try
            {
                if (File.Exists(filename))
                {
                    _logHandler.Debug($"WebConfigsHandler - UpdateFrontendWebConfig - File [{filename}] exist, start updating...");
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(filename);
                    var adds = xmlDocument.SelectNodes("//add");
                    //Remove old urls
                    foreach (XmlNode item in adds)
                    {
                        var attribute = item.Attributes["input"];
                        if (attribute.Value == "{REQUEST_URI}")
                        {
                            item.ParentNode.RemoveChild(item);
                        }
                    }                    
                    //Add new urls
                    var conditions = xmlDocument.SelectNodes("//conditions");                    
                    foreach (XmlNode condition in conditions)
                    {
                        foreach (XmlNode item in condition.ChildNodes)
                        {
                            var attribute = item.Attributes["input"];
                            if (attribute.Value == "{REQUEST_FILENAME}")
                            {
                                foreach (var route in routes)
                                {
                                    var addEl = xmlDocument.CreateElement("add");
                                    XmlAttribute nameAttr = xmlDocument.CreateAttribute("input");
                                    nameAttr.Value = "{REQUEST_URI}";
                                    XmlAttribute patternAttr = xmlDocument.CreateAttribute("pattern");
                                    patternAttr.Value = $"/{route}(.*)$";
                                    XmlAttribute negateAttr = xmlDocument.CreateAttribute("negate");
                                    negateAttr.Value = "true";
                                    addEl.Attributes.Append(nameAttr);
                                    addEl.Attributes.Append(patternAttr);
                                    addEl.Attributes.Append(negateAttr);
                                    condition.AppendChild(addEl);
                                }
                                break;
                            }
                        }
                    }

                    xmlDocument.Save(filename);
                    _logHandler.Debug($"WebConfigsHandler - UpdateFrontendWebConfig - Successfully update File [{filename}]");
                }
                else
                {
                    _logHandler.Debug($"WebConfigsHandler - UpdateFrontendWebConfig - File [{filename}] not exist");
                }
            }
            catch (Exception ex)
            {
                _logHandler.Error($"WebConfigsHandler - UpdateFrontendWebConfig - Error ", ex);
            }
        }

    }
}