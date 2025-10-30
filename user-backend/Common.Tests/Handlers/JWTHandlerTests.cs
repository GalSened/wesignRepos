using Common.Handlers;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using DAL;
using DAL.Connectors;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Common.Tests.Handlers
{
    public class JWTHandlerTests
    {
        private readonly JWTHandler _jwtHandler;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IOptions<GeneralSettings> _generalSettings;
        
        public JWTHandlerTests()
        {
            //WSE actual JwtSignerSignatureKey 
            _jwtSettings = Options.Create(new JwtSettings() { 
                SignerLinkExpirationInHours = 720,
                JwtSignerSignatureKey = "85081ca6-53e4-4625-bdb6-80f173460d80",
                JwtBearerSignatureKey = "49194210-c6f7-40cc-8ca9-8d00b0141107"
            });

            _generalSettings = Options.Create(new GeneralSettings()
            {
                ConnectionString= ""
            });
            _jwtHandler = new JWTHandler(_jwtSettings, new DaterHandler());
        }

        #region GenerateSignerToken

        [Fact]
        public void GenerateSignerToken_Suc()
        {           

            string signerId = "B6030A77-2A29-448E-E2C4-08D99D306589";
            string signerId2 = "FEA104A5-D053-4CB8-E2C3-08D99D306589";
            Signer signer = new Signer
            {
                Id = new Guid(signerId)
            };
            string updateJwt = _jwtHandler.GenerateSignerToken(signer, 720);
            string scriptUpdate = $"UPDATE SignerTokensMapping SET JwtToken = '{updateJwt}' WHERE SignerId = '{signerId}'; \n";
            Signer signer2 = new Signer
            {
                Id = new Guid(signerId2)
            };
            string updateJwt2 = _jwtHandler.GenerateSignerToken(signer2, 720);
            _ = $"UPDATE SignerTokensMapping SET JwtToken = '{updateJwt2}' WHERE SignerId = '{signerId2}'; ";



            Console.WriteLine(updateJwt);
        }

        #endregion
    }
}
