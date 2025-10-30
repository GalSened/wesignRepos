namespace Common.Models.Documents.Signers
{
    using System;

    public class SignerTokenMapping
    {
        public Guid DocumentCollectionId { get; set; }
        public Guid SignerId { get; set; }
        public Guid GuidToken { get; set; }
        public Guid GuidAuthToken { get; set; }
        public string JwtToken { get; set; }
        public string AuthToken { get; set; }
        public string ADName { get; set; }
        public string AuthName { get; set; }
        public string AuthId { get; set; }
    }
}
