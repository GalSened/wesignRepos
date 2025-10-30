namespace Common.Interfaces
{
    using Common.Enums.Users;
    using Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAdmins
    {
        Task Create(Group group);
        Task<IEnumerable<Group>> ReadGroups();
        Task Update(Group group);
        Task Delete(Group group);

        Task Create(User user);
        Task<(IEnumerable<User>,int)> ReadUsers(string key, int offset, int limit, UserStatus? status);
        Task Update(User user);
        Task Delete(User user);
    }
}
