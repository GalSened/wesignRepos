using Common.Enums.Reports;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.ManagementApp.Reports;
using DAL.DAOs.Management;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class ManagementPeriodicReportConnector : IManagementPeriodicReportConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IDater _dater;
        private readonly ILogger _logger;
        public ManagementPeriodicReportConnector(IWeSignEntities dbContext, IDater dater, ILogger logger)
        {
            _dbContext = dbContext;
            _dater = dater;
            _logger = logger;
        }

        public async Task<Guid> Create(ManagementPeriodicReport managementPeriodicReport)
        {
            try
            {
                await _dbContext.ManagementPeriodicReports.Where(_ => _.UserId == managementPeriodicReport.UserId && _.ReportType == managementPeriodicReport.ReportType).ExecuteDeleteAsync();
                var managementPeriodicReportDAO = new ManagementPeriodicReportDAO(managementPeriodicReport);
                _dbContext.ManagementPeriodicReports.Add(managementPeriodicReportDAO);
                _dbContext.SaveChanges();
                managementPeriodicReport.Id = managementPeriodicReportDAO.Id;
                return managementPeriodicReportDAO.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ManagementPeriodicReportConnector_Create = ");
                throw;
            }
        }

        public IEnumerable<ManagementPeriodicReport> ReadAll()
        {
            try
            {
                var reportsDAO = _dbContext.ManagementPeriodicReports.Where(_ => true);
                if (reportsDAO == null)
                {
                    return Enumerable.Empty<ManagementPeriodicReport>();
                }
                else
                {
                    var reports = reportsDAO.Select(_ => _.ToManagementPeriodicReport()).ToList();
                    for (int i = 0; i < reports.Count; i++)
                    {
                        var emails = _dbContext.ManagementPeriodicReportEmails.Where(_ => _.PeriodicReportId == reports[i].Id).AsEnumerable();
                        reports[i].Emails = emails.Select(_ => _.ToManagementPeriodicReportEmail()).ToList();
                    }
                    return reports;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ManagementPeriodicReportConnector_ReadAll = ");
                throw;
            }
        }

        public IEnumerable<ManagementPeriodicReport> ReadReportsToSend()
        {
            try
            {
                var reports = _dbContext.ManagementPeriodicReports.Where(_ =>
                _.ReportFrequency == ManagementReportFrequency.Weekly && _.LastTimeSent.AddHours(167) < _dater.UtcNow() ||
                _.ReportFrequency == ManagementReportFrequency.Monthly && _.LastTimeSent.AddHours(719) < _dater.UtcNow() ||
                _.ReportFrequency == ManagementReportFrequency.Yearly && _.LastTimeSent.AddHours(8759) < _dater.UtcNow());
                return reports == null ? Enumerable.Empty<ManagementPeriodicReport>() : reports.Select(_ => _.ToManagementPeriodicReport());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ManagementPeriodicReportConnector_ReadReportsToSend = ");
                throw;
            }
        }

        public async Task Update(ManagementPeriodicReport report)
        {
            try
            {
                var managementPeriodicReportDAO = _dbContext.ManagementPeriodicReports.Local.FirstOrDefault(mpr => mpr.Id == report.Id)
                                ?? _dbContext.ManagementPeriodicReports.FirstOrDefault(mpr => mpr.Id == report.Id);

                if (managementPeriodicReportDAO != null)
                {
                    managementPeriodicReportDAO.UserId = report.UserId;
                    managementPeriodicReportDAO.ReportType = report.ReportType;
                    managementPeriodicReportDAO.LastTimeSent = report.LastTimeSent;
                    managementPeriodicReportDAO.ReportFrequency = report.ReportFrequency;
                    managementPeriodicReportDAO.ReportParameters = report.ReportParameters;
                    _dbContext.ManagementPeriodicReports.Update(managementPeriodicReportDAO);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ManagementPeriodicReportConnector_Update = ");
                throw;
            }
        }

        public void UpdateReportSendTime(ManagementPeriodicReport report)
        {
            try
            {
                var managementPeriodicReportDAO = _dbContext.ManagementPeriodicReports.Local.FirstOrDefault(mpr => mpr.Id == report.Id)
                                ?? _dbContext.ManagementPeriodicReports.FirstOrDefault(mpr => mpr.Id == report.Id);

                if (managementPeriodicReportDAO != null)
                {
                    managementPeriodicReportDAO.LastTimeSent = _dater.UtcNow();
                    _dbContext.ManagementPeriodicReports.Update(managementPeriodicReportDAO);
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ManagementPeriodicReportConnector_UpdateReportSendTime = ");
                throw;
            }
        }

        public void Delete(Guid reportId)
        {
            try
            {
                var entity = _dbContext.ManagementPeriodicReports.FirstOrDefault(_ => _.Id == reportId);
                if (entity != null)
                {
                    _dbContext.ManagementPeriodicReports.Remove(entity);
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ManagementPeriodicReportConnector_Delete = ");
                throw;
            }
        }
    }
}
