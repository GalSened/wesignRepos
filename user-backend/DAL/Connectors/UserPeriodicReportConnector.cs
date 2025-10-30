using Common.Enums.Reports;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.Users;
using DAL.DAOs.Users;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class UserPeriodicReportConnector : IUserPeriodicReportConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IDater _dater;
        private readonly ILogger _logger;

        public UserPeriodicReportConnector(IWeSignEntities dbContext, IDater dater, ILogger logger)
        {
            _dbContext = dbContext;
            _dater = dater;
            _logger = logger;
        }

        public async Task Create(UserPeriodicReport userPeriodicReport)
        {
            try
            {
                await _dbContext.UserPeriodicReports.Where(_ => _.UserId == userPeriodicReport.UserId && _.ReportType == userPeriodicReport.ReportType).ExecuteDeleteAsync();
                var userPeriodicReportDAO = new UserPeriodicReportDAO(userPeriodicReport);
                _dbContext.UserPeriodicReports.Add(userPeriodicReportDAO);
                _dbContext.SaveChanges();
                userPeriodicReport.Id = userPeriodicReportDAO.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserPeriodicReportConnector_Create = ");
                throw;
            }
        }

        public IEnumerable<UserPeriodicReport> ReadByUser(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return Enumerable.Empty<UserPeriodicReport>();
                var reports = _dbContext.UserPeriodicReports.Where(_ => _.UserId == userId).ToList();
                return reports == null || !reports.Any() ? Enumerable.Empty<UserPeriodicReport>() : reports.Select(_ => _.ToUserPeriodicReport());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserPeriodicReportConnector_ReadByUser = ");
                throw;
            }
        }

        public IEnumerable<UserPeriodicReport> ReadReportsToSend()
        {
            try
            {
                var reports = _dbContext.UserPeriodicReports.Where(_ =>
                _.ReportFrequency == ReportFrequency.None ||
                _.ReportFrequency == ReportFrequency.Daily && _.LastTimeSent.AddHours(23) < _dater.UtcNow() ||
                _.ReportFrequency == ReportFrequency.Weekly && _.LastTimeSent.AddHours(167) < _dater.UtcNow() ||
                _.ReportFrequency == ReportFrequency.Monthly && _.LastTimeSent.AddHours(719) < _dater.UtcNow());

                return reports == null ? Enumerable.Empty<UserPeriodicReport>() : reports.Select(_ => _.ToUserPeriodicReport());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserPeriodicReportConnector_ReadReportsToSend = ");
                throw;
            }
        }

        public void UpdateReportSendTime(UserPeriodicReport report)
        {
            try
            {
                var userPeriodicReportDAO = _dbContext.UserPeriodicReports.FirstOrDefault(upr => upr.Id == report.Id);

                if (userPeriodicReportDAO != null)
                {
                    userPeriodicReportDAO.LastTimeSent = _dater.UtcNow();
                    _dbContext.UserPeriodicReports.Update(userPeriodicReportDAO);
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserPeriodicReportConnector_UpdateReportSendTime = ");
                throw;
            }
        }

        public async Task DeleteAllByUserId(Guid userId)
        {
            try
            {
                await _dbContext.UserPeriodicReports.Where(_ => _.UserId == userId).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserPeriodicReportConnector_DeleteAllByUserId = ");
                throw;
            }
        }
    }
}