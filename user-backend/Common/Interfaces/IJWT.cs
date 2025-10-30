namespace Common.Interfaces
{
    using Common.Models.Documents.Signers;
    using Models;

    public interface IJWT
    {
        string GenerateToken(User user);
        string GenerateSignerToken(Signer signer, int expiredLinkInHours = 0);
        bool CheckPasswordToken(string token);
        User GetUser(string token);
        User GetUserFromExpiredToken(string token);
        Signer GetSigner(string token);
        bool IsTokenExpired(string token);
    }
}
