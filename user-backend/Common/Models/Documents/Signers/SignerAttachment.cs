namespace Common.Models.Documents.Signers   
{
    using System;

    public class SignerAttachment
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsMandatory { get; set; }
        public string Base64File { get; set; }
    }
}
