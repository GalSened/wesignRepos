namespace WeSign.areas.api.Controllers
{
    using Common.Interfaces;
    using Common.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using WeSign.Models;
    using WeSign.Models.Contacts;
    using WeSign.Models.Contacts.Responses;
    using Common.Extensions;
    using System.Text;
    using WeSign.Models.Documents;
    using Common.Models.Documents;
    using Common.Models.Users;
    using System.Threading.Tasks;

//#if DEBUG
//    [Route("userapi/v3/contacts")]
//#else
//    [Route("v3/contacts")]
//#endif
    [ApiController]
    [Area("Api")]
    [Route("v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "api")]
    [Authorize]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class ContactsController : ControllerBase
    {
        private readonly IContacts _contactsBl;

        public ContactsController(IContacts contactsBl)
        {
            _contactsBl = contactsBl;
        }

        /// <summary>
        /// Create new contact
        /// </summary>
        /// <remarks> 
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// SendingMethod: SMS = 1, Email = 2, Tablet = 3 <br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CreateContactResponseDTO))]
        public async Task<IActionResult> Create(ContactDTO input)
        {
            Contact contact = new Contact()
            {
                Name = input.Name,
                Email = input.Email,
                Phone = input.Phone,
                PhoneExtension = string.IsNullOrWhiteSpace(input.PhoneExtension) ? "+972" : input.PhoneExtension,
                DefaultSendingMethod = input.DefaultSendingMethod,
                Seals = input.Seals?.Select(s => new Seal { Name = s.Name, Base64Image = s.Base64Image }),
                SearchTag = input.SearchTag,
            };
            await _contactsBl.Create(contact);

            CreateContactResponseDTO result = new CreateContactResponseDTO() { ContactId = contact.Id };
            return Ok(result);
        }

        /// <summary>
        /// Create bulk of contacts from XLSX file
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("bulk")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CreateContactsResponseDTO))]
        public async Task<IActionResult> CreateBulk(ContactsDTO input)
        {
            Contacts contacts = new Contacts
            {
                Base64 = input.Base64File,
            };
            (IEnumerable<Guid> contactsId, int totalCount) = await _contactsBl.CreateBulk(contacts);
            CreateContactsResponseDTO result = new CreateContactsResponseDTO
            {
                ContactsId = contactsId
            };

            Response.Headers.Add("x-total-count", totalCount.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Delete a batch of contacts by their IDs
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="input">Batch of contact IDs to delete</param>
        /// <returns></returns>
        [HttpPut]
        [Route("deletebatch")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBatch(BatchRequestDTO input)
        {

            RecordsBatch contactsBatch = new RecordsBatch();
            foreach (string id in input.Ids?? Enumerable.Empty<string>())
            {
                Guid guidId = Guid.Parse(id);
                if (!contactsBatch.Ids.Contains(guidId))
                    contactsBatch.Ids.Add(guidId);
            }
            await _contactsBl.DeleteBatch(contactsBatch);
            return Ok();
        }

        /// <summary>
        /// Search contacts
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// key: key to search if contains in contact details - name\email\phone <br/>
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllContactResponseDTO))]
        public async Task<IActionResult> Read(string key = null, int offset = 0, int limit = 20, bool popular = false, bool recent = false, bool includeTabletMode = true)
        {
            (IEnumerable<Contact> contacts, int totalCount) = await _contactsBl.Read(key, offset, limit, popular, recent, includeTabletMode);

            List<ContactResponseDTO> response = new List<ContactResponseDTO>();
            foreach (Contact contact in contacts ?? Enumerable.Empty<Contact>())
            {
                response.Add(new ContactResponseDTO(contact));
            }

            Response.Headers.Add("x-total-count", totalCount.ToString());
            return Ok(new AllContactResponseDTO() { Contacts = response });
        }

        /// <summary>
        /// Get contact by ID
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ContactResponseDTO))]
        public async Task<IActionResult> Read(Guid id)
        {
            Contact contact = new Contact() { Id = id };
            contact = await _contactsBl.Read(contact);
            ContactResponseDTO response = new ContactResponseDTO(contact);

            return Ok(response);
        }

        /// <summary>
        /// Update contact
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update(Guid id, ContactDTO input)
        {
            Contact contact = new Contact()
            {
                Id = id,
                Name = input.Name,
                Email = input.Email,
                Phone = input.Phone,
                PhoneExtension = string.IsNullOrWhiteSpace(input.PhoneExtension) ? "+972": input.PhoneExtension,
                DefaultSendingMethod = input.DefaultSendingMethod,
                Seals = input.Seals.Select(s => new Seal { Name = s.Name, Base64Image = s.Base64Image }),
                SearchTag = input.SearchTag,
            };
           await _contactsBl.Update(contact);
            return Ok();
        }

        /// <summary>
        /// Delete contact
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(Guid id)
        {
            Contact contact = new Contact()
            {
                Id = id
            };
            await _contactsBl.Delete(contact);
            return Ok();
        }

        /// <summary>
        /// Get saved signature images for a specific document collection
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="docCollectionId">The document collection ID</param>
        /// <returns>List of signature images</returns>
        [HttpGet]
        [Route("signatures/{docCollectionId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SignaturesImagesDTO))]
        public async Task<IActionResult> ReadSignaturesImages(Guid docCollectionId)
        {            
            List<string> response = await _contactsBl.GetSelfSignContactSavedSignatures(docCollectionId);

            return Ok(new SignaturesImagesDTO { SignaturesImages = response });
        }

        /// <summary>
        /// Update saved signature images for a specific document collection
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="input">Signature images update data</param>
        /// <returns></returns>
        [HttpPut]
        [Route("signatures")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateSignaturesImages( SignaturesImagesDTO input)
        {
            SignaturesImage signaturesImage = new SignaturesImage
            {
                SignaturesImages = input.SignaturesImages.FirstOrDefault(),
                DocumentCollectionId = input.DocumentCollectionId
            };
            await _contactsBl.UpdateSelfSignContactSignaturesImages(signaturesImage);

            return Ok();
        }

        /// <summary>
        /// Get all contact groups
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="key">Search key for group name or member</param>
        /// <param name="offset">Offset for pagination</param>
        /// <param name="limit">Limit for pagination</param>
        /// <returns>List of contact groups</returns>
        [HttpGet]
        [Route("Groups")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllContactsGroupsResponseDTO))]
        public async Task<IActionResult> GetGroups(string key = null, int offset = 0, int limit = 20)
        {
           (IEnumerable<ContactsGroup> contactsGroups, int totalCount) = await _contactsBl.ReadGroups(key, offset, limit);

            AllContactsGroupsResponseDTO response = new AllContactsGroupsResponseDTO();
            foreach(ContactsGroup group in contactsGroups ?? Enumerable.Empty<ContactsGroup>())
            {
                ContactsGroupResponseDTO responseContactsGroup = new ContactsGroupResponseDTO();
                responseContactsGroup.Id = group.Id;
                responseContactsGroup.Name = group.Name;

                foreach(ContactGroupMember member in group.ContactGroupMembers 
                    ?? Enumerable.Empty<ContactGroupMember>())
                {
                    responseContactsGroup.ContactsGroupMembers.Add( new ContactGroupMemberResponseDTO()
                    {
                        ContactId = member.ContactId,
                        Id = member.Id,
                        Name = member.Contact?.Name,
                        Order = member.Order
                    });

                  
                }
                response.ContactGroups.Add(responseContactsGroup);

            }
            Response.Headers.Add("x-total-count", totalCount.ToString());
            return Ok(response);
        }

        /// <summary>
        /// Get a specific contact group by ID
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id">Contact group ID</param>
        /// <returns>Contact group details</returns>
        [HttpGet]
        [Route("Group/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ContactsGroupResponseDTO))]
        public async Task<IActionResult> GetGroup(Guid id)
        {
            ContactsGroup contactsGroup = new ContactsGroup()
            {
                Id = id,
            };
            ContactsGroup contactGroup = await _contactsBl.Read(contactsGroup);
            ContactsGroupResponseDTO responseContactsGroup = new ContactsGroupResponseDTO();
            responseContactsGroup.Id = contactGroup.Id;
            responseContactsGroup.Name = contactGroup.Name;
            if (contactGroup.ContactGroupMembers != null)
            {
                foreach (ContactGroupMember member in contactGroup.ContactGroupMembers.OrderBy(x => x.Order))
                {
                    responseContactsGroup.ContactsGroupMembers.Add(new ContactGroupMemberResponseDTO()
                    {
                        ContactId = member.ContactId,
                        Id = member.Id,
                        Name = member.Contact?.Name,
                        Order = member.Order,
                        Contact = new ContactResponseDTO(member.Contact)
                    });
                }
            }

            return Ok(responseContactsGroup);
        }


        /// <summary>
        /// Delete a contact group by ID
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id">Contact group ID</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Group/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            ContactsGroup contactsGroup = new ContactsGroup()
            {
                Id = id,
            };
            await _contactsBl.DeleteContactGroup(contactsGroup);
            return Ok();
        }

        /// <summary>
        /// Update a contact group by ID
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id">Contact group ID</param>
        /// <param name="input">Updated group data</param>
        /// <returns></returns>
        [HttpPut]
        [Route("Group/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateGroup(Guid id, ContactsGroupDTO input)
        {
            ContactsGroup contactsGroup = new ContactsGroup()
            {
                Id = id,
                Name = input.Name,
            };
            foreach(ContactGroupMemberDTO inputMember in input?.ContactsGroupMembers ?? Enumerable.Empty<ContactGroupMemberDTO>())
            {
                ContactGroupMember member = new ContactGroupMember()
                {
                    ContactId = inputMember.ContactId,
                    Order = inputMember.Order,
                    ContactsGroupId = id
                };
                contactsGroup.ContactGroupMembers.Add(member);
            }
            await _contactsBl.UpdateContactsGroup(contactsGroup);

            return Ok();
        }

        /// <summary>
        /// Create a new contact group
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="input">Contact group data</param>
        /// <returns>Created group ID</returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CreateContactGroupResponseDTO))]
        [Route("Group")]
        public async Task<IActionResult> CreateGroup(ContactsGroupDTO input)
        {
            ContactsGroup contactsGroup = new ContactsGroup()
            {
                
                Name = input.Name,
            };
            foreach (ContactGroupMemberDTO inputMember in input?.ContactsGroupMembers ?? Enumerable.Empty<ContactGroupMemberDTO>())
            {
                ContactGroupMember member = new ContactGroupMember()
                {
                    ContactId = inputMember.ContactId,
                    Order = inputMember.Order,                    
                };
                contactsGroup.ContactGroupMembers.Add(member);
            }
           await _contactsBl.CreateContactsGroup(contactsGroup);
            
            return Ok(new CreateContactGroupResponseDTO { Id = contactsGroup.Id});
        }
      
    }
}