using Common.Enums;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.SelfSign
{
    public class Signer1FileSigingDTO
    {
        public string Base64File { get; set; }
        public SigningFileType SigingFileType { get; set; }
        public Signer1Credential Signer1Credential { get; set; }
        public string FileName { get; set; }
    }
}
