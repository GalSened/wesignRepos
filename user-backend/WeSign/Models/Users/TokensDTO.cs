namespace WeSign.Models.Users
{
    public class TokensDTO
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public string AuthToken { get; set; }

    }
}
