using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SignerBL.Handlers
{
    public class ContactsHandler : Common.Interfaces.SignerApp.IContacts
    {
        
        private readonly IJWT _jwt;        
        private readonly IContactSignatures _contactSignatures;
        private readonly Common.Interfaces.SignerApp.ISignerValidator _validator;
        private readonly ISignerTokenMappingConnector _signerTokenMappingConnector;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;

        public ContactsHandler(ISignerTokenMappingConnector signerTokenMappingConnector, IDocumentCollectionConnector documentCollectionConnector, IJWT jwt,
            Common.Interfaces.SignerApp.ISignerValidator validator, IContactSignatures contactSignatures)
        {
            _signerTokenMappingConnector = signerTokenMappingConnector;
            _documentCollectionConnector = documentCollectionConnector;
            _jwt = jwt;
            _validator = validator;
            _contactSignatures = contactSignatures;
        }

        public async Task<IEnumerable<string>> ReadSignaturesImages(SignerTokenMapping signerTokenMapping)
        {
            var contact =await ValidateToken(signerTokenMapping);
            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }

            return _contactSignatures.GetContactSavedSignatures(contact);
            
        }

        public async Task UpdateSignaturesImages(SignerTokenMapping signerTokenMapping, IEnumerable<string> signaturesImages)
        {
            var contact =await ValidateToken(signerTokenMapping);
            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidSignerId.GetNumericString());
            }

            await _contactSignatures.UpdateSignaturesImages(contact, signaturesImages.ToList());
        }

        #region Private Functions
        


        private async Task<Contact> ValidateToken(SignerTokenMapping signerTokenMapping)
        {
            signerTokenMapping = await _signerTokenMappingConnector.Read(signerTokenMapping);
            if (signerTokenMapping == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            var signer = _jwt.GetSigner(signerTokenMapping.JwtToken);
            if (signer == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            var documentCollection = await _documentCollectionConnector.Read(new DocumentCollection { Id = signerTokenMapping.DocumentCollectionId });
            if (documentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            var contact = documentCollection.Signers.FirstOrDefault(x => x.Id == signer.Id)?.Contact;

            return contact;
        }

        #endregion
    }
}
