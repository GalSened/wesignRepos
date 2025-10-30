namespace DAL.DAOs.Documents
{
    using Common.Models.Documents;
    using DAL.DAOs.Templates;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [Table("Documents")]
    public class DocumentDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentCollectionId { get; set; }
        public Guid TemplateId { get; set; }
        public string Name { get; set; }
        public virtual DocumentCollectionDAO DocumentCollection { get; set; }
        public virtual TemplateDAO Template { get; set; }
        public virtual ICollection<DocumentSignatureFieldDAO> SignatureFields { get; set; }

        public DocumentDAO() { }

        public DocumentDAO(Document document)
        {
            Id = document?.Id ?? default;
            TemplateId = document.TemplateId == Guid.Empty ? default : document.TemplateId;
            Name = document.Name;
            SignatureFields = document?.Fields?.SignatureFields?
                .Select(s => new DocumentSignatureFieldDAO
                {
                    DocumentId = Id,
                    Image = s.Image,
                    FieldName = s.Name
                }).ToList();
        }

        public DocumentDAO(DeletedDocument document)
        {
            Id = document?.Id ?? default;
        }
    }
}
