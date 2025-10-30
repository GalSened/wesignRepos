using Common.Models.Programs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.DB
{
    public interface IProgramUtilizationHistoryConnector
    {
        Task Create(ProgramUtilizationHistory programUtilizationHistory);
        IEnumerable<ProgramUtilizationHistory> Read(string key , int offset , int limit , out int totalCount, Guid companyId = default);

        IEnumerable<ProgramUtilizationHistory> Read(bool? isExpired, Guid? programId, DateTime? expiredFrom, DateTime? expiredTo, Guid companyId = default);
    }
}
