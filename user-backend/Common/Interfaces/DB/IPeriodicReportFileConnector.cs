using Common.Models.Reports;
using System;
using System.Collections.Generic;

namespace Common.Interfaces.DB
{
    public interface IPeriodicReportFileConnector
    {
        Guid Create(PeriodicReportFile periodicReportFile);
        PeriodicReportFile Read(Guid id);
        void Delete(Guid id);
        IEnumerable<Guid> DeleteAllExpired();
    }
}
