using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BL.Handlers.FilesHandler
{
    public class XMLHandler<T> : IXmlHandler<T>
    {
        private readonly IDataUriScheme _dataUriHandler;

        public XMLHandler(IDataUriScheme dataUriSchemeHandler)
        {
            _dataUriHandler = dataUriSchemeHandler;
        }
        public T ConvertBase64ToModel(string base64)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                var content = _dataUriHandler.Getbase64Content(base64);
                using (MemoryStream memoStream = new MemoryStream(Convert.FromBase64String(content)))
                {
                    T result = (T)serializer.Deserialize(memoStream);
                    return result;
                }
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException(ResultCode.InvalidXML.GetNumericString(), ex);
            }
        }

        public string ToXml(T model)
        {
            if (model.GetType().IsAssignableFrom(typeof(T)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (var sWriter = new Utf8StringWriter())
                {

                    using (XmlWriter writer = XmlWriter.Create(sWriter , new XmlWriterSettings { Encoding = Encoding.UTF8 }))
                    {
                        serializer.Serialize(writer, model);
                        return sWriter.ToString();
                    }
                }

            }
            throw new InvalidOperationException(ResultCode.InvalidModelToParseToXml.GetNumericString());
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        // Use UTF8 encoding but write no BOM to the wire
        public override Encoding Encoding
        {
            get { return new UTF8Encoding(false); } // in real code I'll cache this encoding.
        }
    }
}
