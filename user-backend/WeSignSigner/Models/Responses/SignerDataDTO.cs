using Common.Models.Documents.Signers;
using System.Collections.Generic;
using System.Linq;

namespace WeSignSigner.Models.Responses
{
    public class SignerDataDTO
    {
        public string Name { get; set; }
        public bool IsLastSigner { get; set; }
        public bool AreAllOtherSignersSigned { get; set; }
        public IEnumerable<SignerAttachment> Attachments { get; set; }
        public string Seal { get; set; }
        public string Means { get; set; }
        public string ADName{ get; set; }
        public string AuthToken { get; set; }
        public string Note { get; set; }

        public SignerDataDTO()
        {
            Attachments = new List<SignerAttachment>();
        }
    }
}
