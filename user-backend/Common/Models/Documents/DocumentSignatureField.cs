using System;

namespace Common.Models.Documents
{
    public class DocumentSignatureField
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string FieldName { get; set; }
        public string Image { get; set; }
    }
}
