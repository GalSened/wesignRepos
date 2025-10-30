using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;

namespace Common.Handlers
{
    public class AppendicesHandler : IAppendices
    {
        private readonly IFilesWrapper _fileWrapper;


        public AppendicesHandler(IFilesWrapper fileWrapper)
        {
            _fileWrapper = fileWrapper;
        }

        public void Create(DocumentCollection documentCollection)
        {
            documentCollection?.SenderAppendices.ForEach(x=>
            {                            
                _fileWrapper.Signers.CreateSharedAppendices(documentCollection, x);               
            });
        }

        public void Create(Guid documentCollectionId, Signer signer)
        {
            signer?.SenderAppendices.ForEach(appendix=>
            {
                _fileWrapper.Signers.CreateSignerAppendices(documentCollectionId, signer, appendix);               
            });
        }

        public IEnumerable<Appendix> Read(Guid documentCollectionId)
        {
            return _fileWrapper.Signers.ReadSharedAppendices(documentCollectionId);

        }

        public IEnumerable<Appendix> Read(Guid documentCollectionId, Guid signerId)
        {
            return _fileWrapper.Signers.ReadSignerAppendices(documentCollectionId, signerId);

        }

        public IEnumerable<Attachment> ReadForMail(Guid documentCollectionId)
        {
            return _fileWrapper.Signers.ReadSharedAppendicesAsAttachment(documentCollectionId);           
        }

        public IEnumerable<Attachment> ReadForMail(Guid documentCollectionId, Guid signerId)
        {
            return _fileWrapper.Signers.ReadSignerAppendicesAsAttachment(documentCollectionId, signerId);
           
        }

       

      
    }
}
