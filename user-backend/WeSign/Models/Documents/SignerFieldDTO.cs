namespace WeSign.Models.Documents
{
    using System;

    public class SignerFieldDTO
    {
        public Guid TemplateId { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }
}
