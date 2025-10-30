namespace WeSign.Models.Admins.Response
{
    using System.Collections.Generic;

    public class AdminAllUsersResponseDTO
    {
        public IEnumerable<UserResponseAdminDTO> Users { get; set; }
    }


}
