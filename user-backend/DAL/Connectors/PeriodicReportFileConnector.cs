using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.Reports;
using Common.Models.Settings;
using DAL.DAOs.Reports;
using DAL.Extensions;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Connectors
{
    public class PeriodicReportFileConnector : IPeriodicReportFileConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IDater _dater;
        private readonly ILogger _logger;
        private readonly GeneralSettings _generalSettings;

        public PeriodicReportFileConnector(IWeSignEntities dbContext, IDater dater, ILogger logger, IOptions<GeneralSettings> options)
        {
            _dbContext = dbContext;
            _dater = dater;
            _logger = logger;
            _generalSettings = options.Value;
        }

        public Guid Create(PeriodicReportFile periodicReportFile)
        {
            try
            {
                var periodicReportFileDAO = new PeriodicReportFileDAO(periodicReportFile);
                _dbContext.PeriodicReportFiles.Add(periodicReportFileDAO);
                _dbContext.SaveChanges();
                periodicReportFile.Id = periodicReportFileDAO.Id;
                return periodicReportFile.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in PeriodicReportFileConnector_Create = ");
                throw;
            }
        }

        public PeriodicReportFile Read(Guid id)
        {
            try
            {
                return _dbContext.PeriodicReportFiles.Where(prf => prf.Id == id).Select(_ => _.ToPeriodicReportFile()).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in PeriodicReportFileConnector_Read = ");
                throw;
            }
        }

        public void Delete(Guid id)
        {
            try
            {
                var query = _dbContext.PeriodicReportFiles.FirstOrDefault(prf => prf.Id == id);
                _dbContext.PeriodicReportFiles.Remove(query);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in PeriodicReportFileConnector_Delete = ");
                throw;
            }
        }

        public IEnumerable<Guid> DeleteAllExpired()
        {
            try
            {
                var results = _dbContext.PeriodicReportFiles.Where(prf => prf.CreationTIme.AddHours(_generalSettings.PeriodicReportFileExpirationInHours) > _dater.UtcNow())
                .AsEnumerable();
                _dbContext.PeriodicReportFiles.RemoveRange(results);
                _dbContext.SaveChanges();
                return results.Select(_ => _.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in PeriodicReportFileConnector_DeleteAllExpired = ");
                throw;
            }
        }
    }
}
