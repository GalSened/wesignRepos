using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class Signer1FileSigingResult
    {
        public byte[] Base64SignedFile { get; set; }
        public SigningFileType SigingFileType { get; set; }

    }
}
