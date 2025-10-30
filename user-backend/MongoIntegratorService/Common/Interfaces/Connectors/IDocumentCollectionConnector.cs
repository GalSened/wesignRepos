using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Requests;

namespace HistoryIntegratorService.Common.Interfaces.Connectors
{
    public interface IDocumentCollectionConnector
    {
        IEnumerable<DeletedDocumentCollection> Read(DeletedDocumentCollectionRequest deletedDoc);
        void Create(DeletedDocumentCollection deletedDocRequest);
    }
}
