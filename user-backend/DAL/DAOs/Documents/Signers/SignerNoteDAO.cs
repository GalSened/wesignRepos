namespace DAL.DAOs.Documents.Signers
{
    using Common.Models.Documents.Signers;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Notes")]
    public class NotesDAO
    {
        public Guid Id { get; set; }
        public Guid SignerId { get; set; }
        public string SignerNote { get; set; }
        public string UserNote { get; set; }
        public virtual SignerDAO Signer { get; set; }

        public NotesDAO() { }
        public NotesDAO(Notes notes)
        {
            if (notes != null)
            {
                Id = notes.Id == Guid.Empty ? default : notes.Id;
                UserNote = notes.UserNote;
                SignerNote = notes.SignerNote;
            }
        }
    }
}
