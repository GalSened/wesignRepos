using Common.Enums.PDF;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.Documents.Signers;
using CTInterfaces;
using PdfHandler.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Net.Http;
using System.Text;

namespace PdfHandler.Signing
{
    public class SigningTypeHandler : ISigningTypeHandler
    {
          
        private readonly IDataUriScheme _dataUriScheme;
        protected readonly ILogger _logger;
        private readonly ISign _sign;
        private readonly IImage _image;
        private readonly IPDFSign _pdfSign;
        private readonly ISignConnector _signConnector;
        private readonly IEncryptor _encryptor;
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IHttpClientFactory _clientFactory;

        public SigningTypeHandler(IDataUriScheme dataUriScheme, ILogger logger, IPDFSign pdfSign,
            IImage image, ISign sign, ISignConnector signConnector, IHttpClientFactory clientFactory, IConfigurationConnector configurationConnector ,IEncryptor encryptor)
        {
            
            _dataUriScheme = dataUriScheme;
            _logger = logger;
            _sign = sign;
            _image = image;
            _pdfSign = pdfSign;
            _signConnector = signConnector;
            _encryptor = encryptor;
            _configurationConnector = configurationConnector;
            _clientFactory = clientFactory;
        }

        public ISigning ExecuteCreation(SignatureFieldType signatureType)
        {
            if(signatureType == SignatureFieldType.Graphic)
            {
                return new GraphicSigningHandler( _logger, _pdfSign, _image, _sign,_dataUriScheme, _clientFactory, _configurationConnector, _encryptor);
            }
            if (signatureType == SignatureFieldType.Server)
            {
                return new ServerSigningHandler( _dataUriScheme, _signConnector, _encryptor);
            }
            return new GraphicSigningHandler( _logger, _pdfSign, _image, _sign, _dataUriScheme,  _clientFactory, _configurationConnector, _encryptor);
        }
    }
}
