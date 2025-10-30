namespace WeSign.Models.Admins.Response
{
    using System.Collections.Generic;

    public class AdminAllGroupsResponseDTO
    {
        public IEnumerable<GroupResponseAdminDTO> Groups { get; set; }
    }
}
