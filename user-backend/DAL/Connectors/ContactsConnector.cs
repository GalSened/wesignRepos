using Common.Enums.Contacts;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models;
using DAL.DAOs.Contacts;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class ContactsConnector : IContactConnector
    {
        private const int UNLIMITED = -1;
        private readonly IWeSignEntities _dbContext;
        private readonly IDater _dater;
        private readonly ILogger _logger;

        public ContactsConnector(IWeSignEntities dbContext, IDater dater, ILogger logger)
        {
            _dbContext = dbContext;
            _dater = dater;
            _logger = logger;
        }

        public async Task Create(Contact contact)
        {
            try
            {
                var contactDAO = new ContactDAO(contact);
                if (await IsContactExist(contactDAO))
                {
                    throw new InvalidOperationException(ResultCode.ContactAlreadyExists.GetNumericString());
                }
                await _dbContext.Contacts.AddAsync(contactDAO);

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    UpdateContactInfo(contact, contactDAO);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_Create = ");
                throw;
            }
        }

        public async Task AddRange(List<Contact> contacts)
        {
            try
            {
                List<ContactDAO> contactsDAO = new List<ContactDAO>();

                foreach (Contact contact in contacts)
                {
                    contactsDAO.Add(new ContactDAO(contact));
                }

                _dbContext.Contacts.AddRange(contactsDAO);

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    foreach (Contact contact in contacts)
                    {
                        UpdateContactInfo(contact, contactsDAO[contacts.IndexOf(contact)]);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_AddRange = ");
                throw;
            }
        }


        public async Task DeleteNotDeletedContactAndContactsGroupInGroup(Group group)
        {
            try
            {
                await _dbContext.Contacts.Where(x => x.GroupId == group.Id && x.Status == ContactStatus.Activated).ExecuteUpdateAsync(
                 setters => setters.SetProperty(x => x.Status, ContactStatus.Deleted));

                await _dbContext.ContactGroupMembers.Include(x => x.ContactsGroup).Where(x => x.ContactsGroup.GroupId == group.Id).ExecuteDeleteAsync();

                await _dbContext.ContactsGroup.Where(x => x.GroupId == group.Id).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_DeleteNotDeletedContactAndContactsGroupInGroup = ");
                throw;
            }
        }

        public async Task Delete(Contact contact)
        {
            try
            {
                var dbContact = await Read(contact);
                dbContact.Status = ContactStatus.Deleted;
                var contactDAO = new ContactDAO(dbContact);
                contact.UserId = contactDAO.UserId;

                var localDbMemoryContact = _dbContext.Contacts.Local.FirstOrDefault(x => x.Id == contact.Id &&
                                                                            x.Status == ContactStatus.Activated);
                if (localDbMemoryContact != null)
                {
                    _dbContext.Contacts.Local.Remove(localDbMemoryContact);
                }

                _dbContext.Contacts.Update(contactDAO);
                var contactGroupMembers = _dbContext.ContactGroupMembers.Where(x => x.ContactId == contact.Id &&
                contact.GroupId == x.ContactsGroupId);
                if (await contactGroupMembers.AnyAsync())
                {
                    _dbContext.ContactGroupMembers.RemoveRange(contactGroupMembers);
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_Delete = ");
                throw;
            }
        }

        public async Task DeleteRange(IEnumerable<Contact> contacts)
        {
            try
            {
                await _dbContext.ContactGroupMembers.Where(x => contacts.Select(x => x.Id).Contains(x.ContactId)).ExecuteDeleteAsync();
                await _dbContext.Contacts.Where(x => contacts.Select(x => x.Id).Contains(x.Id)).ExecuteUpdateAsync(
                    setter => setter.SetProperty(x => x.Status, ContactStatus.Deleted));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_DeleteRange = ");
                throw;
            }
        }


        public async Task<IEnumerable<Contact>> Read(List<Guid> Ids)
        {
            try
            {
                return await _dbContext.Contacts.Where(x => Ids.Contains(x.Id)).Select(x => x.ToContact()).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_ReadByIds = ");
                throw;
            }
        }

        public async Task<Contact> Read(Contact contact)
        {
            try
            {
                var t = (await _dbContext.Contacts.Include(c => c.Seals).FirstOrDefaultAsync(c => c.Id == contact.Id && c.Status == ContactStatus.Activated)).ToContact();
                if (t == null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    t = (await _dbContext.Contacts.Include(c => c.Seals).FirstOrDefaultAsync(c => c.Email == contact.Email && c.Status == ContactStatus.Activated &&
                    (c.GroupId == contact.GroupId ||
                    c.UserId == contact.UserId))).ToContact();
                }
                if (t == null && !string.IsNullOrWhiteSpace(contact.Phone))
                {
                    t = (await _dbContext.Contacts.Include(c => c.Seals).FirstOrDefaultAsync
                        (c => c.Phone == contact.Phone && c.Status == ContactStatus.Activated && (c.GroupId == contact.GroupId || c.UserId == contact.UserId))).ToContact();
                }
                if (t != null)
                {
                    contact.GroupId = t.GroupId;
                    contact.UserId = t.UserId;
                }
                return t;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_ReadByContact = ");
                throw;
            }
        }


        public async Task<Contact> ReadByContactMeans(Contact contact)
        {
            try
            {
                Contact t = null;
                if (!string.IsNullOrWhiteSpace(contact.Email) && contact.GroupId != Guid.Empty && contact.GroupId != Guid.Empty)
                {
                    t = (await _dbContext.Contacts.Include(c => c.Seals)
                        .FirstOrDefaultAsync(c => c.Email == contact.Email && (c.GroupId == contact.GroupId) && c.Status != ContactStatus.Deleted)).ToContact();

                }
                if (t == null && !string.IsNullOrWhiteSpace(contact.Phone) && contact.GroupId != Guid.Empty && contact.GroupId != Guid.Empty)
                {
                    t = (await _dbContext.Contacts.Include(c => c.Seals).
                        FirstOrDefaultAsync(c => c.Phone == contact.Phone && (c.GroupId == contact.GroupId) && c.Status != ContactStatus.Deleted)).ToContact();
                }
                if (t == null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    t = (await _dbContext.Contacts.Include(c => c.Seals)
                    .FirstOrDefaultAsync(c => c.Email == contact.Email && (c.UserId == contact.UserId) && c.Status != ContactStatus.Deleted)).ToContact();
                }
                if (t == null && !string.IsNullOrWhiteSpace(contact.Phone))
                {

                    t = (await _dbContext.Contacts.Include(c => c.Seals).FirstOrDefaultAsync(c => c.Phone == contact.Phone && (c.UserId == contact.UserId)
                    && c.Status != ContactStatus.Deleted)).ToContact();
                }
                if (t != null)
                {
                    contact.GroupId = t.GroupId;
                    contact.UserId = t.UserId;
                }
                return t;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_ReadByContactMeans = ");
                throw;
            }
        }

        public async Task<Contact> ReadByContactPhone(Contact contact)
        {
            try
            {
                Contact t = null;
                if (!string.IsNullOrWhiteSpace(contact.Phone))
                {
                    t = (await _dbContext.Contacts.Include(c => c.Seals).FirstOrDefaultAsync(c => c.Phone == contact.Phone &&
                    (c.GroupId == contact.GroupId) && c.Status != ContactStatus.Deleted)).ToContact();
                }
                if (t != null)
                {
                    contact.GroupId = t.GroupId;
                    contact.UserId = t.UserId;
                }
                return t;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_ReadByContactPhone = ");
                throw;
            }
        }



        public IEnumerable<Contact> ReadDeletedWithNoSignerConnected()
        {
            try
            {
                var query = _dbContext.Contacts.Include(c => c.Seals)
                                                          .Where(c => c.Status == ContactStatus.Deleted
                                                          && !_dbContext.Signers.Include(x => x.Contact).Select(x => x.Contact.Id).Contains(c.Id));


                return query.Select(x => x.ToContact());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_ReadDeletedWithNoSignerConnected = ");
                throw;
            }
        }
        public async Task<IEnumerable<Contact>> ReadDeleted()
        {
            try
            {
                var query = _dbContext.Contacts.Include(c => c.Seals)
                                                          .Where(c => c.Status == ContactStatus.Deleted);
                return await query.Select(x => x.ToContact()).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_ReadDeleted = ");
                throw;
            }
        }


        public IEnumerable<Contact> Read(User user, string key, int offset, int limit, bool popular, bool recent, out int totalCount, bool includeTabletMode)
        {
            try
            {
                var result = new List<Contact>();
                var query = string.IsNullOrWhiteSpace(key) ?
                                                    _dbContext.Contacts.Include(c => c.Seals)
                                                              .Where(c => c.GroupId == user.GroupId && c.Status == ContactStatus.Activated) :
                                                    _dbContext.Contacts.Include(c => c.Seals)
                                                              .Where(c => c.GroupId == user.GroupId && c.Status == ContactStatus.Activated &&
                                                              (c.Name.Contains(key) || c.Phone.Contains(key) || c.Email.Contains(key) || c.SearchTag.Contains(key)));

                if (!includeTabletMode)
                {
                    query = query.Where(x => x.DefaultSendingMethod != Common.Enums.Documents.SendingMethod.Tablet);
                }
                query = query.OrderBy(x => x.Name);
                //TODO handle recent and popular while documentTocontacts
                totalCount = query.Count();
                query = limit != UNLIMITED ? query.Skip(offset).Take(limit) : query.Skip(offset);
                foreach (var row in query)
                {
                    result.Add(row.ToContact());
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_ReadByUser&Key&Offset&Limit = ");
                throw;
            }
        }

        public async Task UpdateLastUsed(Contact contact)
        {
            try
            {
                await _dbContext.Contacts.Where(x => x.Id == contact.Id).ExecuteUpdateAsync(
               setters => setters.SetProperty(x => x.LastUsedTime, _dater.UtcNow()));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_UpdateLastUsed = ");
                throw;
            }
        }

        public async Task Update(Contact contact)
        {
            try
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async

                    () =>
                {
                    using var transaction = await _dbContext.Database.BeginTransactionAsync();
                    try
                    {
                        var contactDAO = _dbContext.Contacts.Local
                                               .FirstOrDefault(c => c.Id == contact.Id) ?? await _dbContext.Contacts
                                               .Include(c => c.Seals)
                                               .FirstOrDefaultAsync(c => c.Id == contact.Id);
                        contactDAO.GroupId = contact.GroupId;
                        contactDAO.Name = contact.Name;
                        contactDAO.Phone = contact.Phone;
                        contactDAO.PhoneExtension = contact.PhoneExtension;
                        contactDAO.Email = contact.Email;
                        contactDAO.DefaultSendingMethod = contact.DefaultSendingMethod;
                        contactDAO.SearchTag = contact.SearchTag;
                        contactDAO.LastUsedTime = DateTime.UtcNow;

                        _dbContext.ContactSeals.RemoveRange(contactDAO.Seals);
                        contactDAO.Seals = contact.Seals.Select(s => new ContactSealsDAO(s)).ToList();

                        _dbContext.Contacts.Update(contactDAO);
                        await _dbContext.SaveChangesAsync();

                        await transaction.CommitAsync();
                        contact.UserId = contactDAO.UserId;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_Update = ");
                throw;
            }
        }

        public async Task<bool> IsActive(Contact contact)
        {
            try
            {
                return await _dbContext.Contacts.AnyAsync(c => c.Id == contact.Id && c.Status == ContactStatus.Activated);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_IsActive = ");
                throw;
            }
        }

        public async Task<bool> ExistNotDeletedContactInGroup(Group group)
        {
            try
            {
                return await _dbContext.Contacts.AnyAsync(x => x.GroupId == group.Id && x.Status != ContactStatus.Deleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_ExistNotDeletedContactInGroup = ");
                throw;
            }
        }

        public IEnumerable<Contact> ReadAllContactInGroup(Group group)
        {
            try
            {
                var result = new List<Contact>();
                var query = _dbContext.Contacts.Where(x => x.GroupId == group.Id);
                foreach (var row in query)
                {
                    result.Add(row.ToContact());
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_ReadAllContactInGroup = ");
                throw;
            }
        }

        public async Task<bool> IsContactHaveDocumentToSign(Contact contact)
        {
            try
            {
                return await _dbContext.Signers.AnyAsync(x => x.ContactId == contact.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_IsContactHaveDocumentToSign = ");
                throw;
            }
        }

        public async Task Delete(List<Contact> contacts, Action<List<Contact>> deleteContactSealsFromFS)
        {
            try
            {
                contacts = contacts.Where(x => x.Status == ContactStatus.Deleted).ToList();
                if (contacts.Count > 0)
                {
                    await _dbContext.Contacts.Where(x => contacts.Select(x => x.Id).Contains(x.Id)).ExecuteDeleteAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_DeleteByContacts&DeleteContactSealsFromFS = ");
                throw;
            }
        }

        public async Task Delete(Contact contact, Action<Contact> actionDelete)
        {
            try
            {
                if (contact.Status == ContactStatus.Deleted)
                {
                    var strategy = _dbContext.Database.CreateExecutionStrategy();

                    await strategy.ExecuteAsync(async
                        () =>
                    {

                        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                var contactDAO = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == contact.Id);
                                _dbContext.Contacts.Remove(contactDAO);
                                await _dbContext.SaveChangesAsync();
                                actionDelete(contact);
                                await transaction.CommitAsync();

                            }
                            catch
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_DeleteByContacts&ActionDelete = ");
                throw;
            }
        }

        public List<Contact> GetUnusedContacts(DateTime date)
        {
            try
            {
                var contacts = _dbContext.Contacts.Include(x => x.Signers).
                Where(x => x.LastUsedTime <= date && x.Status == ContactStatus.Activated &&
                x.CreationSource == Common.Enums.CreationSource.Application
               && !_dbContext.Signers.Select(x => x.ContactId).Contains(x.Id));
                return contacts.Select(x => x.ToContact()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ContactsConnector_GetUnusedContacts = ");
                throw;
            }
        }

        #region Private Functions

        /// <summary>
        /// Check if there is already exists contact in his group with same email/phone in system
        /// </summary>
        /// <param name="contactDAO"></param>
        /// <returns></returns>
        private async Task<bool> IsContactExist(ContactDAO contactDAO)
        {
            bool emailExist = !string.IsNullOrWhiteSpace(contactDAO.Email);
            bool phoneExist = !string.IsNullOrWhiteSpace(contactDAO.Phone);
            return await _dbContext.Contacts.AnyAsync(
                                            c => c.GroupId == contactDAO.GroupId && c.Status == ContactStatus.Activated &&
                                           ((emailExist && c.Email == contactDAO.Email) ||
                                           (phoneExist && c.Phone == contactDAO.Phone)));


        }



        private void UpdateContactInfo(Contact contact, ContactDAO contactDAO)
        {
            contact.Id = contactDAO.Id;
            for (int i = 0; i < contactDAO.Seals.Count; i++)
            {
                contact.Seals.ElementAt(i).Id = contactDAO.Seals.ElementAt(i).Id;
            }
        }







        #endregion
    }
}
