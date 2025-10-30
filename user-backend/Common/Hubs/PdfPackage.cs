using Common.Interfaces.PDF;
using CTInterfaces;
using CTPdfSigner;
using Serilog;
using System;

namespace Common.Hubs
{
    public class PdfPackage: IPdfPackage
    {
        private readonly ILogger _logger;

        public PdfPackage(ILogger logger)
        {
            _logger = logger;
        }

        public PrepResponse PrepareSignatureForField(string[] fieldsNames, byte[] image, byte[] pdf)
        {
            _logger.Debug("PdfPackage - PrepareSignatureForField - Prepare signature request...");
            PrepResponse response = new PrepResponse();
            try
            {
               PDFSigner pdfSigner = new PDFSigner();
                CERT_DATA cert_data = new CERT_DATA
                {
                    SignCert = null,
                    CertChain = null
                };

                PDF_VIS_SIG_APPEARANCE appearance = new PDF_VIS_SIG_APPEARANCE();
                CTPdfSigner.PDFImage img = new CTPdfSigner.PDFImage();
                img.SetImage(image);
                appearance.Image = img;

                PDF_SIG_METADATA metadata = new PDF_SIG_METADATA
                {
                    Certification = PDF_CERTIFICATION_LEVEL.NOT_CERTIFIED,
                    Contact = "ComSignTrust",
                    Location = "Israel",
                    Reason = ""
                };

                PDF_SIG_CONTEXT sigContext = new PDF_SIG_CONTEXT
                {
                    FieldGeneration = PDF_FIELD_USE.USE_EXISTING_FIELD,
                    FieldNames = fieldsNames,
                    PDF = pdf
                };

                response.Result = pdfSigner.PrepareSignature(CTDigestAlg.SHA256, 
                cert_data, appearance, metadata, sigContext);
                _logger.Debug("PdfPackage - PrepareSignatureForField - Result: {@result} ", response.Result);

                if (response.Result == CTResult.SUCCESS)
                {
                    response.PDF = sigContext.PDF;
                    response.Hash = sigContext.Hash;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "PdfPackage - PrepareSignatureForField - ");
                response.Result = CTResult.GENERAL_ERROR;
            }
            return response;
        }
        public SetResponse SetExternalSignature(byte[] pdf, byte[] signedHash)
        {
            _logger.Debug("PdfPackage - SetSignature - SetSignature request.");
            SetResponse setResponse = new SetResponse();
            try
            {
                CTHashSignerExternal.CSignExternal cSignExternal = new CTHashSignerExternal.CSignExternal();
                PDF_SIG_CONTEXT sigContext = new PDF_SIG_CONTEXT
                {
                    Hash = signedHash,
                    PDF = pdf
                };
                //setResponse.Result = cSignExternal.SetSignature(sigContext);
            }
            catch (Exception e)
            {
                setResponse.Result = CTResult.GENERAL_ERROR;
                _logger.Error(e, "PdfPackage - SetSignature - ");
            }
            return setResponse;
        }

            public SetResponse SetSignature(byte[] pdf, byte[] signedHash)
        {
            _logger.Debug("PdfPackage - SetSignature - SetSignature request.");
            SetResponse setResponse = new SetResponse();
            try
            {
                CTPdfSigner.PDFSigner pdfSigner = new CTPdfSigner.PDFSigner();
                PDF_SIG_CONTEXT sigContext = new PDF_SIG_CONTEXT
                {
                    Hash = signedHash,
                    PDF = pdf
                };
                setResponse.Result = pdfSigner.SetSignature(sigContext);
                _logger.Debug("PdfPackage - SetSignature - Result: {result} ",setResponse.Result);
                if (setResponse.Result == CTResult.SUCCESS)
                    setResponse.PDF = pdf;
            }
            catch (Exception e)
            {
                setResponse.Result = CTResult.GENERAL_ERROR;
                _logger.Error(e, "PdfPackage - SetSignature - ");
            }
            return setResponse;
        }
    }

    public class PrepResponse
    {
        public byte[] PDF;

        public byte[] Hash;

        public CTResult Result;
    }

    public class SetResponse
    {
        public byte[] PDF;

        public CTResult Result;
    }
}
