using System;
using System.Collections.Generic;
using System.Text;

namespace PdfHandler.Interfaces
{
    public interface IPdfConverter
    {
        string Convert(string base64file, string dataInput);
    }
}
