namespace WeSign.Models.Users.Responses
{
    public class UserTokensResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string AuthToken { get; set; }
    }
}
