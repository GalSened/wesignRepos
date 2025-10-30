namespace BL.Handlers
{
    using Common.Enums.Contacts;
    using Common.Enums.Documents;
    using Common.Enums.Results;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Interfaces.Files;
    using Common.Models;
    using Common.Models.CSV_Mapper;
    using Common.Models.Documents;
    using Common.Models.Users;
    using Serilog;
    using Spire.Xls;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Group = Common.Models.Group;

    public class ContactsHandler : IContacts
    {
        private readonly IContactConnector _contactConnector;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IContactsGroupsConnector _contactsGroupsConnector;
        private readonly IProgramConnector _programConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IGroupConnector _groupConnector;
        private readonly ILogger _logger;
        private readonly IValidator _validator;
        private readonly IDataUriScheme _dataUriScheme;
        
        private readonly ICertificate _certificate;
        private readonly IUsers _users;
        public const char PHONE_SEPERATOR = '-';
        private readonly IDater _dater;
        private readonly IContactSignatures _contactSignatures;
        private readonly IFilesWrapper _filesWrapper;

        public ContactsHandler(IContactConnector contactConnector,IProgramConnector programConnector,
            ICompanyConnector companyConnector, IGroupConnector groupConnector,
            IDocumentCollectionConnector documentCollectionConnector,
            IContactsGroupsConnector contactsGroupsConnector, 
            ILogger logger, IValidator validator, ICertificate certificate, IUsers users
            , IDataUriScheme dataUriScheme,  IDater dater,
            IContactSignatures contactSignatures, IFilesWrapper filesWrapper)
        {
            _contactConnector = contactConnector;
            _documentCollectionConnector = documentCollectionConnector;
            _contactsGroupsConnector = contactsGroupsConnector;
            _programConnector = programConnector;
            _companyConnector = companyConnector;
            _groupConnector = groupConnector;
            _logger = logger;
            _validator = validator;
            _certificate = certificate;
            _users = users;
            _dataUriScheme = dataUriScheme;            
            _dater = dater;
            _contactSignatures = contactSignatures;
            _filesWrapper = filesWrapper;
        }

        public async Task Create(Contact contact)
        {
            (var user, var _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }

            if (_programConnector.IsFreeTrialUser(user))
            {

                var contactsList = _contactConnector.ReadAllContactInGroup(new Common.Models.Group()
                {

                    Id = user.GroupId
                });
                if (contactsList.Count() >= 10)
                {
                    throw new InvalidOperationException(ResultCode.ProgramUtilizationGetToMax.GetNumericString());
                }

            }
            if (!user.ProfileProgram.IsSmsProviderSupportGloballySend && !string.IsNullOrWhiteSpace(contact.PhoneExtension) && contact.PhoneExtension != "+972")
            {
                throw new InvalidOperationException(ResultCode.SmsProviderNotSupportSendingSmsGlobally.GetNumericString());
            }

            foreach (var seal in contact.Seals ?? Enumerable.Empty<Seal>())
            {
                seal.Base64Image = (await _validator.ValidateIsCleanFile(seal.Base64Image))?.CleanFile;
            }

            contact.UserId = user.Id;
            contact.GroupId = user.GroupId;
            contact.LastUsedTime = _dater.UtcNow();
            await _contactConnector.Create(contact);
            Group contactGroup = await _groupConnector.Read(new Group() { Id = contact.GroupId });
            var  contactCompanyConfiguration = (await _companyConnector.ReadConfiguration(new Company() { Id = contactGroup.CompanyId })) ??
                new Common.Models.Configurations.CompanyConfiguration() ;
            
            try
            {
                InsertContactSeals(contact);
                _certificate.Create(contact, contactCompanyConfiguration);
                _logger.Information("Successfully create contact [{ContactId} : {ContactName1}] by user [{UserId}: {ContactName}]", contact.Id, contact.Name, contact.UserId, contact.Name);
            }
            catch 
            {
                await _contactConnector.Delete(contact);
                throw ;
            }
        }

        public async Task UpdateSelfSignContactSignaturesImages(SignaturesImage signaturesImage)
        {

            (var user, var _) = await _users.GetUser();
            Contact contact = await ValidateContactForSaveSignature(signaturesImage.DocumentCollectionId, user);

            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidSignerId.GetNumericString());
            }
            await _contactSignatures.UpdateSignaturesImages(contact, new List<string> { signaturesImage.SignaturesImages });
          
        }

        public async Task<List<string>> GetSelfSignContactSavedSignatures(Guid docCollectionId)
        {

            (var user, var _) = await _users.GetUser();
            Common.Models.Contact contact = await ValidateContactForSaveSignature(docCollectionId, user);

           return _contactSignatures.GetContactSavedSignatures(contact);
            
        }

        private async Task<Contact> ValidateContactForSaveSignature(Guid docCollectionId, User user)
        {
            var documentCollection =await _documentCollectionConnector.Read(new DocumentCollection { Id = docCollectionId });
            if (documentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentId.GetNumericString());
            }
            if (documentCollection.DocumentStatus != DocumentStatus.Created && documentCollection.DocumentStatus != DocumentStatus.Sent && documentCollection.DocumentStatus != DocumentStatus.Viewed)
            {
                throw new InvalidOperationException(ResultCode.CannotCancelSignedDocument.GetNumericString());
            }
            if (documentCollection.Mode != DocumentMode.SelfSign)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentId.GetNumericString());

            }
            if (documentCollection.GroupId != user.GroupId)
            {
                throw new InvalidOperationException(ResultCode.ContactNotBelongToUserGroup.GetNumericString());
            }
            if (documentCollection.Signers.Count() > 1)
            {
                throw new InvalidOperationException(ResultCode.ContactNotBelongToUserGroup.GetNumericString());
            }
            var contact = documentCollection.Signers.FirstOrDefault().Contact;

            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidContactId.GetNumericString());
            }

            if (contact?.Email.ToLower() != user.Email.ToLower())
            {
                throw new InvalidOperationException(ResultCode.InvalidContactId.GetNumericString());
            }

            return contact;
        }

        public async Task DeleteBatch(RecordsBatch contactsBatch)
        {
            (var user, var _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);

            var contacts =await _contactConnector.Read(contactsBatch.Ids);
   
            Dictionary<Guid, string> contactInfosForLogs = new Dictionary<Guid, string>();
            foreach (var contact in contacts)
            {


                if (contact?.GroupId != user?.GroupId)
                {
                    throw new InvalidOperationException(ResultCode.ContactNotBelongToUserGroup.GetNumericString());
                }
               
                contactInfosForLogs.Add(contact.Id, contact.Name);

            }

            _logger.Information("user {UserId} : {UserEmail} deleted batch of contacts {Contacts}", user.Id, user.Email, contactInfosForLogs); 

            foreach (var contact in contacts)
            {
                await _contactConnector.Delete(contact);
            }
        }

        public async Task<(IEnumerable<Guid>, int)> CreateBulk(Contacts contactsXLSX)
        {
            int totalCount;

            (var user, var _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }

            if (_programConnector.IsFreeTrialUser(user))
            {
                throw new InvalidOperationException(ResultCode.OperationNotAllowByFreeTrialUser.GetNumericString());
            }

            List<Contact> contacts = new List<Contact>();
            List<Guid> contactsID = new List<Guid>();
            HashSet<string> uniqueContactIdentifiers = new HashSet<string>();
            List<Contact> DbContacts = _contactConnector.ReadAllContactInGroup(new Common.Models.Group() { Id = user.GroupId }).ToList();

            foreach (Contact contact in DbContacts)
            {
                if (!string.IsNullOrWhiteSpace(contact.Email))
                {
                    uniqueContactIdentifiers.Add(contact.Email);
                }
                if (!string.IsNullOrWhiteSpace(contact.Phone))
                {
                    uniqueContactIdentifiers.Add(contact.Phone);
                }
            }

            contactsXLSX.Base64 =(await _validator.ValidateIsCleanFile(contactsXLSX.Base64))?.CleanFile;
            byte[] data = _dataUriScheme.GetBytes(contactsXLSX.Base64);
            List<ContactMapper> contactsMapper = new List<ContactMapper>();
            using (Stream stream = new MemoryStream(data))
            {
                using (Workbook workbook = new Workbook())
                {
                    workbook.LoadFromStream(stream);
                    using (Worksheet worksheet = workbook.Worksheets[0])
                    {
                        var titles = worksheet.Rows[0];

                        if (titles.CellList[0].DisplayedText != "FullName" || titles.CellList[1].DisplayedText != "Email" || titles.CellList[2].DisplayedText != "PhoneNumber" || titles.CellList[3].DisplayedText != "SendingMethod")
                        {
                            throw new InvalidOperationException(ResultCode.InvalidBase64StringFormat.GetNumericString());
                        }
                        bool hasContactTag = titles.CellList.Count == 5 && titles.CellList[4].DisplayedText == "TagName" ? true : false;
                        if (worksheet.Rows.Length < 2)
                        {
                            throw new InvalidOperationException(ResultCode.InvalidBulkOfContacts.GetNumericString());
                        }

                        for (int i = 1; i < worksheet.Rows.Length; i++)
                        {
                            var contactDataFromExcel = worksheet.Rows[i];
                            if (contactDataFromExcel.CellList.Count > 0)
                            {
                                string contactName = contactDataFromExcel.CellList[0].DisplayedText;
                                string contactEmail = contactDataFromExcel.CellList[1].DisplayedText;
                                string contactPhone = contactDataFromExcel.CellList[2].DisplayedText;
                                Enum.TryParse(contactDataFromExcel.CellList[3].DisplayedText, out SendingMethod contactSendingMethod);
                                string contactTag = hasContactTag ? contactDataFromExcel.CellList[4].DisplayedText : String.Empty;

                                var contact = new Contact
                                {
                                    DefaultSendingMethod = contactSendingMethod,
                                    Email = contactEmail,
                                    Name = contactName,
                                    Phone = !string.IsNullOrEmpty(contactPhone) ? CreatePhoneNumber(contactPhone) : string.Empty,
                                    Status = ContactStatus.Activated,
                                    GroupId = user.GroupId,
                                    UserId = user.Id,
                                    SearchTag = contactTag,
                                    LastUsedTime = _dater.UtcNow()

                                };




                                if (!ValidateXLSXContact(contact, uniqueContactIdentifiers))
                                {
                                    continue;
                                }


                                if (!string.IsNullOrWhiteSpace(contact.Email))
                                {
                                    uniqueContactIdentifiers.Add(contact.Email.ToLower());
                                }
                                if (!string.IsNullOrEmpty(contact.Phone))
                                {
                                    uniqueContactIdentifiers.Add(contact.Phone);
                                }
                                contacts.Add(contact);

                            }
                        }

                        if (contacts.Count > 0)
                        {
                            await _contactConnector.AddRange(contacts);
                            contacts.ForEach(x => contactsID.Add(x.Id));
                        }
                        else
                        {
                            throw new InvalidOperationException(ResultCode.InvalidBulkOfContacts.GetNumericString());
                        }


                    }
                }
            }
            totalCount = contactsID.Count;
            return (contactsID, totalCount);



        }

        private bool ValidateXLSXContact(Contact contact, HashSet<string> uniqueContactIdentifiers)
        {
            if (string.IsNullOrEmpty(contact.Name) && string.IsNullOrEmpty(contact.Email) && string.IsNullOrEmpty(contact.Phone) && contact.DefaultSendingMethod == 0)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(contact.Name))
            {
                throw new InvalidOperationException(ResultCode.NameIsMissing.GetNumericString());
            }
            if (string.IsNullOrWhiteSpace(contact.Email) && string.IsNullOrWhiteSpace(contact.Phone))
            {
                throw new InvalidOperationException(ResultCode.InsufficientContactData.GetNumericString());
            }
            if (!string.IsNullOrWhiteSpace(contact.Email))
            {
                if (!ContactsExtenstions.IsValidEmail(contact.Email))
                {
                    //throw new InvalidOperationException(ResultCode.InvalidEmail.GetNumericString()); 
                    return false;
                }

                if (uniqueContactIdentifiers.Contains(contact.Email.ToLower()))
                {
                    //throw new InvalidOperationException(ResultCode.ContactAlreadyExists.GetNumericString());
                    return false;
                }
            }
            if (!string.IsNullOrWhiteSpace(contact.Phone))
            {
                if (!ContactsExtenstions.IsValidPhone(contact.Phone))
                {
                    return false;
                }
                if (uniqueContactIdentifiers.Contains(CreatePhoneNumber(contact.Phone)))
                {
                    return false;
                }
               
            }
            if (contact.DefaultSendingMethod != SendingMethod.SMS && contact.DefaultSendingMethod != SendingMethod.Email)
            {
                throw new InvalidOperationException(ResultCode.InvalidSendingMethod.GetNumericString());
            }

            if (!string.IsNullOrEmpty(contact.Phone) || !string.IsNullOrEmpty(contact.Email))
            {
                if (contact.DefaultSendingMethod == SendingMethod.SMS && string.IsNullOrWhiteSpace(contact.Phone)
                || contact.DefaultSendingMethod == SendingMethod.Email && string.IsNullOrWhiteSpace(contact.Email))
                {

                    if (contact.DefaultSendingMethod == SendingMethod.SMS)
                    {
                        contact.DefaultSendingMethod = SendingMethod.Email;
                    }
                    else
                    {
                        contact.DefaultSendingMethod = SendingMethod.SMS;
                    }
                }
            }
            return true;
        }

        public async Task<Contact> Read(Contact contact)
        {
            if (!await _contactConnector.IsActive(contact))
            {
                throw new InvalidOperationException(ResultCode.InvalidContactId.GetNumericString());
            }
            if (!await IsContactBelongToUserGroup(contact))
            {
                throw new InvalidOperationException(ResultCode.ContactNotBelongToUserGroup.GetNumericString());
            }

            contact = await _contactConnector.Read(contact);
            SetImagesContent(contact);

            return contact;
        }

        public async Task<(IEnumerable<Contact>, int)> Read(string key, int offset, int limit, bool popular, bool recent, bool includeTabletMode = true)
        {
           
            (var user, var _) = await _users.GetUser();
            var contacts = _contactConnector.Read(user, key, offset, limit, popular, recent, out int totalCount, includeTabletMode);
            contacts?.ToList().ForEach(e => SetImagesContent(e));

            return (contacts, totalCount);


        }

        public async Task Update(Contact contact)
        {
            (var user, var _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            if (!await _contactConnector.IsActive(contact))
            {
                throw new InvalidOperationException(ResultCode.InvalidContactId.GetNumericString());
            }
            if (!await IsContactBelongToUserGroup(contact, user))
            {
                throw new InvalidOperationException(ResultCode.ContactNotBelongToUserGroup.GetNumericString());
            }

            foreach (var seal in contact.Seals ?? Enumerable.Empty<Seal>())
            {
                seal.Base64Image = (await _validator.ValidateIsCleanFile(seal.Base64Image))?.CleanFile;
            }

            contact.GroupId = user.GroupId;
            contact.UserId = user.Id;
          
            var existContact =await _contactConnector.ReadByContactMeans(contact);
            if (existContact != null && contact.Id != Guid.Empty && existContact.Id == contact.Id)
            {
                existContact = await _contactConnector.ReadByContactPhone(contact);
            }
            if (existContact != null && contact.Id != Guid.Empty && existContact.Id != contact.Id)
            {
                throw new InvalidOperationException(ResultCode.ContactAlreadyExists.GetNumericString());
            }
            _certificate.Delete(contact);
            contact.LastUsedTime = _dater.UtcNow();
            await _contactConnector.Update(contact);
            try
            {
                DeleteContactSeals(contact);
                InsertContactSeals(contact);
            }
            catch
            {
                //TODO Roll back
                throw ;
            }
            Group contactGroup = await _groupConnector.Read(new Group() { Id = contact.GroupId });
            Company contactCompany = await _companyConnector.Read(new Company() { Id = contactGroup.CompanyId });
            _certificate.Create(contact, contactCompany.CompanyConfiguration);
        }
        
        public async Task Delete(Contact contact)
        {
            (var user, var _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);

            if (!await _contactConnector.IsActive(contact))
            {
                throw new InvalidOperationException(ResultCode.InvalidContactId.GetNumericString());
            }
            if (!await IsContactBelongToUserGroup(contact))
            {
                throw new InvalidOperationException(ResultCode.ContactNotBelongToUserGroup.GetNumericString());
            }

            
            _logger.Information("user {UserId}: {UserEmail} deleted contact {ContactId}: {ContactName}", user.Id, user.Email, contact.Id, contact.Name);

            await _contactConnector.Delete(contact);
        }

        public async Task<int> DeleteBulk(IEnumerable<Contact> contacts)
        {
            int successDeletedContactsCount = 0;
            foreach(var contact in contacts ?? Enumerable.Empty<Contact>())
            {
                try
                {
                    await Delete(contact);
                    successDeletedContactsCount++;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "DeleteBulk - failed to delete contact [{ContactId}] [{ContactName}]", contact.Id, contact.Name);
                }
            }
            return successDeletedContactsCount;
        }

        public async Task<Contact> GetContactForSimpleDocument(string signerMeans, string signerName, SendingMethod sendingMethod, string phoneExtension = "+972")
        {
            (var contacts, var _) =await Read(sendingMethod == SendingMethod.Tablet ? signerName : signerMeans, 0, -1, false, false, true);
            if (sendingMethod == SendingMethod.Tablet)
            {
                contacts = contacts.Where(x => x.DefaultSendingMethod == SendingMethod.Tablet);
            }

            Contact selectedContact = null;
            foreach (Contact contact in contacts)
            {
                if (sendingMethod == SendingMethod.Tablet && contact.Name == signerName)
                {
                    contact.DefaultSendingMethod = sendingMethod;
                    selectedContact = contact;
                    break;
                }
                if (signerMeans == contact.Phone || signerMeans?.ToLower() == contact.Email?.ToLower())
                {
                    selectedContact = contact;
                    selectedContact.Name = signerName;
                    selectedContact.PhoneExtension = phoneExtension;
            
                    await _contactConnector.Update(selectedContact);
                    break;
                }
            }
            if (selectedContact == null)
            {
                bool isPhone = ContactsExtenstions.IsValidPhone(signerMeans);
                selectedContact = new Contact()
                {
                    Email = isPhone ? string.Empty : signerMeans,
                    Phone = isPhone ? signerMeans : string.Empty,
                    PhoneExtension = phoneExtension,
                    DefaultSendingMethod = isPhone ? SendingMethod.SMS : sendingMethod,
                    Name = signerName
                };

                await Create(selectedContact);
            }

            return selectedContact;
        }

        public async Task<Contact> GetOrCreateContact(string signerName, string signerMeans)
        {
            (var contacts,var _) = await Read(signerMeans, 0, -1, false, false,  true);
            var contact = contacts.FirstOrDefault();
            if (contact == null)
            {
                bool isPhone = ContactsExtenstions.IsValidPhone(signerMeans);
                contact = new Contact()
                {
                    Email = isPhone ? string.Empty : signerMeans,
                    Phone = isPhone ? signerMeans : string.Empty,
                    DefaultSendingMethod = isPhone ? SendingMethod.SMS : SendingMethod.Email,
                    Name = signerName
                };

                await Create(contact);

            }

            return contact;
        }
        #region ContactsGroup


        public async Task<ContactsGroup> Read(ContactsGroup contactsGroup)
        {
            (var user, var _) = await _users.GetUser();
           
            ValidateContactsGroup(contactsGroup);
            var dbContactGroup = await _contactsGroupsConnector.Read(contactsGroup);
            if (dbContactGroup != null)
            {
                if (dbContactGroup.GroupId != user.GroupId)
                {
                    throw new InvalidOperationException(ResultCode.InvalidContactsSendingGroupId.GetNumericString());
                }
            }
            return dbContactGroup;
        }
        public async Task<(IEnumerable<ContactsGroup>,int)> ReadGroups(string key, int offset, int limit )
        {
         
            (var user, var _) = await _users.GetUser();           
            var contactsGroups = _contactsGroupsConnector.Read(user, key, offset, limit, out int totalCount);
            return (contactsGroups,totalCount);
        }


        public async Task CreateContactsGroup(ContactsGroup contactsGroup)
        {
            (var user, var _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            ValidateContactsGroup(contactsGroup);
            ValidateContactsGroupName(contactsGroup);
            ValidateContactsOrderInContactsGroup(contactsGroup.ContactGroupMembers);
            await ValidateContactsInContactsGroup(contactsGroup, user);

            var items = _contactsGroupsConnector.Read(user, "", 0, 31, out int totalCount);
            if (items.Count() > 30)
            {
                throw new InvalidOperationException(ResultCode.InvalidContactsSendingGroupMaxLimit.GetNumericString());
            }
            contactsGroup.CompanyId = user.CompanyId;
            contactsGroup.GroupId = user.GroupId;
            await _contactsGroupsConnector.Create(contactsGroup);

        }

        public async Task DeleteContactGroup(ContactsGroup contactsGroup)
        {
            (var user, var _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            ValidateContactsGroup(contactsGroup);
            var dbcontactsGroup =await _contactsGroupsConnector.Read(contactsGroup);
            if (dbcontactsGroup == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidContactsSendingGroupId.GetNumericString());
            }
            if (dbcontactsGroup.GroupId != user.GroupId)
            {
                throw new InvalidOperationException(ResultCode.InvalidContactsSendingGroupId.GetNumericString());
            }

           await _contactsGroupsConnector.Delete(dbcontactsGroup);

        }
        public async Task UpdateContactsGroup(ContactsGroup contactsGroup)
        {
            (var user, var _) = await _users.GetUser();

            await _validator.ValidateEditorUserPermissions(user);
            ValidateContactsGroup(contactsGroup);
            ValidateContactsGroupName(contactsGroup);
            ValidateContactsOrderInContactsGroup(contactsGroup.ContactGroupMembers);

            var dbContactsGroup =await _contactsGroupsConnector.Read(contactsGroup);            
            await ValidateContactGroupUpdateInput(contactsGroup, user, dbContactsGroup);            

            contactsGroup.GroupId = dbContactsGroup.GroupId;
            contactsGroup.CompanyId = dbContactsGroup.CompanyId;
            await _contactsGroupsConnector.Update(contactsGroup);

        }

        #endregion
        #region Helper Functions

        private void InsertContactSeals(Contact contact)
        {
            _filesWrapper.Contacts.SaveSeals(contact);
        }

       

        private void DeleteContactSeals(Contact contact)
        {
            _filesWrapper.Contacts.DeleteSeals(contact);            
        }

        private void SetImagesContent(Contact contact)
        {
            _filesWrapper.Contacts.SetSealsData(contact);
            
        }

        private async Task<bool> IsContactBelongToUserGroup(Contact contact, User user = null)
        {
            var connectedUser = user;
            if (user == null)
            {
              (connectedUser, var _) = await _users.GetUser();
            }
            
            contact = await _contactConnector.Read(contact);
            return contact?.GroupId == connectedUser.GroupId;
        }

        private string CreatePhoneNumber(string phoneNumber)
        {
            // Allow not only ILS phone
            //if (phoneNumber.Length == 10)
            //{
            //    return phoneNumber;
            //}
            if (phoneNumber.Length == 9 && phoneNumber[0] != 0)
            {
                return phoneNumber.Insert(0, "0");
            }

            if (phoneNumber[1] == PHONE_SEPERATOR)
            {
                return phoneNumber.Replace("-", string.Empty);
            }
            return phoneNumber;
        }

        private async Task ValidateContactGroupUpdateInput(ContactsGroup contactsGroup, User user, ContactsGroup dbContactsGroup)
        {
            if (dbContactsGroup == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidContactsSendingGroupId.GetNumericString());

            }
            if (dbContactsGroup.GroupId != user.GroupId)
            {
                throw new InvalidOperationException(ResultCode.InvalidContactsSendingGroupId.GetNumericString());
            }
            if (contactsGroup.ContactGroupMembers != null && contactsGroup.ContactGroupMembers.Count > 0)
            {
                ValidateContactsOrderInContactsGroup(contactsGroup.ContactGroupMembers);
                await ValidateContactsInContactsGroup(contactsGroup, user);
                
            }
        }

        private void ValidateContactsOrderInContactsGroup(List<ContactGroupMember> contactsGroup)
        {
            if (contactsGroup == null || contactsGroup.Count == 0)
            {
                return;
            }

            if ( contactsGroup.Count > 25)
            {
                throw new InvalidOperationException(ResultCode.InvalidContactsSendingGroupMembersMaxLimit.GetNumericString());
            }
            var orderGroup = contactsGroup.OrderBy(x => x.Order).ToList();

            for (int i = 0; orderGroup.Count > i; ++i)
            {
                if(orderGroup[i].Order != i+1)
                {
                    throw new InvalidOperationException(ResultCode.InvalidContactsSendingGroupOrderOfTheMembers.GetNumericString());
                }

            }

           
        }

        private async Task ValidateContactsInContactsGroup(ContactsGroup contactsGroup, User user)
        {
            if (contactsGroup.ContactGroupMembers != null && contactsGroup.ContactGroupMembers.Count > 0)
            {
                var contacts =await _contactConnector.Read(contactsGroup.ContactGroupMembers.Select(x => x.ContactId).Distinct().ToList());
                foreach (var members in contactsGroup.ContactGroupMembers ?? Enumerable.Empty<ContactGroupMember>())
                {
                    var selectedContact = contacts.FirstOrDefault(x => x.Id == members.ContactId);
                    if (selectedContact == null || selectedContact.GroupId != user.GroupId)
                    {
                        throw new InvalidOperationException(ResultCode.InvalidContactId.GetNumericString());
                    }
                    if (selectedContact.Status == ContactStatus.Deleted)
                    {
                        throw new InvalidOperationException(ResultCode.InvalidContactInStatusDeleted.GetNumericString());
                    }

                }
            }
        }
        private void ValidateContactsGroup(ContactsGroup contactsGroup)
        {
            if(contactsGroup == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }
          
        }
        private void ValidateContactsGroupName(ContactsGroup contactsGroup)
        {
            if (string.IsNullOrWhiteSpace(contactsGroup.Name))
            {
                throw new InvalidOperationException(ResultCode.InvalidContactsGroupName.GetNumericString());

            }
        }
        #endregion
    }
}