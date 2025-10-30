using System;
using System.Collections.Generic;

namespace Common.Models.Documents
{
    public class DeletedDocumentTemplate
    {
        public Guid TemplateId { get; set; }
        public ICollection<DeletedTemplateSignatureField>? TemplateSignatureFields { get; set; } = new List<DeletedTemplateSignatureField>();
    }
}
