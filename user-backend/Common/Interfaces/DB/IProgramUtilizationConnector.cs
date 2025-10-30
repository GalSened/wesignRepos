
namespace Common.Interfaces.DB
{
    using Common.Enums;
    using Common.Models;
    using Common.Models.Programs;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IProgramUtilizationConnector
    {
        Task Create(ProgramUtilization programUtilization);
        Task<ProgramUtilization> Read(ProgramUtilization programUtilization);
        IEnumerable<ProgramUtilization> Read(int offset, int limit, out int totalCount);
        Task Delete(ProgramUtilization programUtilization);
        Task UpdateUsersAmount(User user, CalcOperation operation, int additionRangeSize = 1);
        Task UpdateTemplatesAmount(User user, CalcOperation operation, int additionRangeSize = 1);
        Task AddDocument(User user);
        Task AddDocument(User user, int additionRangeSize = 1);
        Task AddSms(User user, int additionRangeSize = 1);
        Task AddVisualIdentification(User user, int additionRangeSize = 1);
        Task RemoveVisualIdentification(User user, int additionRangeSize = 1);
        Task AddVideoConfrence(User user, int additionRangeSize = 1);
        Task RemoveUser(User user);
        Task Update(ProgramUtilization programUtilization);
        void FixUnsetProgramLastResetDate();
    }
}
