namespace Common.Interfaces.DB
{
    using Common.Enums.Contacts;
    using Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IContactConnector
    {
        Task Create(Contact contact);

        Task AddRange(List<Contact> contacts);
        Task<Contact> Read(Contact contact);
        Task<IEnumerable<Contact>> Read(List<Guid> Ids);
        Task<Contact> ReadByContactMeans(Contact contact);
        Task<Contact> ReadByContactPhone(Contact contact);
        IEnumerable<Contact> Read(User user, string key, int offset, int limit, bool popular, bool recent, out int totalCount, bool includeTabletMode);
        Task UpdateLastUsed(Contact contact);
        Task Update(Contact contact);
        
        Task Delete(Contact contact);
        Task DeleteRange(IEnumerable<Contact> contacts);
        Task<bool> IsActive(Contact contact);
        IEnumerable<Contact> ReadAllContactInGroup(Group group);
        Task<bool> ExistNotDeletedContactInGroup(Group group);
        Task DeleteNotDeletedContactAndContactsGroupInGroup(Group group);
        Task<bool> IsContactHaveDocumentToSign(Contact contact);
        Task Delete(Contact contact, Action<Contact> actionDelete);
        Task Delete(List<Contact> contacts, Action<List<Contact>> deleteContactSealsFromFS);
        Task<IEnumerable<Contact>> ReadDeleted();
        List<Contact> GetUnusedContacts(DateTime date);
        IEnumerable<Contact> ReadDeletedWithNoSignerConnected();

    }
}
