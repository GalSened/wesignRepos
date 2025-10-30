using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models.Documents.Signers;

namespace WeSign.Models.Documents.Responses
{
    public class SignerAttachmentResponseDTO
    {
        public string Name { get; set; }        
        public string Base64File { get; set; }

        public SignerAttachmentResponseDTO(SignerAttachment signerAttachment)
        {
            Name = signerAttachment.Name;
            Base64File = signerAttachment.Base64File;
        }
    }
}
