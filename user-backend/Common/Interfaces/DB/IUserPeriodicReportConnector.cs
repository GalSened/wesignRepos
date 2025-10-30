using Common.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.DB
{
    public interface IUserPeriodicReportConnector
    {
        Task Create(UserPeriodicReport userPeriodicReport);
        IEnumerable<UserPeriodicReport> ReadByUser(Guid userId);
        IEnumerable<UserPeriodicReport> ReadReportsToSend();
        void UpdateReportSendTime(UserPeriodicReport report);
        Task DeleteAllByUserId(Guid userId);
    }
}
