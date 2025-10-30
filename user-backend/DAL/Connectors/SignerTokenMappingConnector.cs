using Common.Interfaces.DB;
using Common.Models.Documents.Signers;
using DAL.DAOs.Documents.Signers;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class SignerTokenMappingConnector : ISignerTokenMappingConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly ILogger _logger;

        public SignerTokenMappingConnector(IWeSignEntities dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Create(SignerTokenMapping signerTokenMapping)
        {
            try
            {
                var signerTokenMappingDAO = new SignerTokenMappingDAO(signerTokenMapping);
                await _dbContext.SignerTokensMapping.AddAsync(signerTokenMappingDAO);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignerTokenMappingConnecter_Create = ");
                throw;
            }
        }

        public async Task<SignerTokenMapping> Read(SignerTokenMapping signerTokenMapping)
        {
            try
            {
                return (await _dbContext.SignerTokensMapping.FirstOrDefaultAsync(x => (signerTokenMapping.GuidToken != Guid.Empty && x.GuidToken == signerTokenMapping.GuidToken) ||
            (signerTokenMapping.SignerId != Guid.Empty && x.SignerId == signerTokenMapping.SignerId)
            || (signerTokenMapping.GuidAuthToken != Guid.Empty && x.GuidAuthToken == signerTokenMapping.GuidAuthToken)))
                                                 .ToSignerTokenMapping();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignerTokenMappingConnecter_Read = ");
                throw;
            }
        }

        public async Task Delete(SignerTokenMapping signerTokenMapping)
        {
            try
            {
                await _dbContext.SignerTokensMapping.Where(x => x.GuidToken == signerTokenMapping.GuidToken || x.SignerId == signerTokenMapping.SignerId).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignerTokenMappingConnecter_Delete = ");
                throw;
            }
        }

        public async Task Update(SignerTokenMapping signerTokenMapping)
        {
            try
            {
                var element = await _dbContext.SignerTokensMapping.FirstOrDefaultAsync(x => x.SignerId == signerTokenMapping.SignerId);
                if (element != null)
                {
                    element.GuidToken = signerTokenMapping.GuidToken;
                    element.ADName = signerTokenMapping.ADName;
                    element.JwtToken = signerTokenMapping.JwtToken;
                    element.GuidAuthToken = signerTokenMapping.GuidAuthToken;
                    element.DocumentCollectionId = signerTokenMapping.DocumentCollectionId;
                    element.AuthToken = signerTokenMapping.AuthToken;
                    element.AuthId = signerTokenMapping.AuthId;
                    element.AuthName = signerTokenMapping.AuthName;
                    _dbContext.SignerTokensMapping.Update(element);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SignerTokenMappingConnecter_Update = ");
                throw;
            }
        }
    }
}
