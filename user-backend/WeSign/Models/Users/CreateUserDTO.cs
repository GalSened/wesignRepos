namespace WeSign.Models.Users
{
    using Common.Enums.Users;

    public class CreateUserDTO
    {
        public string Name { get; set; }
        public Language Language { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ReCAPCHA { get; set; }
        public bool SendActivationLink { get; set; } = true;
        public string Username { get; set; }
    }
}
