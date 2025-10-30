using HistoryIntegratorService.Requests;
using HistoryIntegratorService.Common.Models;

namespace HistoryIntegratorService.Common.Interfaces
{
    public interface IDocumentCollection
    {
        IEnumerable<DeletedDocumentCollection> Read(string appKey, DeletedDocumentCollectionRequest deletedDocRequest, out long totalCount);
        void Create(string appKey, DeletedDocumentCollection deletedDoc);
    }
}
