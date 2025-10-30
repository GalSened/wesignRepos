using Common.Interfaces.DB;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using DAL.Extensions;
using Common.Enums.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Serilog;

namespace DAL.Connectors
{
    public class SignersConnector : ISignersConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        public SignersConnector(IWeSignEntities dbContext, IServiceScopeFactory scopeFactory, ILogger logger)
        {
            _dbContext = dbContext;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<Signer> GetSignerById(Guid id)
        {
            try
            {
                var dbSigner = await _dbContext.Signers.Include(c => c.Contact).Include(d => d.DocumentCollection)
                .FirstOrDefaultAsync(x => x.Id == id);

                var signer = dbSigner.ToSigner();

                return signer;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignersConnector_GetSignerById = ");
                throw;
            }
        }

        public IEnumerable<Signer> ReadSignersByEmail(string email)
        {
            try
            {
                var dbSigners = _dbContext.Signers.Where(x => (x.Status == SignerStatus.Sent || x.Status == SignerStatus.Viewed) &&
                                                    x.Contact.Email == email);
                IEnumerable<Signer> signers = dbSigners.Select(x => x.ToSigner());

                return signers;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignersConnector_ReadSignersByEmail = ");
                throw;
            }
        }

        public IEnumerable<Signer> ReadPendingSigners(Guid documentCollectionId)
        {
            try
            {
                var dbSignersQuery = _dbContext.Signers.Where(s => s.DocumentCollectionId == documentCollectionId &&
                (s.Status == SignerStatus.Sent || s.Status == SignerStatus.Viewed));
                var signers = dbSignersQuery.Select(s => s.ToSigner()).AsEnumerable();
                return signers;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignersConnector_ReadSignersByEmail = ");
                throw;
            }
        }

        public async Task UpdateGeneratedOtpDetails(Signer signer)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();
                    var dbSigner = await dependencyService.Signers.Include(x => x.OtpDetails).AsTracking().FirstOrDefaultAsync(x => x.Id == signer.Id);

                    dbSigner.OtpDetails.Code = signer.SignerAuthentication.OtpDetails.Code;
                    dbSigner.OtpDetails.ExpirationTime = signer.SignerAuthentication.OtpDetails.ExpirationTime;
                    await dependencyService.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignersConnector_UpdateGeneratedOtpDetails = ");
                throw;
            }
        }

        public async Task UpdateOtpAttempts(Guid signerId, int attempts)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();
                    var dbSigner = await dependencyService.Signers.Include(x => x.OtpDetails).AsTracking().FirstOrDefaultAsync(x => x.Id == signerId);

                    if (dbSigner.OtpDetails != null)
                    {
                        dbSigner.OtpDetails.Attempts = attempts;

                        await dependencyService.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignersConnector_UpdateOtpAttempts = ");
                throw;
            }
        }

        public Task UpdateIdentificationAttempts(Signer signer)
        {
            try
            {
                return _dbContext.Signers.Where(s => s.Id == signer.Id).ExecuteUpdateAsync(
                   setters => setters.SetProperty(x => x.IdentificationAttempts, signer.IdentificationAttempts));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignersConnector_UpdateIdentificationAttempts = ");
                throw;
            }
        }

        public Task UpdateSignerStatus(Signer signer)
        {
            try
            {
                return _dbContext.Signers.Where(s => s.Id == signer.Id).ExecuteUpdateAsync(
                 setters => setters.SetProperty(x => x.TimeViewed, signer.TimeViewed).
                 SetProperty(x => x.Status, signer.Status).
                 SetProperty(x => x.FirstViewIPAddress, signer.FirstViewIPAddress));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignersConnector_UpdateSignerStatus = ");
                throw;
            }
        }

        public async Task UpdateSignerNotes(Guid signerId, string notes)
        {
            try
            {
                var dbSigner = await _dbContext.Signers
               .Include(s => s.Notes)
               .FirstOrDefaultAsync(s => s.Id == signerId);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dependencyService = scope.ServiceProvider.GetService<IWeSignEntities>();

                    if (dbSigner?.Notes != null)
                    {
                        dbSigner.Notes.SignerNote = notes;
                        dependencyService.Signers.Update(dbSigner);
                        await dependencyService.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignersConnector_UpdateSignerNotes = ");
                throw;
            }
        }
    }
}