using Common.Hubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.PDF
{
    public interface IPdfPackage
    {
        PrepResponse PrepareSignatureForField(string[] fieldName, byte[] image, byte[] pdf);
        SetResponse SetSignature(byte[] pdf, byte[] signedHash);
    }
}
