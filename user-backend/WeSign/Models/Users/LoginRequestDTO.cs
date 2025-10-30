namespace WeSign.Models.Users
{
    public class LoginRequestDTO
    {
        public string Email { get; set; } // Email or username (both in the same field)
        public string Password { get; set; }
    }
}
