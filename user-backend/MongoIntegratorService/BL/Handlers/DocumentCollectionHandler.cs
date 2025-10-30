using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Extensions;
using HistoryIntegratorService.Common.Interfaces;
using HistoryIntegratorService.Common.Interfaces.Connectors;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Requests;
using Microsoft.Extensions.Options;

namespace HistoryIntegratorService.BL.Handlers
{
    public class DocumentCollectionHandler : IDocumentCollection
    {
        private readonly GeneralSettings _generalSettings;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IEncryptor _encryptor;

        public DocumentCollectionHandler(IOptions<GeneralSettings> options, IDocumentCollectionConnector documentCollectionConnector, IEncryptor encryptor)
        {
            _generalSettings = options.Value;
            _documentCollectionConnector = documentCollectionConnector;
            _encryptor = encryptor;
        }

        public IEnumerable<DeletedDocumentCollection> Read(string appKey, DeletedDocumentCollectionRequest deletedDocRequest, out long totalCount)
        {
            if (string.IsNullOrEmpty(appKey) || _encryptor.Decrypt(_generalSettings.AppKey) != appKey)
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }
            if (deletedDocRequest.Offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (deletedDocRequest.Limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }

            var res = _documentCollectionConnector.Read(deletedDocRequest);
            if (res != null)
            {
                var docs = res.ToList();
                foreach (var doc in docs)
                {
                    if (doc.User != null)
                    {
                        doc.User.Id = doc.UserId;
                    }
                    else
                    {
                        doc.User = new User { Id = doc.UserId };
                    }
                }
                totalCount = docs.Count;
                return docs.Skip(deletedDocRequest.Offset).Take(deletedDocRequest.Limit);
            }
            totalCount = 0;
            return Enumerable.Empty<DeletedDocumentCollection>();
        }

        public void Create(string appKey, DeletedDocumentCollection deletedDoc)
        {
            if (string.IsNullOrEmpty(appKey) || _encryptor.Decrypt(_generalSettings.AppKey) != appKey)
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }
            _documentCollectionConnector.Create(deletedDoc);
        }
    }
}
