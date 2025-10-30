namespace WSE_ADAuth.Models
{
    public class UserTokensResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string AuthToken { get; set; }
    }
}
