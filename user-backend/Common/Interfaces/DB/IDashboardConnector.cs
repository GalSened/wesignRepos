using Common.Models.Dashboard;
using System;

namespace Common.Interfaces.DB
{
    public interface IDashboardConnector
    {
        DashboardView ReadDashboardView(Guid groupId);
    }
}
