namespace DAL.DAOs.Documents.Signers
{
    using Common.Models.Documents.Signers;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SignerTokensMapping")]
    public class SignerTokenMappingDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SignerId { get; set; }
        public Guid DocumentCollectionId { get; set; }
        public Guid GuidToken { get; set; }
        public Guid GuidAuthToken { get; set; }
        public string AuthToken { get; set; }
        public string JwtToken { get; set; }
        public string ADName { get; set; }
        public string AuthName { get; set; }
        public string AuthId { get; set; }
        public virtual DocumentCollectionDAO DocumentCollection { get; set; }

        public SignerTokenMappingDAO() { }

        public SignerTokenMappingDAO(SignerTokenMapping signerTokenMapping)
        {
            if (signerTokenMapping != null)
            {
                DocumentCollectionId = signerTokenMapping.DocumentCollectionId;
                SignerId = signerTokenMapping.SignerId;
                GuidToken = signerTokenMapping.GuidToken;
                GuidAuthToken = signerTokenMapping.GuidAuthToken;
                JwtToken = signerTokenMapping.JwtToken;
                ADName = signerTokenMapping.ADName;
                AuthToken = signerTokenMapping.AuthToken;
                AuthName = signerTokenMapping.AuthName;
                AuthId = signerTokenMapping.AuthId;
            }
        }
    }
}
