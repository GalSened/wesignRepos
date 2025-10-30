using Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.Reports
{
    public interface IHistoryDocumentCollection
    {
        Task<IEnumerable<DocumentCollection>> ReadOldDocuments(DateTime from, DateTime to, Guid? userId, IEnumerable<Guid> groupIds = null, int offset = 0, int limit = int.MaxValue);
    }
}
