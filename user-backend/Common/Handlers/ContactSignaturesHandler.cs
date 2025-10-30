using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers.Files;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers
{
    public class ContactSignaturesHandler : IContactSignatures
    {
       
        private readonly IValidator _validator;
   
        private readonly IFilesWrapper _filesWrapper;
    
        public ContactSignaturesHandler(IFilesWrapper filesWrapper,            
            IValidator validator)
        {
          
            _validator = validator;         
            _filesWrapper = filesWrapper;
        }


        public async Task UpdateSignaturesImages(Contact contact, List<string> signaturesImages)
        {
            List<string> signatureAfterClean = new List<string>();
            foreach (var signatureImage in signaturesImages ?? Enumerable.Empty<string>())
            {
                signatureAfterClean.Add((await _validator.ValidateIsCleanFile(signatureImage))?.CleanFile);
            }

            _filesWrapper.Contacts.UpdateSeals(contact, signatureAfterClean);

          
        }
        public List<string> GetContactSavedSignatures(Contact contact)        
        {            
            return _filesWrapper.Contacts.ReadSeals(contact);            
        }

    }
}
