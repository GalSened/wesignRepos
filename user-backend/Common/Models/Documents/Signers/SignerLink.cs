namespace Common.Models.Documents.Signers
{
    using System;

    public class SignerLink
    {
        public Guid SignerId { get; set; }
        public string Link { get; set; }
    }
}
