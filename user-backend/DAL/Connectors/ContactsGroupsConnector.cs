using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Enums.Contacts;
using Common.Interfaces.DB;
using Common.Models;
using DAL.DAOs.Contacts;
using DAL.Extensions;
using IO.ClickSend.ClickSend.Model;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DAL.Connectors
{
    public class ContactsGroupsConnector : IContactsGroupsConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ILogger _logger;
        private const int UNLIMITED = -1;
        public ContactsGroupsConnector(IWeSignEntities dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Create(ContactsGroup contactsGroup)
        {
            try
            {
                var dbContactGroup = new ContactsGroupDAO(contactsGroup);
                await _dbContext.ContactsGroup.AddAsync(dbContactGroup);

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    UpdateInfo(contactsGroup, dbContactGroup);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsGroupsConnector_Create = ");
                throw;
            }
        }

        public async Task Delete(ContactsGroup contactsGroup)
        {
            try
            {
                var dbContactsGroup = await _dbContext.ContactsGroup.Include(x => x.ContactGroupMembers).
                FirstOrDefaultAsync(x => x.Id == contactsGroup.Id);
                if (dbContactsGroup != null)
                {
                    if (dbContactsGroup.ContactGroupMembers != null && dbContactsGroup.ContactGroupMembers.Count > 0)
                    {
                        _dbContext.ContactGroupMembers.RemoveRange(dbContactsGroup.ContactGroupMembers);
                    }
                    _dbContext.ContactsGroup.Remove(dbContactsGroup);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsGroupsConnector_DeleteByContactsGroup = ");
                throw;
            }
        }

        public Task Delete(ContactGroupMember contactGroupMember)
        {
            try
            {
                return _dbContext.ContactGroupMembers.Where(x => x.Id == contactGroupMember.Id).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsGroupsConnector_DeleteByContactGroupMember = ");
                throw;
            }
        }

        public async Task<ContactsGroup> Read(ContactsGroup contactsGroup)
        {
            try
            {
                return (await _dbContext.ContactsGroup.Include(x => x.ContactGroupMembers).ThenInclude(x => x.Contact).
                                        FirstOrDefaultAsync(x => x.Id == contactsGroup.Id)).ToContactsGroup();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsGroupsConnector_ReadByContactsGroup = ");
                throw;
            }
        }

        public async Task Update(ContactsGroup contactsGroup)
        {
            try
            {
                var dbContactsGroup = await _dbContext.ContactsGroup.Include(x => x.ContactGroupMembers)
                .FirstOrDefaultAsync(x => x.Id == contactsGroup.Id);
                if (dbContactsGroup != null)
                {
                    dbContactsGroup.Name = contactsGroup.Name;
                    if (dbContactsGroup.ContactGroupMembers != null && dbContactsGroup.ContactGroupMembers.Count > 0)
                    {
                        // need to remove ..  


                        _dbContext.ContactGroupMembers.RemoveRange(dbContactsGroup.ContactGroupMembers);
                    }
                    _dbContext.ContactsGroup.Update(dbContactsGroup);
                    // update Name Only
                    await _dbContext.SaveChangesAsync();
                    if (contactsGroup.ContactGroupMembers != null && contactsGroup.ContactGroupMembers.Any())
                    {
                        List<ContactGroupMemberDAO> contactsMembers = contactsGroup.ContactGroupMembers.Select(x =>
                        {
                            x.ContactsGroupId = contactsGroup.Id;
                            return new ContactGroupMemberDAO(x);
                        }
                        ).ToList();
                        await _dbContext.ContactGroupMembers.AddRangeAsync(contactsMembers);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsGroupsConnector_Update = ");
                throw;
            }
        }

        public IEnumerable<ContactsGroup> Read(User user, string key, int offset, int limit, out int totalCount)
        {
            try
            {
                var result = new List<ContactsGroup>();
                var query = string.IsNullOrWhiteSpace(key) ?
                                                    _dbContext.ContactsGroup.Include(c => c.ContactGroupMembers).
                                                    ThenInclude(x => x.Contact)
                                                              .Where(c => c.GroupId == user.GroupId) :
                                                    _dbContext.ContactsGroup.Include(c => c.ContactGroupMembers).
                                                        ThenInclude(x => x.Contact)
                                                              .Where(c => c.GroupId == user.GroupId && c.Name.Contains(key))
                                                              .Union(
                                                        _dbContext.ContactsGroup.Include(c => c.ContactGroupMembers).
                                                        ThenInclude(x => x.Contact)
                                                              .Where(c => c.GroupId == user.GroupId && c.ContactGroupMembers.Any(x => x.Contact.Name.Contains(key)))).Distinct();
                totalCount = query.Count();
                query = limit != UNLIMITED ? query.Skip(offset).Take(limit) : query.Skip(offset);
                foreach (var row in query)
                {
                    result.Add(row.ToContactsGroup());
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsGroupsConnector_ReadByUser&Key&Offset&Limit = ");
                throw;
            }
        }

        #region Private Functions
        private void UpdateInfo(ContactsGroup contactsGroup, ContactsGroupDAO dbContactGroup)
        {
            contactsGroup.Id = dbContactGroup.Id;
            for (int i = 0; i < dbContactGroup.ContactGroupMembers.Count; i++)
            {
                contactsGroup.ContactGroupMembers.ElementAt(i).Id = dbContactGroup.ContactGroupMembers.ElementAt(i).Id;
            }
        }
        #endregion
    }
}
