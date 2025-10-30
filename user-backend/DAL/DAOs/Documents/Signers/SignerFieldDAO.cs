namespace DAL.DAOs.Documents
{
    using Common.Models.Documents.Signers;
    using DAL.DAOs.Documents.Signers;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SignerFields")]
    public class SignerFieldDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SignerId { get; set; }
        public Guid DocumentId { get; set; }
        public string FieldName { get; set; }
        public virtual SignerDAO Signer { get; set; }

        public SignerFieldDAO(){}

        public SignerFieldDAO(SignerField signerField)
        {
            if(signerField != null)
            {
                Id = signerField.Id == Guid.Empty ? default : signerField.Id;
                DocumentId = signerField.DocumentId == Guid.Empty ? default : signerField.DocumentId;
                FieldName = signerField.FieldName;
            }
        }
    }
}
