using System.Collections.Generic;

namespace WeSignManagement.Models.Users
{
    public class AllUsersResponseDTO
    {
        public IEnumerable<UserManagementResponseDTO> Users { get; set; }
        public AllUsersResponseDTO()
        {

        }
    }
}
