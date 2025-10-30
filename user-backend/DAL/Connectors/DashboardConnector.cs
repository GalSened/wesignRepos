using Common.Enums.Documents;
using Common.Interfaces.DB;
using Common.Models.Dashboard;
using Serilog;
using System;
using System.Linq;

namespace DAL.Connectors
{
    public class DashboardConnector : IDashboardConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ILogger _logger;

        public DashboardConnector(IWeSignEntities dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public DashboardView ReadDashboardView(Guid groupId)
        {
            try
            {
                if (groupId == Guid.Empty)
                    return null;
                var result = _dbContext.DocumentCollections
                    .Where(dc => dc.GroupId == groupId)
                    .GroupBy(dc => dc.GroupId)
                    .Select(g => new DashboardView()
                    {
                        GroupId = groupId,
                        SignedDocsAmount = g.Count(dc => dc.Status == DocumentStatus.Signed),
                        PendingDocsAmount = g.Count(dc => dc.Status == DocumentStatus.Sent || dc.Status == DocumentStatus.Viewed),
                        DeclinedDocsAmount = g.Count(dc => dc.Status == DocumentStatus.Declined),
                        CanceledDocsAmount = g.Count(dc => dc.Status == DocumentStatus.Canceled)
                    }).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DashboardConnector_ReadDashboardView = ");
                throw;
            }
        }
    }
}
