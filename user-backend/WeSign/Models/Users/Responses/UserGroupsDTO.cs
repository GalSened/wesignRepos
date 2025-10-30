using Common.Models;
using System.Collections.Generic;
using System.Linq;
using WeSign.Models.Admins.Response;

namespace WeSign.Models.Users.Responses
{
    public class UserGroupsDTO
    {
        public List<GroupResponseAdminDTO> Groups { get; set; } = new List<GroupResponseAdminDTO>();
        public UserGroupsDTO() { }
        public UserGroupsDTO(List<Group> inputGroups)
        {
            foreach (var group in inputGroups ?? Enumerable.Empty<Group>()) {
                Groups.Add(new GroupResponseAdminDTO(group));
            }
        }

    }
}
