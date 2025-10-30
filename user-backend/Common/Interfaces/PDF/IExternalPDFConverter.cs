using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.PDF
{
    public  interface IExternalPDFConverter
    {

        string ConvertToPDF(string data, FileType fileType);
    }
}
