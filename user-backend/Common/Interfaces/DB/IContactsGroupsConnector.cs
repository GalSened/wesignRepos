using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.DB
{
    public interface IContactsGroupsConnector
    {
        Task Create(ContactsGroup contactsGroup);
        Task Delete(ContactsGroup contactsGroup);
        Task Delete(ContactGroupMember contactGroupMember);
        Task<ContactsGroup> Read(ContactsGroup contactsGroup);
        IEnumerable<ContactsGroup> Read(User user, string key, int offset, int limit, out int totalCount);
        Task Update(ContactsGroup contactsGroup);
    }
}