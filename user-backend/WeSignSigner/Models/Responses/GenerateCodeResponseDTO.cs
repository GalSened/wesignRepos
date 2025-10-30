using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignSigner.Models.Responses
{
    public class GenerateCodeResponseDTO
    {
        public string SentSignerMeans { get; set; }
        public Guid AuthToken { get; set; } = default;
    }
}
