using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Documents.Signers
{
    public class Signer1Credential
    {
        public string Password { get; set; }
        public string CertificateId { get; set; }
        public bool ShouldUseADDetails { get; set; }
        public string SignerToken { get; set; }
    }
}
