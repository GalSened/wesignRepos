using Cloudmersive.APIClient.NETCore.DocumentAndDataConvert.Api;
using Cloudmersive.APIClient.NETCore.DocumentAndDataConvert.Client;
using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.PDF;
using Common.Models.Settings;

using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO;


namespace Common.Handlers.PDF
{
    public class ExternalCloudmersivePDFConverterHandler : IExternalPDFConverter
    {
        private ILogger _logger;
        private GeneralSettings _generalSettings;
        private IEncryptor _encryptor;

        public ExternalCloudmersivePDFConverterHandler(ILogger logger,  IOptions<GeneralSettings> generalSettings,
            IEncryptor encryptor)
        {
            _logger = logger;
            _generalSettings = generalSettings.Value;
            _encryptor = encryptor;
        }
        
        public string ConvertToPDF(string data, FileType fileType)
        {
            try
            {
                Configuration.Default.AddApiKey("Apikey", _encryptor.Decrypt(_generalSettings.ExternalConvertorKey1));
                ConvertDocumentApi convertDocumentApi = new ConvertDocumentApi();                
                using (var stream = new MemoryStream(Convert.FromBase64String(data)))
                {
                    switch (fileType)
                    {
                        case FileType.DOCX:
                            {
                                
                                var convertedPDF = convertDocumentApi.ConvertDocumentDocxToPdf(stream);
                                return Convert.ToBase64String(convertedPDF);
                            }
                        case FileType.DOC:
                            {
                                
                                var convertedPDF = convertDocumentApi.ConvertDocumentDocToPdf(stream);
                                return Convert.ToBase64String(convertedPDF);
                            }
                        case FileType.XLSX:
                            {
                                var convertedPDF = convertDocumentApi.ConvertDocumentXlsxToPdf(stream);
                                return Convert.ToBase64String(convertedPDF);
                            }
                        case FileType.XLS:
                            {
                                var convertedPDF = convertDocumentApi.ConvertDocumentXlsToPdf(stream);
                                return Convert.ToBase64String(convertedPDF);
                            }
                        case FileType.CSV:
                            {
                                var convertedPDF = convertDocumentApi.ConvertDocumentCsvToPdf(stream);
                                return Convert.ToBase64String(convertedPDF);
                            }
                        case FileType.HTML:
                            {
                                var convertedPDF = convertDocumentApi.ConvertDocumentHtmlToPdf(stream);
                                return Convert.ToBase64String(convertedPDF);
                            }
                        case FileType.RTF:
                            {
                                
                                var convertedPDF = convertDocumentApi.ConvertDocumentRtfToPdf(stream);
                                
                                return Convert.ToBase64String(convertedPDF);
                            }
                        default:
                            {
                                return null;
                            }
                    }
                }

            }
            catch (Exception ex) {
                _logger.Error("Fail to convert using external resource {ExceptionMessage}", ex.Message);
                if (ex.Message.Contains("file contains virus or malware."))
                {
                    throw ;
                }
                
                return null;
            }


        }
    }
}
