namespace WeSign.Models.Users
{
    using Common.Models.Configurations;

    public class UpdateUserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public UserConfigurationDTO UserConfiguration { get; set; }
        public string Username { get; set; }
    }
}
