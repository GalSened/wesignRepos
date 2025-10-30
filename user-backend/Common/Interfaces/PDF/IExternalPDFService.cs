using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.PDF
{
    public interface IExternalPDFService
    {
        Task<string> Merge(List<string> templatesContents);
    }
}
