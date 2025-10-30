namespace DAL.Extensions
{
    using Common.Models.Documents.Signers;
    using DAL.DAOs.Documents.Signers;

    public static class SignerTokenMappingExtensions
    {
        public static SignerTokenMapping ToSignerTokenMapping(this SignerTokenMappingDAO signerTokenMappingDAO)
        {
            return signerTokenMappingDAO == null ? null : new SignerTokenMapping()
            {
                DocumentCollectionId = signerTokenMappingDAO.DocumentCollectionId,
                SignerId = signerTokenMappingDAO.SignerId,
                GuidToken = signerTokenMappingDAO.GuidToken,
                GuidAuthToken = signerTokenMappingDAO.GuidAuthToken,
                JwtToken = signerTokenMappingDAO.JwtToken,
                ADName = signerTokenMappingDAO.ADName,
                AuthToken = signerTokenMappingDAO.AuthToken,
                AuthId = signerTokenMappingDAO.AuthId,
                AuthName = signerTokenMappingDAO.AuthName,
            };
        }

        public static SignerTokenMappingDAO ToSignerTokenMappingDAOs(this SignerTokenMapping signerTokenMapping)
        {
            return signerTokenMapping == null ? null : new SignerTokenMappingDAO
            {
                DocumentCollectionId = signerTokenMapping.DocumentCollectionId,
                GuidToken = signerTokenMapping.GuidToken,
                GuidAuthToken = signerTokenMapping.GuidAuthToken,
                SignerId = signerTokenMapping.SignerId,
                JwtToken = signerTokenMapping.JwtToken,
                ADName = signerTokenMapping.ADName,
                AuthToken = signerTokenMapping.AuthToken,
                AuthName = signerTokenMapping.AuthName,
                AuthId = signerTokenMapping.AuthId,
            };
        }
    }
}