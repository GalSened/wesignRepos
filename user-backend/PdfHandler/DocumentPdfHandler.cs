// Ignore Spelling: debenu

namespace PdfHandler
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Enums.PDF;
    using Common.Handlers.Files;
    using Common.Interfaces;
    using Common.Interfaces.Files;
    using Common.Interfaces.PDF;
    using Common.Models.Configurations;
    using Common.Models.Documents.Signers;
    using Common.Models.Files.PDF;
    using Common.Models.Settings;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using PdfHandler.Interfaces;
    using PdfHandler.pdf;
    using PdfHandler.Signing;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;

    public class DocumentPdfHandler : Pdf, IDocumentPdf
    { 
        private readonly ISigningTypeHandler _signingTypeHandler;
        private readonly IFilesWrapper _fileWrapper;
       

        public override IList<PdfImage> Images { get => GetImages(); }

        public DocumentPdfHandler(
            IOptions<GeneralSettings> generalSettings,
            ILogger logger, IDebenuPdfLibrary debenu,  ISigningTypeHandler signingTypeHandler,
             IServiceScopeFactory scopeFactory, IMemoryCache memoryCache,
             IFilesWrapper fileWrapper, IFileSystem fileSystem)
            : base(generalSettings, debenu,  logger, scopeFactory, memoryCache, fileWrapper, fileSystem)
        {                        
            _fileWrapper = fileWrapper;            
            _signingTypeHandler = signingTypeHandler;
        }

        public void SetId(Guid id)
        {
            _id = id;
           
        }
        public Guid GetId()
        {
            return _id;
        }

        public override DocumentType GetDocumentType()
        {
            return DocumentType.Document;
        }
        public override bool Load(Guid id, bool includeMemoryLoading = true)
        {
            _logger.Debug("Enter Load function for id = [{Id}]", id);
            try
            {
                
           
                if (_fileWrapper.Documents.IsDocumentExist(DocumentType.Document, id))
                {
                    this._id = id;
                    return IsLoaded(_fileWrapper.Documents.ReadDocument(DocumentType.Document, id)  , id, includeMemoryLoading);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load document {Id}", _id);
            }
            return false;
        }



        public override void EmbadTextDataFields(List<Common.Models.Files.PDF.TextField> textFields, List<ChoiceField> choiceFields)
        {
            var data = _fileWrapper.Documents.ReadDocument(GetDocumentType(), _id);
            EmbadTextData(textFields, choiceFields, data, true);
        }

        public new byte[] CreateTraceFile(DocumentCollectionAuditTrace documentCollectionAuditTrace, DocumentMode mode)
        {
            var traceBytes = base.CreateTraceFile(documentCollectionAuditTrace, mode);

            return traceBytes;
        }
        public new byte[] CreateTraceFile(List<string> result)
        {
            var traceBytes = base.CreateTraceFile(result);

            return traceBytes;
        }

   
        public bool Create(Guid id, byte[] file, bool formExistTemplate, bool shouldLoadToMemory = true)
        {
            try
            {
                if (!formExistTemplate)
                {
                    file = DocumentPagesRotation(file);
                }
                _fileWrapper.Documents.SaveDocument(GetDocumentType(), id, file);
                this._id = id;
                return IsLoaded(file,id, shouldLoadToMemory);
                
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create document {Id}", _id);
                return false;
            }
        }


        public bool CreateImagesFromExternalList(IList<PdfImage> images)
        {

            bool result = false;
            try
            {
              
                if(_fileWrapper.Documents.IsDocumentsImagesWasCreated(DocumentType.Document, _id))
                {
                    // case images was created before.
                    result = true;
                }
                else
                {
                    _fileWrapper.Documents.CreateImagesFromList(DocumentType.Document, _id, images.ToList());
                    result = true;
                }
                
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save PDF images from external list {Id}", _id);
                result =  false;
            }
            return result;
        }
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
                _logger.Error(ex, "Failed to save document {Id}", _id);
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
                _logger.Error(ex, "Failed to delete document {Id}", _id);
                return false;
            }
        }

      


        public bool IsExists(IBaseField field)
        {
            if (field is TextField)
            {
                return TextFields.FirstOrDefault(x => x.Name == ((TextField)field).Name) != null;
            }
            if (field is ChoiceField)
            {
                return ChoiceFields.FirstOrDefault(x => x.Name == ((ChoiceField)field).Name) != null;
            }
            if (field is CheckBoxField)
            {
                return CheckBoxFields.FirstOrDefault(x => x.Name == ((CheckBoxField)field).Name) != null;
            }
            if (field is RadioGroupField)
            {
                return RadioGroupFields.FirstOrDefault(x => x.Name == ((RadioGroupField)field).Name) != null;
            }
            if (field is SignatureField)
            {
                return SignatureFields.FirstOrDefault(x => x.Name == ((SignatureField)field).Name) != null;
            }
            return false;
        }

        public async Task Sign(SigningInfo signingInfo, bool isServerWithoutFields = false, bool useForAllFields = false)
        {
            SignatureFieldType signatureType = isServerWithoutFields ? SignatureFieldType.Server :
                signingInfo.Signatures.Any(x => x.SigningType == SignatureFieldType.Server) ? SignatureFieldType.Server :
                signingInfo.Signatures.Any(x => x.SigningType == SignatureFieldType.SmartCard) ? SignatureFieldType.SmartCard :
                SignatureFieldType.Graphic;

            var signingTypeHadler = _signingTypeHandler.ExecuteCreation(signatureType);

            signingInfo.Data = _fileWrapper.Documents.ReadDocument(GetDocumentType(), _id);

            var signData =await signingTypeHadler.Sign(signingInfo, useForAllFields);

            _fileWrapper.Documents.SaveDocument(GetDocumentType(), _id, signData);

        }

        public  Task VerifySigner1Credential(SignerAuthentication input, CompanySigner1Details companySigner1Details = null)
        {
            companySigner1Details = companySigner1Details != null ? companySigner1Details : new CompanySigner1Details();
            var signingTypeHadler = _signingTypeHandler.ExecuteCreation(SignatureFieldType.Server);
            return signingTypeHadler.VerifyCredential(new SigningInfo 
            { 
                SignerAuthentication = input,
                CompanySigner1Details = companySigner1Details

            });
        }


        #region Private


        public void SaveCopy(Guid id)
        {
           

            if(_generalSettings.SaveSignerDocTrace)
            {
                _fileWrapper.Documents.SaveDocumentCopy(GetDocumentType(), _id);
               
            }

        }

        private IList<PdfImage> GetImages()
        {
            
            if (!_fileWrapper.Documents.IsDocumentExist(GetDocumentType(), _id))
            {
                throw new FileNotFoundException($"{GetDocumentType()} {_id} not found");
            }
            var images = base.Images;
            

            return images;
        }

      
        public PdfImage GetPdfImageByIndex(int page, Guid id)
        {

            return _fileWrapper.Documents.ReadImagesOfDocumentInRange(GetDocumentType(), id, page, page + 1).FirstOrDefault();
            
            
        }

        public IList<PdfImage> GetPdfImages(int startPage, int endPage, Guid id)
        {         
            var pdfImages = _fileWrapper.Documents.ReadImagesOfDocumentInRange (GetDocumentType(), id , startPage, endPage);
                
            if(!pdfImages.Any())
            {
                CreateImagesFromPdfInFileSystem();
                pdfImages = _fileWrapper.Documents.ReadImagesOfDocumentInRange(GetDocumentType(), id, startPage, endPage);
            }
            
            return pdfImages;
        }
        public void CreateFromExistingTemplate(Guid sourceTemplateId)
        {
            _fileWrapper.Documents.CopyDocumentDataFromSource(GetDocumentType(), _id, DocumentType.Template, sourceTemplateId, false);
        }
        public void CopyPagesImagesFromTemplate(Guid origTemplateId)
        {
            _fileWrapper.Documents.CopyDocumentDataFromSource(GetDocumentType(), _id,DocumentType.Template, origTemplateId, true);

        }

        public void CreateImages()
        {
            _fileWrapper.Documents.CreateImagesFromData(GetDocumentType(), _id, Save());
        }





        #endregion
    }


}
