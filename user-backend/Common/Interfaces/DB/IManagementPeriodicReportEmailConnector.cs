using Common.Models.ManagementApp;
using System;
using System.Collections.Generic;

namespace Common.Interfaces.DB
{
    public interface IManagementPeriodicReportEmailConnector
    {
        void Create(ManagementPeriodicReportEmail managementPeriodicReportEmail);
        IEnumerable<ManagementPeriodicReportEmail> ReadAllByReportId(Guid reportId);
        void DeleteAllByReportId(Guid reportId);
    }
}
