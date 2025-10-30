namespace WeSign.Models.Users.Responses
{
    using Common.Enums.Users;
    using Common.Models;
    using Common.Models.Configurations;
    using System;

    public class UserResponseDTO
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public UserType Type { get; set; }
        public DateTime CreationTime { get; set; }
        public UserConfiguration UserConfiguration { get; set; }
        public Guid? ProgramUtilizationId { get; set; }
        public ProgramResponseDTO Program { get; set; }
        public string CompanyLogo { get; set; }
        public CompanySigner1Details CompanySigner1Details { get; set; }
        public DateTime LastSeen { get; set; }
        public string Username { get; set; }

        public UserResponseDTO() { }

        public UserResponseDTO(User user, CompanySigner1Details companySigner1Details)
        {
            Id = user.Id;
            GroupId = user.GroupId;
            CompanyId = user.CompanyId;
            Name = user.Name;
            Email = user.Email;
            Type = user.Type;
            CreationTime = user.CreationTime;
            ProgramUtilizationId = user.ProgramUtilization?.Id;
            UserConfiguration = user.UserConfiguration;
            CompanyLogo = user.CompanyLogo;
            Program = new ProgramResponseDTO(user.ProfileProgram);
            CompanySigner1Details = companySigner1Details;
            LastSeen = user?.LastSeen ?? DateTime.MinValue;
            Username = user.Username;
        }
    }
}
