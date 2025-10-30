using Common.Consts;
using Common.Enums.Logs;
using Common.Interfaces.DB;
using Common.Models;
using DAL.DAOs.Logs;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Microsoft.Data.SqlClient;
using Serilog;

namespace DAL.Connectors
{
    public class LogsConnector : ILogConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ILogger _logger;
        public LogsConnector(IWeSignEntities dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IEnumerable<LogMessage> Read(LogApplicationSource source, string key, string from, string to, LogLevel
            logLevel, int offset, int limit, out int totalCount)
        {
            try
            {
                if (source == LogApplicationSource.SignerApp)
                {
                    return GetSignerApplicationLogs(key, from, to, logLevel, offset, limit, out totalCount);
                }
                if (source == LogApplicationSource.ManagementApp)
                {
                    return GetManagementApplicationLogs(key, from, to, logLevel, offset, limit, out totalCount);
                }
                return GetUserApplicationLogs(key, from, to, logLevel, offset, limit, out totalCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in LogsConnector_Read = ");
                throw;
            }
        }
        public void Delete(DateTime from, LogApplicationSource source)
        {
            try
            {
                int deletedNumberOfRecords = 0, retry = 10;
                int take = 500;
                int timeoutSqlExceptionCount = 0;

                do
                {
                    try
                    {
                        if (source == LogApplicationSource.UserApp)
                        {
                            deletedNumberOfRecords = _dbContext.Logs.Where(x => x.TimeStamp <= from).Take(take).ExecuteDelete();
                        }
                        if (source == LogApplicationSource.SignerApp)
                        {
                            deletedNumberOfRecords = _dbContext.SignerLogs.Where(x => x.TimeStamp <= from).Take(take).ExecuteDelete();
                        }

                        if (source == LogApplicationSource.ManagementApp)
                        {
                            deletedNumberOfRecords = _dbContext.ManagementLogs.Where(x => x.TimeStamp <= from).Take(take).ExecuteDelete();

                        }
                    }
                    catch (SqlException ex) when (ex.Number == -2)  // -2 is a sql timeout
                    {
                        take = take / 10;
                        take++;
                        deletedNumberOfRecords = take;
                        if (timeoutSqlExceptionCount >= 2)
                        {
                            throw ex;
                        }
                        timeoutSqlExceptionCount++;

                    }
                    retry--;
                } while (deletedNumberOfRecords >= take && retry > 0);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in LogsConnector_Delete = ");
                throw;
            }
        }

        #region Private Functions
        private IEnumerable<LogMessage> GetUserApplicationLogs(string key, string from, string to, LogLevel logLevel, int offset, int limit, out int totalCount)
        {
            IQueryable<LogDAO> query = logLevel == LogLevel.All ?
                                _dbContext.Logs.ToList().AsQueryable() :
                                _dbContext.Logs.Where(t => t.Level == logLevel.ToString());

            query = string.IsNullOrWhiteSpace(key) ? query :
                            query.Where(x => x.Message.Contains(key));


            if (!string.IsNullOrWhiteSpace(from))
            {
                query = query.Where(t => t.TimeStamp >= DateTime.Parse(from));
            }
            if (!string.IsNullOrWhiteSpace(to))
            {
                query = query.Where(t => t.TimeStamp <= DateTime.Parse(to));
            }
            totalCount = query.Count();
            query = limit != Consts.UNLIMITED ?
                    query.OrderByDescending(x => x.TimeStamp).Skip(offset).Take(limit) :
                    query.OrderByDescending(x => x.TimeStamp).Skip(offset);

            return query.Select(t => t.ToLog()).AsEnumerable();
        }

        private IEnumerable<LogMessage> GetSignerApplicationLogs(string key, string from, string to, LogLevel logLevel, int offset, int limit, out int totalCount)
        {
            IQueryable<SignerLogDAO> query = logLevel == LogLevel.All ?
                                _dbContext.SignerLogs.ToList().AsQueryable() :
                                _dbContext.SignerLogs.Where(t => t.Level == logLevel.ToString());

            query = string.IsNullOrWhiteSpace(key) ? query :
                            query.Where(x => x.Message.Contains(key));


            if (!string.IsNullOrWhiteSpace(from))
            {
                query = query.Where(t => t.TimeStamp >= DateTime.Parse(from));
            }
            if (!string.IsNullOrWhiteSpace(to))
            {
                query = query.Where(t => t.TimeStamp <= DateTime.Parse(to));
            }
            totalCount = query.Count();
            query = limit != Consts.UNLIMITED ?
                    query.OrderByDescending(x => x.TimeStamp).Skip(offset).Take(limit) :
                    query.OrderByDescending(x => x.TimeStamp).Skip(offset);

            return query.Select(t => t.ToLog()).AsEnumerable();
        }

        private IEnumerable<LogMessage> GetManagementApplicationLogs(string key, string from, string to, LogLevel logLevel, int offset, int limit, out int totalCount)
        {
            IQueryable<ManagementLogDAO> query = logLevel == LogLevel.All ?
                                _dbContext.ManagementLogs.ToList().AsQueryable() :
                                _dbContext.ManagementLogs.Where(t => t.Level == logLevel.ToString());

            query = string.IsNullOrWhiteSpace(key) ? query :
                            query.Where(x => x.Message.Contains(key));


            if (!string.IsNullOrWhiteSpace(from))
            {
                query = query.Where(t => t.TimeStamp >= DateTime.Parse(from));
            }
            if (!string.IsNullOrWhiteSpace(to))
            {
                query = query.Where(t => t.TimeStamp <= DateTime.Parse(to));
            }
            totalCount = query.Count();
            query = limit != Consts.UNLIMITED ?
                    query.OrderByDescending(x => x.TimeStamp).Skip(offset).Take(limit) :
                    query.OrderByDescending(x => x.TimeStamp).Skip(offset);

            return query.Select(t => t.ToLog()).AsEnumerable();
        }
        #endregion
    }
}
