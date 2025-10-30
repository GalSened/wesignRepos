using Common.Interfaces.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.Files
{
    public class FilesWrapper : IFilesWrapper
    {
        public IDocumentFileWrapper Documents { get; }
        public IContactFileWrapper Contacts { get; }        
        public IUserFileWrapper Users { get; }
        public  ISignerFileWrapper Signers { get; }
        public IConfigurationFileWrapper Configurations { get; }

        public FilesWrapper(IDocumentFileWrapper documentFileWrapper, IContactFileWrapper contact,
             IUserFileWrapper user, ISignerFileWrapper signer, IConfigurationFileWrapper configurations  )
        {
            Documents = documentFileWrapper;
            Contacts = contact;
            Users = user;
            Signers = signer;
            Configurations = configurations;

        }
    }
}
