using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Documents
{
    public class ReplaceSignerDTO
    {
        public string NewSignerName { get; set; }

        public string NewSignerMeans { get; set; }

    }
}
