namespace DAL.DAOs.Templates
{
    using Common.Enums.PDF;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TemplateSignatureFields")]
    public class TemplateSignatureFieldDAO
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string Name { get; set; }
        public SignatureFieldType SignaturFieldType { get; set; }
        public SignatureFieldKind SignatureKind { get; set; }
        public bool Mandatory { get; set; }
        public virtual TemplateDAO Template { get; set; }
    }
}
