using Common.Enums;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class Signer1FileSiging
    {
        public string Base64File { get; set; }
        public SigningFileType SigingFileType { get; set; }
        public Signer1Credential Signer1Credential { get; set; }

    }
}
