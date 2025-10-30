
namespace DAL.DAOs.Documents
{
    
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DocumentSignatureFields")]
    public class DocumentSignatureFieldDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string FieldName { get; set; }
        public string Image { get; set; }
        public virtual DocumentDAO Document{ get; set; }
    }
}
