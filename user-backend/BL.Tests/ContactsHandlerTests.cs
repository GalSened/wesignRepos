using BL.Handlers;

using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.FileGateScanner;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class ContactsHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";
        private const string GUID_1 = "8F9D5D30-1AAB-4EC6-840E-C3F6ADF10BCF";
        private const string GUID_TO_DELETE = "E8D2755A-6E68-4542-A443-B6520371D84E";


        private CompanySigner1Details _companySigner1Details;
        
        
        private readonly IContacts _contactHandler;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IValidator> _validator;
        private readonly Mock<IDataUriScheme> _dataUriScheme;
        private readonly Mock<IDater> _dater;
        
        private readonly Mock<ICertificate> _certificate;
        private readonly Mock<IUsers> _users;
        private readonly Mock<IContactSignatures> _contactSignatures;
        private readonly IFilesWrapper _filesWrapper;



        private readonly Mock<IDocumentFileWrapper> _documentFileWrapper;
        private readonly Mock<IContactFileWrapper> _contactFileWrapper;
        private readonly Mock<IUserFileWrapper> _userFileWrapper;
        private readonly Mock<ISignerFileWrapper> _signerFileWrapper;
        private readonly Mock<IConfigurationFileWrapper> _configurationFileWrapper;


        private readonly Mock<IContactConnector> _contactConnector;
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnector;
        private readonly Mock<IContactsGroupsConnector> _contactsGroupsConnector;
        private readonly Mock<IProgramConnector> _programConnector;
        private readonly Mock<ICompanyConnector> _companyConnector;
        private readonly Mock<IGroupConnector> _groupConnector;


        public void Dispose()
        {
            
            _loggerMock.Invocations.Clear();
        }

        public ContactsHandlerTests()
        {
            _contactConnector = new Mock<IContactConnector>();
            _documentCollectionConnector = new Mock<IDocumentCollectionConnector>();
            _contactsGroupsConnector = new Mock<IContactsGroupsConnector>();
            _programConnector = new Mock<IProgramConnector>();
            _companyConnector = new Mock<ICompanyConnector> ();
            _groupConnector = new Mock<IGroupConnector>();

            //_dbConnectorMock.Setup(x => x.Users.Read(It.IsAny<User>())).ReturnsAsync(new User() { Id = new Guid(GUID), GroupId = new Guid(GUID) });
           
            _loggerMock = new Mock<ILogger>();
            _validator = new Mock<IValidator>();
            _certificate = new Mock<ICertificate>();
            _dataUriScheme = new Mock<IDataUriScheme>();
            _dater = new Mock<IDater>();
        
            _users = new Mock<IUsers>();
            _contactSignatures = new Mock<IContactSignatures>();

            _documentFileWrapper = new Mock<IDocumentFileWrapper>();            
            _contactFileWrapper = new Mock<IContactFileWrapper>();
            _userFileWrapper = new Mock<IUserFileWrapper>();
            _signerFileWrapper = new Mock<ISignerFileWrapper>();
            _configurationFileWrapper = new Mock<IConfigurationFileWrapper>();

            _filesWrapper = new FileWrapperStub(_documentFileWrapper.Object, _contactFileWrapper.Object,_userFileWrapper.Object,_signerFileWrapper.Object,_configurationFileWrapper.Object );
          


        _users.Setup(x => x.GetUser()).ReturnsAsync((new User() { Id = new Guid(GUID), GroupId = new Guid(GUID) }, _companySigner1Details));

            _contactHandler = new ContactsHandler(_contactConnector.Object,_programConnector.Object, _companyConnector.Object,_groupConnector.Object
                ,_documentCollectionConnector.Object, _contactsGroupsConnector.Object, _loggerMock.Object,
               _validator.Object, _certificate.Object, _users.Object, _dataUriScheme.Object,
               _dater.Object, _contactSignatures.Object, _filesWrapper);
        }


            

        #region Create

        [Fact]
        public async Task Create_UserProgramExpired_ReturnException()
        {
            var contact = new Contact()
            {
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il"
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Create(contact));

            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_ContactAlreadyExists_ReturnDbException()
        {
            var contact = new Contact()
            {
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il"
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _contactConnector.Setup(x => x.Create(It.IsAny<Contact>())).Throws(new InvalidOperationException(ResultCode.ContactAlreadyExists.GetNumericString()));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Create(contact));

            Assert.Equal(ResultCode.ContactAlreadyExists.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_ValidContact_ReturnDbException()
        {
            var contact = new Contact()
            {
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il"
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _contactConnector.Setup(x => x.Create(It.IsAny<Contact>())).Throws(new Exception("DB Exception"));

            var actual = await Assert.ThrowsAsync<Exception>(() => _contactHandler.Create(contact));

            Assert.Equal("DB Exception", actual.Message);
        }

        [Fact]
        public async Task Create_ValidContactWithSeal_Success()
        {
            string smallImage = "R0lGODlhAQABAIAAAP///////yH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
            bool saved = false;
            var contact = new Contact()
            {
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il",
                Seals = new List<Seal>() { new Seal() { Name = "image", Base64Image = $"data:image/png;base64,{smallImage}" } }
            };
            var guid = Guid.NewGuid();
            var group = new Group()
            {
                CompanyId = guid
            };
            var company = new Company()
            {
                Id = guid,
            };
            
            
            _contactFileWrapper.Setup(x => x.SaveSeals(It.IsAny<Contact>())).Callback(() => { saved = true; });
            
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _contactConnector.Setup(x => x.Create(It.IsAny<Contact>()));
            _groupConnector.Setup(x => x.Read(It.IsAny<Group>())).ReturnsAsync(group);
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);

            _validator.Setup(x => x.ValidateIsCleanFile(It.IsAny<string>())).ReturnsAsync(new FileGateScanResult { CleanFile = $"data:image/png;base64,{smallImage}" });

           await  _contactHandler.Create(contact);
            Assert.True(saved);
        }

        #endregion

        #region Read

        [Fact]
        public async Task Read_NotActiveContact_ReturnException()
        {
            var contact = new Contact();

            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(false);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Read(contact));

            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Read_ContactNotExist_ReturnException()
        {
            var contact = new Contact()
            {
                Id = new Guid(GUID)
            };
            Contact nullContact = null;
            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(true);
            _contactConnector.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(nullContact);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Read(contact));

            Assert.Equal(ResultCode.ContactNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Read_ValidContact_ReturnException()
        {
            var contact = new Contact()
            {
                Id = new Guid(GUID)
            };
            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).Throws(new InvalidOperationException("Failed to read contact from DB"));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Read(contact));

            Assert.Equal("Failed to read contact from DB", actual.Message);
        }

        [Fact]
        public async Task Read_ContactBelongToAnotherGroup_Failed()
        {
            var contact = new Contact()
            {
                Id = new Guid(GUID)
            };
            var anotherContact = new Contact()
            {
                GroupId = new Guid("6D17F027-04B9-434B-B41E-E3687CFFAFD2")
            };

            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(true);
            _contactConnector.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(anotherContact);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Read(contact));

            Assert.Equal(ResultCode.ContactNotBelongToUserGroup.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task Read_ValidContact_Success()
        {
            var contact = new Contact()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID)
            };

            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(true);
            _contactConnector.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(contact);

            var contectResult = await _contactHandler.Read(contact);

            Assert.Equal(contectResult.Id, contact.Id);
            Assert.Equal(contectResult.GroupId, contact.GroupId);
        }

        #endregion

        #region CreateBulk

        [Fact]
        public async Task CreateBulk_UserProgramExpired_ReturnException()
        {
            var contacts = new Contacts()
            {
                Base64 = "validBase64"
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.CreateBulk(contacts));

            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        //[Fact]
        //public void CreateBulk_ContactAlreadyExists_ReturnDbException()
        //{
        //    var contacts = new Contacts
        //    {
        //        Base64 = "data:application/vnd.ms-excel;base64,RnVsbE5hbWUsRW1haWwsUGhvbmVOdW1iZXIsU2VuZGluZ01ldGhvZA0KYXZpYSxhdmlheUBjb21kYS5jby5pbCwsMQ0K"
        //    };
        //    FileType fileType = FileType.CSV;
        //    _dbConnectorMock.Setup(x => x.Programs.IsProgramExpired(It.IsAny<User>())).Returns(false);
        //    _dataUriScheme.Setup(x => x.IsValidFileType(contacts.Base64, out fileType)).Returns(false);
        //    //_csvHandler.Setup(x => x.ConvertCsvToModel(It.IsAny<TextReader>(), It.IsAny<Func<It.IsAnyType,new >()));
        //    //var actual = Assert.Throws<InvalidOperationException>(() => _contactHandler.CreateBulk(contacts, out int totalCount));

        //    //Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);

        //}

        #endregion

        #region Read_Search

        [Fact]
        public async Task Read_Search_DBReturnNull_ReturnNull()
        {
            string key = "user";
            int offset = 0;
            int limit = 10;
            bool popular = false;
            bool recent = false;
            int totalCount = 0;
            List<Contact> result = null;
            _contactConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<bool>(), It.IsAny<bool>(), out totalCount, It.IsAny<bool>()))
                            .Returns(result);

            bool includeTabletMode = false;
            (var actual, totalCount) = await _contactHandler.Read(key, offset, limit, popular, recent, includeTabletMode);

            Assert.Null(actual);
        }

        [Fact]
        public async Task Read_Search_NotExistContacts_ReturnEmptyList()
        {
            string key = "notExist";
            int offset = 0;
            int limit = 10;
            bool popular = false;
            bool recent = false;
            int totalCount = 0;
            List<Contact> result = new List<Contact>();
            _contactConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<bool>(), It.IsAny<bool>(), out totalCount, It.IsAny<bool>()))
                            .Returns(result);

            bool includeTabletMode = false;
            (var actual, totalCount) =await _contactHandler.Read(key, offset, limit, popular, recent, includeTabletMode);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Read_Search_Success()
        {
            string key = "user";
            int offset = 0;
            int limit = 10;
            bool popular = false;
            bool recent = false;
            int totalCount = 0;
            List<Contact> result = new List<Contact>() { new Contact { Email = "user@comda.co.il" } };
            _contactConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<bool>(), It.IsAny<bool>(), out totalCount, It.IsAny<bool>()))
                            .Returns(result.FindAll(x => x.Email.StartsWith(key)));

            bool includeTabletMode = false;
            (var actual, totalCount) =await _contactHandler.Read(key, offset, limit, popular, recent, includeTabletMode);

            Assert.Single(actual);
        }


        #endregion

        #region Update

        [Fact]
        public async Task Update_UserProgramExipred_ReturnException()
        {
            var contact = new Contact()
            {
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il"
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Update(contact));

            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_NotActiveContact_ReturnException()
        {
            var contact = new Contact()
            {
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il"
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(false);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Update(contact));

            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_ContactBelongToOtherGroup_ReturnException()
        {
            var contact = new Contact()
            {
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il",
                GroupId = Guid.NewGuid()
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(true);
            _contactConnector.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(new Contact { GroupId = Guid.NewGuid() });

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Update(contact));

            Assert.Equal(ResultCode.ContactNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_ContactAlreadyExist_ReturnException()
        {
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il",
                GroupId = new Guid(GUID)
            };
            var existContact = new Contact
            {
                Id = Guid.NewGuid(),
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il"
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(true);
            _contactConnector.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(new Contact { GroupId = new Guid(GUID) });
            _contactConnector.Setup(x => x.ReadByContactMeans(It.IsAny<Contact>())).ReturnsAsync(existContact);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Update(contact));

            Assert.Equal(ResultCode.ContactAlreadyExists.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Update_ValidContactWithoutSeals_Succsess()
        {
            var newID = Guid.NewGuid();
          
            var contact = new Contact
            {
                Id = newID,
                Name = "contact",
                DefaultSendingMethod = SendingMethod.Email,
                Email = "contact@comda.co.il",
                GroupId = new Guid(GUID)
            };
            Contact existContact = null;
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(true);
            _contactConnector.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(new Contact { GroupId = new Guid(GUID) });
            _contactConnector.Setup(x => x.ReadByContactMeans(It.IsAny<Contact>())).ReturnsAsync(existContact);

            _groupConnector.Setup(x => x.Read(It.IsAny<Group>())).ReturnsAsync(new Group());
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company());
            _contactFileWrapper.Setup(x => x.SaveSeals(It.Is<Contact>(x => EmptyContactSeals(x,contact.Seals))));
           
            await _contactHandler.Update(contact);
            _contactFileWrapper.Verify(x => x.SaveSeals(It.IsAny<Contact>()), Times.Once);



        }

        private bool EmptyContactSeals(Contact contact, IEnumerable<Seal> seals)
        {
            Assert.Equal(contact.Seals, seals);
            return true;
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_BasicUser_ReturnException()
        {
            var contact = new Contact()
            {
                Id = new Guid(GUID)
            };
            
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>())).Throws(new InvalidOperationException(ResultCode.UserIsNotEditorOrCompanyAdmin.GetNumericString()));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Delete(contact));

            Assert.Equal(ResultCode.UserIsNotEditorOrCompanyAdmin.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Delete_UserProgramExpired_ReturnException()
        {
            var contact = new Contact()
            {
                Id = new Guid(GUID)
            };
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>())).Throws(new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString()));

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Delete(contact));

            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Delete_InActiveContact_ReturnException()
        {
            var contact = new Contact()
            {
                Id = new Guid(GUID)
            };
            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(false);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Delete(contact));

            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }
        [Fact]
        public async Task Delete_ContactBelongToOtherGroup_ReturnException()
        {
            var contact = new Contact()
            {
                Id = new Guid(GUID)
            };
            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(true);
            _contactConnector.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(new Contact { GroupId = Guid.NewGuid() });

            var actual =await  Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Delete(contact));

            Assert.Equal(ResultCode.ContactNotBelongToUserGroup.GetNumericString(), actual.Message);
        }


        [Fact]
        public async Task Delete_ValidContact_Success()
        {
            var contact = new Contact()
            {
                Id = new Guid(GUID_TO_DELETE),
                UserId = new Guid(GUID),
                GroupId = new Guid(GUID),
                Status = ContactStatus.Activated
            };
            _contactConnector.Setup(x => x.IsActive(It.IsAny<Contact>())).ReturnsAsync(true);
            _contactConnector.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(contact);
            _contactConnector.Setup(x => x.Delete(It.IsAny<Contact>())).Callback(() =>
            {
                contact.Status = ContactStatus.Deleted;
            });

            await _contactHandler.Delete(contact);

            Assert.Equal(ContactStatus.Deleted, contact.Status);
        }

        #endregion

        #region GetOrCreateContact

        [Fact]
        public async Task GetOrCreateContact_ContactWithEmailExist_Success()
        {
            string signerMeans = "validEmail@comda.co.il";
            string signerName = "signerName";
            int totalCount = 0;
            List<Contact> result = new List<Contact>() { new Contact { DefaultSendingMethod = SendingMethod.Email } };
            _contactConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<bool>(), It.IsAny<bool>(), out totalCount, It.IsAny<bool>()))
                            .Returns(result);

            var actual = await _contactHandler.GetOrCreateContact(signerName, signerMeans);

            Assert.NotNull(actual);
            Assert.Equal(SendingMethod.Email, actual.DefaultSendingMethod);
        }

        [Fact]
        public async Task GetOrCreateContact_ContactWithPhoneExist_Success()
        {
            string signerMeans = "0522222222";
            string signerName = "signerName";
            int totalCount = 0;
            List<Contact> result = new List<Contact>() { new Contact { DefaultSendingMethod = SendingMethod.SMS } };
            _contactConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<bool>(), It.IsAny<bool>(), out totalCount, It.IsAny<bool>()))
                            .Returns(result);

            var actual = await _contactHandler.GetOrCreateContact(signerName, signerMeans);

            Assert.NotNull(actual);
            Assert.Equal(SendingMethod.SMS, actual.DefaultSendingMethod);
        }

        [Fact]
        public async Task GetOrCreateContact_UserProgramExpired_ReturnException()
        {
            string signerMeans = "validEmail@comda.co.il";
            string signerName = "signerName";
            int totalCount = 0;
            List<Contact> result = new List<Contact>();
            _contactConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<bool>(), It.IsAny<bool>(), out totalCount, It.IsAny<bool>()))
                            .Returns(result);
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.GetOrCreateContact(signerName, signerMeans));

            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        #endregion

        #region GetContactForSimpleDocument

        [Fact]
        public async Task GetContactForSimpleDocument_ContactExist_Success()
        {
            string signerMeans = "validEmail@comda.co.il";
            string signerName = "signerName";
            int totalCount = 0;
            List<Contact> result = new List<Contact>() { new Contact { Email = signerMeans, Name = signerName } };
            _contactConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<bool>(), It.IsAny<bool>(), out totalCount, It.IsAny<bool>()))
                            .Returns(result);

            var actual = await _contactHandler.GetContactForSimpleDocument(signerMeans, signerName, SendingMethod.Email);

            Assert.NotNull(actual);
            Assert.Equal(signerMeans, actual.Email);
        }


        #endregion

        #region ContactsGroupRead


      

        [Fact]
        public async Task Read_Groups_UserNoInTheSameGroup_ReturnException()
        {
            ContactsGroup contactsGroup = new ContactsGroup()
            {
                GroupId = new Guid(GUID_1),
            };

            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
           
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<ContactsGroup>())).ReturnsAsync(contactsGroup);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.Read(contactsGroup));
            Assert.Equal(ResultCode.InvalidContactsSendingGroupId.GetNumericString(), actual.Message);

        }
        [Fact]
        public async Task Read__Group_ValidGroup_Success()
        {
            ContactsGroup sendcontactsGroup = new ContactsGroup()
            {
                GroupId = new Guid(GUID_1),
            };
            ContactsGroup returnGroup = new ContactsGroup()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
            };

            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
      
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<ContactsGroup>())).ReturnsAsync(returnGroup);

            var result = await _contactHandler.Read(sendcontactsGroup);
            Assert.Equal(result.Id, returnGroup.Id);

        }


        [Fact]
        public async Task Read_Groups_Search_DBReturnNull_ReturnNull()
        {
            int totalCount;
            List<ContactsGroup> result = null;
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(), out totalCount))
                            .Returns(result);


            (var actual, totalCount) =await _contactHandler.ReadGroups("return null", 0, 10);

            Assert.Null(actual);
        }

        [Fact]
        public async Task Read_Group_Search_NotExistContacts_ReturnEmptyList()
        {
            int totalCount;
            List<ContactsGroup> result = new List<ContactsGroup>();
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(),
                                                       out totalCount))
                            .Returns(result);


            (var actual, totalCount) = await _contactHandler.ReadGroups("Empty", 0, 10);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Read_Group_Search_Success()
        {

            int totalCount = 0;
            string key = "Groups";
            List<ContactsGroup> result = new List<ContactsGroup>() {
                new ContactsGroup { Name = "Groups name" },
            new ContactsGroup{ Name = "NOT RETURN"} };
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                                                        It.IsAny<int>(), It.IsAny<int>(),
                                                         out totalCount))
                            .Returns(result.FindAll(x => x.Name.Contains(key)));


            (var actual,totalCount) =await _contactHandler.ReadGroups(key, 0, 10);

            Assert.Single(actual);
        }


        #endregion

        #region ContactsGroupCreate
        [Fact]
        public async Task CreateContactsGroup_SendNull_ReturnException()
        {
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.CreateContactsGroup(null));
            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);
        }
        [Fact]
        public async Task CreateContactsGroup_SendWithoutGroupName_ReturnException()
        {
            var newContactGroup = new ContactsGroup();
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.CreateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsGroupName.GetNumericString(), actual.Message);
        }
        [Fact]
        public async Task CreateContactsGroup_SendOver25Members_ReturnException()
        {
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };

            for (int i = 0; i < 26; ++i)
            {
                newContactGroup.ContactGroupMembers.Add(new ContactGroupMember());
            }
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.CreateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsSendingGroupMembersMaxLimit.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task CreateContactsGroup_SendMembersWithOrdersIssue_ReturnException()
        {
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };

            for (int i = 0; i < 3; ++i)
            {
                newContactGroup.ContactGroupMembers.Add(new ContactGroupMember() { Order = 2 });
            }
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await  Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.CreateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsSendingGroupOrderOfTheMembers.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task CreateContactsGroup_SendContactThatAreInDifferentGroup_ReturnException()
        {
            Guid contact1ID = Guid.NewGuid();
            Guid contact2ID = Guid.NewGuid();
            Guid contact3ID = Guid.NewGuid();
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 1,
                ContactId = contact1ID
            });

            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 2,
                ContactId = contact2ID
            });
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 3,
                ContactId = contact3ID
            });

            List<Contact> contacts = new List<Contact>()
            {
                new Contact
                {
                    Id = contact1ID,
                    Name="contact 1",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact2ID,
                    Name="contact 2",
                     GroupId= new Guid(GUID),
                     Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact3ID,
                    Name="contact 3",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                }
            };
            
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            _contactConnector.Setup(x => x.Read(It.IsAny<List<Guid>>())).ReturnsAsync(contacts);
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.CreateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateContactsGroup_SendContactThatAreNotExist_ReturnException()
        {

            Guid contact1ID = Guid.NewGuid();
            Guid contact2ID = Guid.NewGuid();
            Guid contact3ID = Guid.NewGuid();
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 1,
                ContactId = contact1ID
            });

            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 2,
                ContactId = contact2ID
            });
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 3,
                ContactId = contact3ID
            });

            List<Contact> contacts = new List<Contact>()
            {
                new Contact
                {
                    Id = contact1ID,
                    Name="contact 1",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact2ID,
                    Name="contact 2",
                     GroupId= new Guid(GUID_1),
                     Status = ContactStatus.Activated
                },
            };
            
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            _contactConnector.Setup(x => x.Read(It.IsAny<List<Guid>>())).ReturnsAsync(contacts);
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.CreateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateContactsGroup_SendContactsWithStatusDelete_ReturnException()
        {
            Guid contact1ID = Guid.NewGuid();
            Guid contact2ID = Guid.NewGuid();
            Guid contact3ID = Guid.NewGuid();
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 1,
                ContactId = contact1ID
            });

            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 2,
                ContactId = contact2ID
            });
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 3,
                ContactId = contact3ID
            });

            List<Contact> contacts = new List<Contact>()
            {
                new Contact
                {
                    Id = contact1ID,
                    Name="contact 1",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact2ID,
                    Name="contact 2",
                     GroupId= new Guid(GUID_1),
                     Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact3ID,
                    Name="contact 3",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Deleted
                }
            };
            
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            _contactConnector.Setup(x => x.Read(It.IsAny<List<Guid>>())).ReturnsAsync(contacts);
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.CreateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactInStatusDeleted.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateContactsGroup_TryToCreateNewGroupButGroupCountIsOverTheLimit_ReturnException()
        {
            
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            List<ContactsGroup> contatsGroup = new List<ContactsGroup>();
            for (int i = 0; i < 31; ++i)
            {
                contatsGroup.Add(new ContactsGroup());
            }
            int totalCount;
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), out totalCount)).Returns(contatsGroup);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.CreateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsSendingGroupMaxLimit.GetNumericString(), actual.Message);



        }

        [Fact]
        public async Task CreateContactsGroup_AddNewGroup_Success()
        {
            
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));

            List<ContactsGroup> contatsGroup = new List<ContactsGroup>();
            for (int i = 0; i < 5; ++i)
            {
                contatsGroup.Add(new ContactsGroup());
            }
            int totalCount;
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(),
              It.IsAny<int>(), It.IsAny<int>(), out totalCount)).Returns(contatsGroup);

            _contactsGroupsConnector.Setup(x => x.Create(newContactGroup)).Callback(() =>
            newContactGroup.Id = Guid.Parse(GUID));
           await _contactHandler.CreateContactsGroup(newContactGroup);
            Assert.Equal(newContactGroup.Id.ToString().ToLower(), GUID.ToLower());
        }
        #endregion

        #region UpdateContactsGroup

        [Fact]
        public async Task UpdateContactsGroup_SendNull_ReturnException()
        {
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.UpdateContactsGroup(null));
            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);

        }
        [Fact]
        public async Task UpdateContactsGroup_SendWithoutGroupName_ReturnException()
        {
            var newContactGroup = new ContactsGroup();
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.UpdateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsGroupName.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task UpdateContactsGroup_SendOver25Members_ReturnException()
        {
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };

            for (int i = 0; i < 26; ++i)
            {
                newContactGroup.ContactGroupMembers.Add(new ContactGroupMember());
            }
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.UpdateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsSendingGroupMembersMaxLimit.GetNumericString(), actual.Message);

        }

        [Fact]
        public async Task UpdateContactsGroup_SendMembersWithOrdersIssue_ReturnException()
        {
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };

            for (int i = 0; i < 3; ++i)
            {
                newContactGroup.ContactGroupMembers.Add(new ContactGroupMember() { Order = 2 });
            }
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.UpdateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsSendingGroupOrderOfTheMembers.GetNumericString(), actual.Message);

        }

        [Fact]
        public void UpdateContactsGroup_UpdatedGroupInDifferentGroup_ReturnException()
        {
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
        }
        [Fact]
        public async Task UpdateContactsGroup_GroupNotExist_ReturnException()
        {
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            ContactsGroup nullContactsGroup = null;
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<ContactsGroup>())).ReturnsAsync(nullContactsGroup);
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.UpdateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsSendingGroupId.GetNumericString(), actual.Message);
        }
        [Fact]
        public async Task UpdateContactsGroup_SendContactThatAreInDifferentGroup_ReturnException()
        {
            Guid contact1ID = Guid.NewGuid();
            Guid contact2ID = Guid.NewGuid();
            Guid contact3ID = Guid.NewGuid();
            var oldContactGroup = new ContactsGroup()
            {
                Name = "Foo_old",
                GroupId = new Guid(GUID_1),
            };
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 1,
                ContactId = contact1ID
            });

            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 2,
                ContactId = contact2ID
            });
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 3,
                ContactId = contact3ID
            });

            List<Contact> contacts = new List<Contact>()
            {
                new Contact
                {
                    Id = contact1ID,
                    Name="contact 1",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact2ID,
                    Name="contact 2",
                     GroupId= new Guid(GUID),
                     Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact3ID,
                    Name="contact 3",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                }
            };
            
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<ContactsGroup>())).ReturnsAsync(oldContactGroup);
            _contactConnector.Setup(x => x.Read(It.IsAny<List<Guid>>())).ReturnsAsync(contacts);
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.UpdateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task UpdateContactsGroup_SendContactThatAreNotExist_ReturnException()
        {

            Guid contact1ID = Guid.NewGuid();
            Guid contact2ID = Guid.NewGuid();
            Guid contact3ID = Guid.NewGuid();
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 1,
                ContactId = contact1ID
            });

            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 2,
                ContactId = contact2ID
            });
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 3,
                ContactId = contact3ID
            });
            var oldContactGroup = new ContactsGroup()
            {
                Name = "Foo_old",
                GroupId = new Guid(GUID_1),
            };
            List<Contact> contacts = new List<Contact>()
            {
                new Contact
                {
                    Id = contact1ID,
                    Name="contact 1",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact2ID,
                    Name="contact 2",
                     GroupId= new Guid(GUID_1),
                     Status = ContactStatus.Activated
                },
            };
            
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            _contactConnector.Setup(x => x.Read(It.IsAny<List<Guid>>())).ReturnsAsync(contacts);
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<ContactsGroup>())).ReturnsAsync(oldContactGroup);
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.UpdateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }
        
        [Fact]
        public async Task UpdateContactsGroup_SendContactsWithStatusDelete_ReturnException()
        {
            Guid contact1ID = Guid.NewGuid();
            Guid contact2ID = Guid.NewGuid();
            Guid contact3ID = Guid.NewGuid();
            var oldContactGroup = new ContactsGroup()
            {
                Name = "Foo_old",
                GroupId = new Guid(GUID_1),
            };
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 1,
                ContactId = contact1ID
            });

            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 2,
                ContactId = contact2ID
            });
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 3,
                ContactId = contact3ID
            });

            List<Contact> contacts = new List<Contact>()
            {
                new Contact
                {
                    Id = contact1ID,
                    Name="contact 1",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact2ID,
                    Name="contact 2",
                     GroupId= new Guid(GUID_1),
                     Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact3ID,
                    Name="contact 3",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Deleted
                }
            };
            
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<ContactsGroup>())).ReturnsAsync(oldContactGroup);
            _contactConnector.Setup(x => x.Read(It.IsAny<List<Guid>>())).ReturnsAsync(contacts);
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.UpdateContactsGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactInStatusDeleted.GetNumericString(), actual.Message);
        }


        [Fact]
        public async Task UpdateContactsGroup_UpdateContactsGroupSuccessfully_Success()
        {
                                                            
            Guid contact1ID = Guid.NewGuid();
            Guid contact2ID = Guid.NewGuid();
            Guid contact3ID = Guid.NewGuid();
            var oldContactGroup = new ContactsGroup()
            {
                Name = "Foo_old",
                GroupId = new Guid(GUID_1),
                CompanyId = new Guid(GUID_1),
            };
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 1,
                ContactId = contact1ID
            });

            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 2,
                ContactId = contact2ID
            });
            newContactGroup.ContactGroupMembers.Add(new ContactGroupMember()
            {
                Order = 3,
                ContactId = contact3ID
            });

            List<Contact> contacts = new List<Contact>()
            {
                new Contact
                {
                    Id = contact1ID,
                    Name="contact 1",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact2ID,
                    Name="contact 2",
                     GroupId= new Guid(GUID_1),
                     Status = ContactStatus.Activated
                },
                new Contact
                {
                    Id = contact3ID,
                    Name="contact 3",
                    GroupId= new Guid(GUID_1),
                    Status = ContactStatus.Activated
                }
            };

            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<ContactsGroup>())).ReturnsAsync(oldContactGroup);
            _contactConnector.Setup(x => x.Read(It.IsAny<List<Guid>>())).ReturnsAsync(contacts);
            _contactsGroupsConnector.Setup(x => x.Update(It.IsAny<ContactsGroup>()));
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
          await _contactHandler.UpdateContactsGroup(newContactGroup);
            Assert.Equal(newContactGroup.GroupId, new Guid(GUID_1));
            Assert.Equal(newContactGroup.CompanyId, new Guid(GUID_1));
        }


        #endregion

        #region DeleteContactGroup

        [Fact]
        public async Task DeleteContactGroup_SendNull_ReturnException()
        {
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await  Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.DeleteContactGroup(null));
            Assert.Equal(ResultCode.InvalidInput.GetNumericString(), actual.Message);

        }
        [Fact]
        public async Task DeleteContactGroup_GroupNotExist_ReturnException()
        {
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };

            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            ContactsGroup nullContactsGroup = null;
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<ContactsGroup>())).ReturnsAsync(nullContactsGroup);
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.DeleteContactGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsSendingGroupId.GetNumericString(), actual.Message);
        }
        [Fact]
        public async Task DeleteContactGroup_DeleteGroupInDifferentGroup_ReturnExceptionAsync()
        {
            var newContactGroup = new ContactsGroup()
            {
                Name = "Foo",
            };
            var dbContactsGroup = new ContactsGroup()
            {
                GroupId = new Guid(GUID),
            };
            _users.Setup(x => x.GetUser()).ReturnsAsync((new User()
            {
                Id = new Guid(GUID),
                GroupId = new Guid(GUID_1),
                Type = Common.Enums.Users.UserType.Editor
            }, _companySigner1Details));
            
            _contactsGroupsConnector.Setup(x => x.Read(It.IsAny<ContactsGroup>())).ReturnsAsync(dbContactsGroup);
            _validator.Setup(x => x.ValidateEditorUserPermissions(It.IsAny<User>()));
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.DeleteContactGroup(newContactGroup));
            Assert.Equal(ResultCode.InvalidContactsSendingGroupId.GetNumericString(), actual.Message);
        }

        #endregion
    }
}
