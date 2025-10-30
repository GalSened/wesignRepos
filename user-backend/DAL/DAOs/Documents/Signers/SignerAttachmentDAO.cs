namespace DAL.DAOs.Documents.Signers
{
    using Common.Models.Documents.Signers;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SignerAttachments")]
    public class SignerAttachmentDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SignerId { get; set; }
        public string Name { get; set; }
        public bool IsMandatory { get; set; }
        public virtual SignerDAO Signer { get; set; }

        public SignerAttachmentDAO() { }
        public SignerAttachmentDAO(SignerAttachment signerAttachment)
        {
            if (signerAttachment != null)
            {
                Id = signerAttachment.Id == Guid.Empty ? default : signerAttachment.Id;
                Name = signerAttachment.Name;
                IsMandatory = signerAttachment.IsMandatory;
            }
        }

    }
}
