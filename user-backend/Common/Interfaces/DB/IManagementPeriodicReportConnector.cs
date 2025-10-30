using Common.Models.ManagementApp.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.DB
{
    public interface IManagementPeriodicReportConnector
    {
        Task<Guid> Create(ManagementPeriodicReport managementPeriodicReport);
        IEnumerable<ManagementPeriodicReport> ReadAll();
        IEnumerable<ManagementPeriodicReport> ReadReportsToSend();
        Task Update(ManagementPeriodicReport report);
        void UpdateReportSendTime(ManagementPeriodicReport report);
        void Delete(Guid reportId);
    }
}
