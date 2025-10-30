namespace Common.Interfaces.DB
{
    using Common.Models;
    using System.Collections.Generic;
    using Common.Consts;
    using System.Threading.Tasks;

    public interface IProgramConnector
    {
        Task Create(Program program);
        Task<bool> Exists(Program program);
        /// <summary>
        /// Check if can add users to company user's program utilization
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> CanAddUser(User user);

        /// <summary>
        /// Check if can add template to user's program utilization
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> CanAddTemplate(User user);
        Task<bool> CanAddDocument(User user, int additionRangeSize = 1);
        Task<bool> CanAddSms(User user, int additionRangeSize = 1);
        Task<bool> CanAddVisualIdentifications(User user, int additionRangeSize = 1);

        Task<bool> CanAddVideoConference(User user, int additionRangeSize = 1);
        Task Delete(Program program);
        Task<bool> IsProgramExpired(User user);
        Task<Program> Read(Program program);
        IEnumerable<Program> Read(string key, int offset, int limit, out int totalCount);
        IEnumerable<Program> Read(int minDocs, int minSMS, bool? isUsed ,int offset, int limit, out int totalCount);
        bool IsFreeTrialUser(User user);
        Task Update(Program program);
    }
}
