using System;
using System.Collections.Generic;

namespace WeSignSigner.Models.Requests
{
    public class UpdateDocumentDTO
    {
        public Guid DocumentId { get; set; }
        public IEnumerable<FieldDTO> Fields { get; set; }
    }
}