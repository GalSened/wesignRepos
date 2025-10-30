namespace Common.Interfaces.DB
{
    using Common.Enums.Groups;
    using Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGroupConnector
    {
        Task Create(Group group);
        IEnumerable<Group> Read(Company company);
        IEnumerable<Group> Read(GroupStatus status, bool incluseUsers);      
        Task<Group> Read(User user);
        Task<Group> Read(Group group);
        IEnumerable<Group> ReadMany(IEnumerable<Group> groups);
        IEnumerable<Group> ReadManyByGroupName(string groupName); 
        Task<bool> IsGroupsExistInCompany(Company company);
     
        /// <summary>
        /// Delete by changing status
        /// </summary>
        /// <param name="group"></param>
        Task Delete(Group group);
       
        Task Delete(List<Group> groups);
        Task DeletePermanently(Group group);
        Task Update(Group group);
        Task UpdateAdditionalGroups(User dbUser, List<AdditionalGroupMapper> additionalGroupsMapper);
        Task<List<Group>> ReadAllUserGroups(User user);
        Task DeleteAllAdditionalGroupsGroupConnection(Group group);
        bool ExistRecordsInGroup(Group group);
        Task<Company> ReadCompany(Group group);
        Dictionary<Guid, string> GetGroupIdNameDictionary(List<Guid> groupIds);
    }
}
