
using Comda.Authentication.Models;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using PdfExternalService.Interfaces;
using PdfExternalService.Models;
using PdfExternalService.Models.DTO;
using Serilog;
using System.IO.Abstractions;
using System.Reflection.Metadata;
using System.Text;

namespace PdfExternalService.Handlers
{
    public class PdfOperationsHandler : IPdfOperations
    {
        private readonly IDebenuPdfLibrary _debenu;
        private readonly PDFExternalGeneralSettings _generalSettings;
        private readonly Serilog.ILogger _log;
        private readonly IFileSystem _fileSystem;
        private readonly IEncryptor _encryptor;
        

        public PdfOperationsHandler(IDebenuPdfLibrary debenu, IOptions<PDFExternalGeneralSettings> generalSettings,
            Serilog.ILogger log, IFileSystem fileSystem, 
             IEncryptor encryptor)
        {
            _debenu = debenu;
            _generalSettings = generalSettings.Value;
            _log = log;
            _fileSystem = fileSystem;
            _encryptor = encryptor;
        
        }

        public byte[] MergeFiles(FileMergeObject filesForMerge)
        {
            if(_encryptor.Decrypt(_generalSettings.UserAppKey) != filesForMerge.APIKey)
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }
            byte[] result;
            string operationId = filesForMerge.OperationId.ToString();
            string tempFolder = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), operationId);

            try
            {
                int fileIndex = 0;
                _fileSystem.Directory.CreateDirectory(tempFolder);
                foreach (string file in filesForMerge.Base64Files)
                {
                    if(_debenu.LoadFromString(Convert.FromBase64String(file), string.Empty) == 0)
                    {
                        throw new InvalidOperationException(ResultCode.InvalidFileContent.GetNumericString());
                    }
                    Clear(true);
                    byte[] pdf = _debenu.AppendToString(0);

                    var currentFilePath = _fileSystem.Path.Combine(tempFolder, $"{fileIndex}.pdf");
                    _fileSystem.File.WriteAllBytes(currentFilePath, pdf);

                    _debenu.AddToFileList(operationId, currentFilePath);
                    
                    ++fileIndex;
                }
                string outputhPath = _fileSystem.Path.Combine(tempFolder, $"{operationId}.pdf");
                _debenu.MergeFileList(operationId, outputhPath);
                result = _fileSystem.File.ReadAllBytes(outputhPath); 

            }
            finally
            {
                if (_fileSystem.Directory.Exists(tempFolder))
                {
                    _fileSystem.Directory.Delete(tempFolder, true);
                }
            }
            return result;
        }


        private void Clear(bool onlySigned = false)
        {

            int signtureCountFields = GetSigCount();
            int signAfterFlat = 0;
            bool doFlat = signtureCountFields > 0;
            while (doFlat)
            {
                DoFlatSig(onlySigned);
                signAfterFlat = GetSigCount();
                if (signAfterFlat == 0)
                {
                    doFlat = false;
                }
                else
                {
                    if(signAfterFlat == signtureCountFields)
                    {
                        doFlat = false;
                    }
                    else
                    {
                        signtureCountFields = signAfterFlat;

                    }
                }
                    
            }
            
        }

        private void DoFlatSig(bool onlySigned)
        {
            var count = _debenu.FormFieldCount();
            for (int i = 1; i <= _debenu.FormFieldCount(); i++)
            {
                if (_debenu.GetFormFieldType(i) == 6)// signature
                {
                    if (onlySigned)
                    {
                        var a = _debenu.GetFormFieldValue(i);
                        if (a != "")
                        {
                            int flat = _debenu.FlattenFormField(i);
                        }

                    }
                    else
                    {
                        _debenu.DeleteFormField(i);
                    }
                }
            }
        }

        private int GetSigCount()
        {
            int signatureCount = 0;
            var count = _debenu.FormFieldCount();
            for (int i = 1; i <= _debenu.FormFieldCount(); i++)
            {
                if (_debenu.GetFormFieldType(i) == 6)// signature
                {
                    signatureCount++;
                }
            }
            return signatureCount;
        }
    }
}
