using Common.Models.Files.PDF;
using System.Collections.Generic;

namespace Common.Models.Documents.Signers
{
    public class BaseSigner
    {
        public string FullName { get; set; }
        public string SignerMeans { get; set; }
        public string SignerSecondaryMeans { get; set; }
        public string PhoneExtension { get; set; }
        public bool ShouldSendOTP { get; set; }
        public List<FieldNameToValuePair> Fields { get; set; }
        public BaseSigner()
        {
            PhoneExtension = "+972";
            Fields = new List<FieldNameToValuePair>();
        }
    }
}
