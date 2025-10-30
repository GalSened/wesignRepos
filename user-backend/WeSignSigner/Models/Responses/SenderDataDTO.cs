using Common.Models.Documents;
using System.Collections;
using System.Collections.Generic;

namespace WeSignSigner.Models.Responses
{
    public class SenderDataDTO
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public IEnumerable<string> Appendices { get; set; }
        public string Base64Logo { get; set; }
        public string SignatureColor { get; set; }
        public bool IsSmsProviderSupportGloballySend { get; set; }
    }
}
