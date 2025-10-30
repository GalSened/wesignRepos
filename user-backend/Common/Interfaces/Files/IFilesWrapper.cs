using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Files
{
    public interface IFilesWrapper
    {
        IDocumentFileWrapper Documents { get;  }
        IContactFileWrapper Contacts { get;  }
        IUserFileWrapper Users { get;  }
        ISignerFileWrapper Signers { get;  }
        IConfigurationFileWrapper Configurations { get; }


    }
}
