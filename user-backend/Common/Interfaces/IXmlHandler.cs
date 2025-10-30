using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IXmlHandler<T>
    {
        T ConvertBase64ToModel(string base64);
        string ToXml(T obj);

    }
}
