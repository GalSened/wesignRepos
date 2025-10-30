using Common.Enums.Logs;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Documents.Signers;
using Serilog;
using System;
using System.Threading.Tasks;

namespace SignerBL.Handlers
{
    public class LogsHandler : ILogs
    {
        private readonly ILogger _logger;
        private readonly ISignerTokenMappingConnector _signerTokenMappingConnector;
        private readonly IJWT _jwt;

        public LogsHandler(ILogger logger, ISignerTokenMappingConnector signerTokenMappingConnector, IJWT jwt)
        {
            _logger = logger;
            _signerTokenMappingConnector = signerTokenMappingConnector;
            _jwt = jwt;
        }

        public async Task Create(string token, LogMessage logMessage)
        {
            await ValidateToken(token);

            switch (logMessage.LogLevel)
            {
                case LogLevel.Debug:
                    _logger.Debug(logMessage.Message);
                    break;
                case LogLevel.Information:
                    _logger.Information(logMessage.Message);
                    break;
                case LogLevel.Error:
                    if (string.IsNullOrWhiteSpace(logMessage.Exception))
                    {
                        _logger.Error(logMessage.Message);
                    }
                    else
                    {
                        _logger.Error(logMessage.Exception, logMessage.Message);
                    }
                    break;
                default:
                    _logger.Debug(logMessage.Message);
                    break;
            }
        }

        private async Task ValidateToken(string token)
        {
            Guid.TryParse(token, out var newGuid);
            var signerTokenMapping = new SignerTokenMapping()
            {
                GuidToken = newGuid
            };
            var dbSignerTokenMapping =await _signerTokenMappingConnector.Read(signerTokenMapping);
            User user = null;
            Signer signer = null;
            try
            {
                signer = _jwt.GetSigner(dbSignerTokenMapping?.JwtToken);
            }
            catch (Exception)
            {

                user = _jwt.GetUser(token);
            }
            if (signer == null && user == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
        }
    }
}
