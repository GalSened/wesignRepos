using Common.Enums.Documents;
using Common.Models;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IDocumentCollectionOperationsNotifier
    {
        Task AddNotification(DocumentCollection documentCollection, DocumentNotification notifiaction, Signer signer);
    }
}
