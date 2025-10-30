namespace Common.Interfaces
{
    using Common.Enums.Documents;
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Users;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IContacts
    {
        Task Create(Contact contact);
        Task<(IEnumerable<Guid>,int)> CreateBulk(Contacts contactsXLSX);
        Task<Contact> Read(Contact contact );
        Task<ContactsGroup> Read(ContactsGroup contactsGroup);
        Task<(IEnumerable<Contact>, int)> Read(string key, int offset, int limit, bool popular, bool recent , bool includeTabletMode);
        Task Update(Contact contact);
        Task Delete(Contact contact);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contacts"></param>
        /// <returns>Success deleted contacts count</returns>
        Task<int> DeleteBulk(IEnumerable<Contact> contacts);
        Task<Contact> GetContactForSimpleDocument(string signerMeans, string signerName, SendingMethod sendingMethod, string phoneExtension = "+972");
        Task<Contact> GetOrCreateContact(string signerName, string signerMeans );
        Task DeleteBatch(RecordsBatch contactsBatch);
        Task UpdateSelfSignContactSignaturesImages(SignaturesImage signaturesImage);
       Task<List<string>> GetSelfSignContactSavedSignatures(Guid docCollectionId);
        Task<(IEnumerable<ContactsGroup>,int)> ReadGroups(string key, int offset, int limit);
        Task DeleteContactGroup(ContactsGroup contactsGroup);
        Task UpdateContactsGroup(ContactsGroup contactsGroup);
        Task CreateContactsGroup(ContactsGroup contactsGroup);
    }
}
