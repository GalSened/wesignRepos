using Common.Enums.Documents;
using Common.Models;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IDistribution
    {
        Task SendDocumentsUsingDistributionMechanism(IEnumerable<DocumentCollection> documentCollections);
        
        Task<IEnumerable<BaseSigner>> ExtractSignersFromExcel(string base64File);

        /// <summary>
        /// Get all distribution documents
        /// </summary>
        /// <param name="key"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        Task<(IEnumerable<DocumentCollection>, int)> Read(string key, string from, string to, int offset, int limit);
        
        /// <summary>
        /// Get all documents of distribution id
        /// </summary>
        /// <param name="distributionId"></param>
        /// <param name="key"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        Task<(IEnumerable<DocumentCollection>, int)> Read(Guid distributionId, string key, string from, string to, int offset, int limit,
            Dictionary<DocumentStatus, int> statusCounts);
        
        Task DeleteAllDocuments(Guid distributionId);
        Task ReSendUnSignedDocuments(Guid distributionId);
        Task ReSendDocumentsInStatus(Guid distributionId, DocumentStatus status);
        
        Task<(List<(string SignerName, IDictionary<Guid, (string name, byte[] content)> Files)>, string)> DownloadSignedDocuments(Guid distributionId);
    }
}
