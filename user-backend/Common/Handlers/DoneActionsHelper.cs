using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Common.Handlers
{
    public class DoneActionsHelper : IDoneActionsHelper
    {
        private readonly ILogger _logger;
        private readonly IDocumentPdf _documentPdf;
        
        private readonly IEncryptor _encryptor;
        private readonly IConfiguration _configuration;
        private ICompanyConnector _companyConnector;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;

        public DoneActionsHelper(ICompanyConnector companyConnector, IDocumentCollectionConnector documentCollectionConnector, 
            ILogger logger, IDocumentPdf documentPdf, IEncryptor encryptor, IConfiguration configuration)
        {

            _companyConnector = companyConnector;
            _documentCollectionConnector = documentCollectionConnector;
            _logger = logger;
            _documentPdf = documentPdf;
            
            _encryptor = encryptor;
            _configuration = configuration;
        }

        public async Task<ResultCode> HandlerSigningUsingSigner1AfterDocumentSigningFlow(DocumentCollection dbDcumentCollection, CompanySigner1Details companySigner1Details = null)
        {
            ResultCode result = ResultCode.GeneralErrorMessage;
            if (dbDcumentCollection.ShouldSignUsingSigner1AfterDocumentSigningFlow && dbDcumentCollection.DocumentStatus == DocumentStatus.Signed)
            {
                var senderCompany =await _companyConnector.Read(new Company { Id = dbDcumentCollection.User.CompanyId });
                var signingInfo = new SigningInfo
                {
                    SignerAuthentication = new SignerAuthentication
                    {
                        Signer1Credential = new Signer1Credential
                        {
                            CertificateId = _encryptor.Decrypt(senderCompany.CompanySigner1Details.CertId),
                            Password = _encryptor.Decrypt(senderCompany.CompanySigner1Details.CertPassword)
                        },
                        Signer1Configuration =await _configuration.GetSigner1Configuration(senderCompany.CompanySigner1Details)
                    },
                    CompanySigner1Details = senderCompany.CompanySigner1Details != null ? senderCompany.CompanySigner1Details : new CompanySigner1Details(),
                };



                foreach (var doc in dbDcumentCollection.Documents)
                {
                    _documentPdf.Load(doc.Id);
                    try
                    {
                        await _documentPdf.Sign(signingInfo, isServerWithoutFields: true);
                        await _documentCollectionConnector.UpdateStatus(dbDcumentCollection, DocumentStatus.ExtraServerSigned);
                        result = ResultCode.Success;
                    }
                    catch (InvalidOperationException ex)
                    {
                        ResultCode resultCode = (ResultCode)int.Parse(ex.Message);
                        _logger.Error(ex, "Failed to sign using signer1 after document signing flow");
                        result = resultCode;
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to sign using signer1 after document signing flow");
                        result = ResultCode.GeneralErrorMessage;
                        break;
                    }
                    finally
                    {
                        if (result == ResultCode.Success)
                        {
                            await UpdateExtraServerSignedStatusInDB(dbDcumentCollection);
                        }
                    }
                }
            }
            return result;
        }

        private Task UpdateExtraServerSignedStatusInDB(DocumentCollection dbDcumentCollection)
        {
            dbDcumentCollection.DocumentStatus = DocumentStatus.ExtraServerSigned;
            return _documentCollectionConnector.Update(dbDcumentCollection);
        }
    }
}
