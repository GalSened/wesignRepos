using Common.Enums;
using Common.Handlers.PDF;
using Common.Interfaces.PDF;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PdfHandler.Interfaces;
using Serilog;
using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace PdfHandler
{
    public class PdfConverter : IPdfConverter
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;
        private readonly GeneralSettings _generalSettings;
        private readonly FolderSettings _folderSettings;
        private readonly IExternalPDFConverter _externalConvertor;
        static readonly object _locker = new object();
        static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(2);
        public PdfConverter(ILogger logger, IFileSystem fileSystem, IOptions<GeneralSettings> generalSettings, IOptions<FolderSettings> folderSettings,
                IExternalPDFConverter externalConvertor)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _generalSettings = generalSettings.Value;
            _folderSettings = folderSettings.Value;
            _externalConvertor = externalConvertor;
        }



        public string Convert(string Base64Input, string dataInput)
        {

            if (_generalSettings.UseExternalConvertor)
            {
                
                FileType fileType = FileType.DOCX;
                if ((dataInput ?? "").ToLower().Contains("application/msword"))
                {
                    
                    fileType = FileType.DOC;
                    if(Base64Input.Substring(0,6).ToLower() == "e1xydg")
                    {
                        
                        fileType = FileType.RTF;
                    }
                }
                if ((dataInput ?? "").ToLower().Contains("application/rtf"))
                {
                    fileType = FileType.RTF;
                }
                if((dataInput ?? "").ToLower().Contains("application/vnd.openxmlformats") && !(dataInput ?? "").ToLower().Contains("wordprocessingml.document"))
                    
                {
                    fileType = FileType.XLSX;

                }
                if((dataInput ?? "").ToLower() == "application/vnd.ms-excel")
                {
                    fileType = FileType.XLS;

                }
                    string resultData = null;
                try
                {
                    _semaphoreSlim.Wait();
                    resultData = _externalConvertor.ConvertToPDF(Base64Input, fileType);
                }
                finally { 
                    _semaphoreSlim.Release();
                }
                if(!string.IsNullOrWhiteSpace(resultData))
                {
                    
                    return resultData;
                }
                _logger.Warning("Failed to convert using external converter try to convert using Libre");

            }


            Guid guid = Guid.NewGuid();
            _logger.Information("convert called. input length: {InputLength}. id: {Id}, temp folder: {FolderPath}", Base64Input.Length, guid, _folderSettings.Temp);
            _fileSystem.Directory.CreateDirectory(_folderSettings.Temp);
            string fileExtension = ".docx";
            if((dataInput?? "").ToLower().Contains("application/msword"))
            {
                fileExtension = ".doc";
            }
            string tempInputFile = _fileSystem.Path.Combine(_folderSettings.Temp, guid.ToString() + fileExtension);
            string tempOutputFile = _folderSettings.Temp;
            string result = "";
            
            _fileSystem.File.WriteAllBytes(tempInputFile, System.Convert.FromBase64String(Base64Input));
            try
            {
                lock (_locker)
                {
                    using (Process process = Process.Start(_generalSettings.LibreOfficePath, $"--convert-to pdf --outdir \"{tempOutputFile}\" \"{tempInputFile}\""))
                    {
                        _logger.Information("start libreoffice process id {ProcessId}", process.Id);
                        int count = 3;
                        while (!process.HasExited && count > 0)
                        {
                            process.WaitForExit(1000 * 5);
                            count--;
                        }
                        tempOutputFile = _fileSystem.Path.Combine(tempOutputFile, $"{guid}.pdf");
                        try
                        {
                            result = System.Convert.ToBase64String(_fileSystem.File.ReadAllBytes(tempOutputFile));
                            process.Kill();
                        }
                        catch
                        {
                            Process[] ps = Process.GetProcessesByName("soffice.bin");
                            foreach (var p in ps)
                            {
                                _logger.Information("found libreoffice process id {ProcessId} running  try to kill", p.Id);
                                p.Kill();

                            }
                            throw;
                        }

                    }
                }
            }
            finally
            {
                DeleteTempFiles(tempInputFile, tempOutputFile);
            }
            _logger.Information("convert succeeded. id: {Id}", guid);

            return result;

        }

        private void DeleteTempFiles(string tempInputFile, string tempOutputFile)
        {
            if (_fileSystem.File.Exists(tempInputFile))
            {
                _fileSystem.File.Delete(tempInputFile);
            }
            if (_fileSystem.File.Exists(tempOutputFile))
            {
                _fileSystem.File.Delete(tempOutputFile);
            }
        }
    }
}
