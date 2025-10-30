using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class SelfSignUpdateDocumentResult
    {
        public Guid Token { get; set; }
        public string RedirectUrl { get; set; }
    }
}
