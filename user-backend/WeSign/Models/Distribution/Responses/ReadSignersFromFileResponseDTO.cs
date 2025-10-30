using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Distribution.Responses
{
    public class ReadSignersFromFileResponseDTO
    {
        public IEnumerable<BaseSigner> Signers { get; set; }        
    }
}
