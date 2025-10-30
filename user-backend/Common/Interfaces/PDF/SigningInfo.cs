using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
namespace Common.Interfaces.PDF
{
    public class SigningInfo
    {
        public SignerAuthentication SignerAuthentication { get; set; }
        public X509Certificate2 Certificate { get; set; }
        public IEnumerable<SignatureField> Signatures { get; set; }
        public byte[] Data { get; set; }
        
        public string Reason { get; set; }
        public CompanySigner1Details CompanySigner1Details { get; set; }

    }
}