using Castle.Core.Logging;
using Common.Interfaces.DB;
using Common.Models.ManagementApp;
using DAL.DAOs.Reports;
using DAL.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Connectors
{
    public class ManagementPeriodicReportEmailConnector : IManagementPeriodicReportEmailConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly Serilog.ILogger _logger;
        public ManagementPeriodicReportEmailConnector(IWeSignEntities weSignEntities, Serilog.ILogger logger)
        {
            _dbContext = weSignEntities;
            _logger = logger;
        }

        public void Create(ManagementPeriodicReportEmail managementPeriodicReportEmail)
        {
            try
            {
                var managementPeriodicReportEmailDAO = new ManagementPeriodicReportEmailDAO(managementPeriodicReportEmail);
                _dbContext.ManagementPeriodicReportEmails.Add(managementPeriodicReportEmailDAO);
                _dbContext.SaveChanges();
                managementPeriodicReportEmail.Id = managementPeriodicReportEmailDAO.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ManagementPeriodicReportEmailConnector_Create = ");
                throw;
            }
        }

        public IEnumerable<ManagementPeriodicReportEmail> ReadAllByReportId(Guid reportId)
        {
            try
            {
                var emails = _dbContext.ManagementPeriodicReportEmails.Where(_ => _.PeriodicReportId == reportId)
                .Select(_ => _.ToManagementPeriodicReportEmail());

                return emails == null ? Enumerable.Empty<ManagementPeriodicReportEmail>() : emails;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ManagementPeriodicReportEmailConnector_ReadAllByReportId = ");
                throw;
            }
        }

        public void DeleteAllByReportId(Guid reportId)
        {
            try
            {
                var query = _dbContext.ManagementPeriodicReportEmails.Where(_ => _.PeriodicReportId == reportId);
                _dbContext.ManagementPeriodicReportEmails.RemoveRange(query);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ManagementPeriodicReportEmailConnector_DeleteAllByReportId = ");
                throw;
            }
        }
    }
}
