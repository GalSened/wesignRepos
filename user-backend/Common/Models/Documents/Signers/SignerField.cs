namespace Common.Models.Documents.Signers
{
    using System;

    public class SignerField
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid ContactId { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public bool IsMandatory { get; set; }
    }
}
