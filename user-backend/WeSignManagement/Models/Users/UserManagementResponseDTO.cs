using Common.Enums.Users;
using Common.Models.ManagementApp;
using System;

namespace WeSignManagement.Models.Users
{
    public class UserManagementResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreationTime { get; set; }
        public Language Language { get; set; }
        public UserType Type { get; set; }
        public string CompanyName { get; set; }
        public string GroupName { get; set; }
        public string ProgramName { get; set; }
        public DateTime LastSeen { get; set; }
        public string Username { get; set; }

        public UserManagementResponseDTO(UserDetails userDetails)
        {
            Id = userDetails.Id;
            Name = userDetails?.Name;
            Email = userDetails?.Email;
            Username = userDetails?.Username;
            CreationTime = userDetails?.CreationTime ?? DateTime.MinValue;
            Language = userDetails?.Language?? Language.en;
            Type = userDetails?.Type ?? UserType.Basic;
            CompanyName = userDetails?.CompanyName;
            ProgramName = userDetails?.ProgramName;
            GroupName= userDetails?.GroupName;
            LastSeen = userDetails?.LastSeen ?? DateTime.MinValue;
        }
    }
}
