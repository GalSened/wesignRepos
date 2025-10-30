namespace Common.Models.Documents.Signers
{
    using System;

    public class Notes
    {
        public Guid Id { get; set; }
        public string SignerNote { get; set; }
        public string UserNote { get; set; }

    }
}
