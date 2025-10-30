using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfHandler.Interfaces
{
    public interface IPdfMerge
    {
        string MergePdf(List<string> documents);
    }
}
