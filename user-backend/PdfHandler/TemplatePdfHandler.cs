namespace PdfHandler
{
    using Common.Consts;
    using Common.Enums;
    using Common.Enums.Results;
    using Common.Extensions;
    using Common.Handlers.Files;
    using Common.Interfaces;
    using Common.Interfaces.Files;
    using Common.Interfaces.PDF;
    using Common.Models;
    using Common.Models.Files.PDF;
    using Common.Models.Settings;
    using Interfaces;

    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
  
    using PdfHandler.pdf;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;

    public class TemplatePdfHandler : Pdf, ITemplatePdf
    {

        
        private readonly IFilesWrapper _fileWrapper;


        public override IList<PdfImage> Images { get => GetImages(); }

        public TemplatePdfHandler(
            IOptions<GeneralSettings> generalSettings,
            ILogger logger, IDebenuPdfLibrary debenu,
             IServiceScopeFactory scopeFactory, IMemoryCache memoryCache, IFilesWrapper fileWrapper, IFileSystem fileSystem)
            : base(generalSettings, debenu,  logger, scopeFactory, memoryCache, fileWrapper, fileSystem)
        {          
            _fileWrapper = fileWrapper;
        }

        public override DocumentType GetDocumentType()
        {
            return DocumentType.Template;
        }

        public override bool Load(Guid id, bool shouldLoadToMemory = true)
        {
            try
            {                
                if (_fileWrapper.Documents.IsDocumentExist(GetDocumentType(), id))
                {
                    _id = id;
                    return IsLoaded(_fileWrapper.Documents.ReadDocument(GetDocumentType(), id), id, shouldLoadToMemory);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load template {TemplateId}", _id);
            }
            return false;
        }


        public void SetId(Guid id)
        {
            _id = id;
         
        }
        public Guid GetId()
        {
            return _id;
        }

        public (string HTML ,string JS) GetHtmlTemplate()
        {
            return _fileWrapper.Documents.ReadDocumentHTMLJs(GetDocumentType(), _id);           
        }

        //TODO change function name to be Save ?!?
        public override bool SaveDocument()
        {
            try
            {
                byte[] bytes = Save();
                _fileWrapper.Documents.SaveDocument(GetDocumentType(), _id, bytes);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save template {TemplateId}" ,_id);
                return false;
            }
        }

        public bool Create(Guid id, string base64file)
        {
            return Create(id, Convert.FromBase64String(base64file), false);
        }

        public bool Create(Guid id, byte[] file, bool IsDuplicate,string directory = "")
        { 
            try
            {
                if(!IsDuplicate)
                {
                    file = DocumentPagesRotation(file);
                }
                
                _fileWrapper.Documents.SaveDocument(GetDocumentType(), id, file);               
                if(!_fileWrapper.Documents.IsDocumentsImagesWasCreated(GetDocumentType(),id))
                {
                    _fileWrapper.Documents.CreateImagesFromData(GetDocumentType(),id,file);
                }


                if (!Load(id))
                {
                    throw new Exception("Failed to load the template " + _id);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    _fileWrapper.Documents.DeleteAllDocumentData(GetDocumentType(), id);
                    
                }
                catch (Exception execption)
                {
                    _logger.Error(execption, execption.Message);
                }
                _logger.Error(ex, "Failed to create template {TemplateId}", _id);
                return false;
            }
        }

        public bool Duplicate(Guid newId)
        {
            try
            {
                var creationResult = _fileWrapper.Documents.Duplicate(GetDocumentType(), newId, _id);                
                return creationResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to duplicate template {TemplateId}", _id);
                return false;
            }
        }

        public bool Delete()
        {
            try
            {
               _fileWrapper.Documents.DeleteAllDocumentData(GetDocumentType(), _id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete template {TemplateId}", _id);
                return false;
            }
        }

        public byte[] Download()
        {
           
            if (_fileWrapper.Documents.IsDocumentExist(GetDocumentType(),_id) )
            {
                return _fileWrapper.Documents.ReadDocument(GetDocumentType(),_id);
            }
            throw new Exception($"Failed to download template [{_id}]");
        }

        private IList<Common.Models.Files.PDF.PdfImage> GetImages()
        {

            if (!_fileWrapper.Documents.IsDocumentExist(GetDocumentType(), _id))
            {
                throw new FileNotFoundException($"{GetDocumentType()} {_id} not found");
            }

            var images = base.Images;

            return images;
        }

        #region Private


        public string ConvertImageToPdf(string input)
        {
            return base.ConvertImagesToPdf(input);
        }

        public Common.Models.Files.PDF.PdfImage GetPdfImageByIndex(int page, Guid id)
        {
            int pagesCount = GetPagesCount();
            if(page < 1 || page > pagesCount)
            {
                throw new InvalidOperationException(ResultCode.InvalidPageNumber.GetNumericString());
            }

            
            return _fileWrapper.Documents.ReadImagesOfDocumentInRange(GetDocumentType(), id, page, page + 1).FirstOrDefault();
        }

        public IList<Common.Models.Files.PDF.PdfImage> GetPdfImages(int startPage, int endPage, Guid id)
        {
            var pdfImages = new List<Common.Models.Files.PDF.PdfImage>();
            var picturesPathsList = _fileWrapper.Documents.ReadImagesOfDocumentInRange(GetDocumentType(), id, startPage, endPage);
            if (!picturesPathsList.Any())
            {
                CreateImagesFromPdfInFileSystem();
                picturesPathsList = _fileWrapper.Documents.ReadImagesOfDocumentInRange(GetDocumentType(), id, startPage, endPage);
            }


            return picturesPathsList;
        }

        public override void EmbadTextDataFields(List<Common.Models.Files.PDF.TextField> textFields, List<ChoiceField> choiceFields)
        {
            var data = _fileWrapper.Documents.ReadDocument(GetDocumentType(), _id);
            EmbadTextData(textFields, choiceFields, data, false);
        }


        public bool AddTextFields(List<TextField> textFields)
        {
            var data = _fileWrapper.Documents.ReadDocument(GetDocumentType(), _id);
            return AddTextFields(data, textFields);
            
        }






        #endregion
    }
}
