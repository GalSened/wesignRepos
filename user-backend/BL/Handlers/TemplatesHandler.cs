namespace BL.Handlers
{
    using Common.Enums;
    using Common.Enums.PDF;
    using Common.Enums.Results;
    using Common.Enums.Templates;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Interfaces.Files;
    using Common.Interfaces.PDF;
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Files.PDF;
    using Common.Models.Settings;
    using Common.Models.XMLModels;

    using LazyCache;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using PdfHandler.Enums;
    using PdfHandler.Interfaces;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    public class TemplatesHandler : ITemplates
    {
        private const string COPY_TEMPLATE_SUFFIX = " - Copy";

        private readonly ITemplatePdf _templatePdf;
        private readonly ITemplateConnector _templateConnector;
        private readonly IProgramConnector _programConnector;
        private readonly IDocumentConnector _documentConnector;
        private readonly IUserConnector _userConnector;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly IValidator _validator;
        private readonly ILogger _logger;

        private readonly IUsers _users;
        private readonly IDataUriScheme _dataUriScheme;
        private readonly IDater _dater;
        
        private readonly IXmlHandler<PDFMetaData> _xmlHandler;
        private readonly GeneralSettings _generalSetting;
        private readonly IContacts _contacts;
        private readonly IPdfConverter _pdfConverter;
        private readonly IMemoryCache _memoryCache;
        private readonly IExternalPDFService _externalPDFService;
        private readonly IFilesWrapper _filesWrapper;
        private readonly IOcrService _ocrService;

        public TemplatesHandler(ITemplateConnector templateConnector, IProgramConnector programConnector, IDocumentConnector documentConnector,
            IProgramUtilizationConnector programUtilizationConnector, ITemplatePdf templatePdf, ILogger logger, IUserConnector userConnector,
            IValidator validator, IOptions<GeneralSettings> generalSettings, IUsers users,
            IDataUriScheme dataUriScheme,
            IDater dater, IXmlHandler<PDFMetaData> xmlHandler, IContacts contacts, IPdfConverter pdfConverter
            , IMemoryCache memoryCache, IExternalPDFService externalPDFService, IFilesWrapper filesWrapper, IOcrService ocrService
        )
        {
            _templateConnector = templateConnector;
            _programConnector = programConnector;
            _documentConnector = documentConnector;
            _userConnector = userConnector;
            _programUtilizationConnector = programUtilizationConnector;
            _templatePdf = templatePdf;
            _logger = logger;
            _validator = validator;
            
            _generalSetting = generalSettings.Value;
            _users = users;
            _dataUriScheme = dataUriScheme;
            _dater = dater;
           
            _xmlHandler = xmlHandler;
            _contacts = contacts;
            _pdfConverter = pdfConverter;
            _memoryCache = memoryCache;
            _externalPDFService = externalPDFService;
            _filesWrapper = filesWrapper;
            _ocrService = ocrService;
        }


        public async Task<Template> MergeTemplates(MergeTemplates mergeTemplates)
        {
            (User user,  _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            PDFFields existingFields = new PDFFields();

            for(int i = 0; i< mergeTemplates.Templates.Count ; ++i)
            {

                if(mergeTemplates.Templates[i].Id != Guid.Empty)
                {
                    Template templateRecord = await ReadAndValidateTempalteGroup(mergeTemplates.Templates[i]);
                        
                    
                    existingFields.Merge(templateRecord.Fields);

                }              
                else 
                {

                    mergeTemplates.Templates[i].Base64File =(await _validator.ValidateIsCleanFile(mergeTemplates.Templates[i].Base64File))?.CleanFile;
                    if (_dataUriScheme.IsValidFileType(mergeTemplates.Templates[i].Base64File, out FileType fileType))
                    {
                        mergeTemplates.Templates[i].FileType = fileType;
                    }
                    else
                    {
                        throw new InvalidOperationException(ResultCode.InvalidFileType.GetNumericString());
                    }
                }
            }
            IEnumerable<Template> templates = _templateConnector.Read(user, mergeTemplates.Name, null, null, 0, 20, true, true, out _);
            if (templates.Any())
            {
                mergeTemplates.Name = CreateDuplicateTemplateName(mergeTemplates.Name);
            }
            Template template = new Template()
            {
                Name = mergeTemplates.Name,
                UserId = user.Id,
                GroupId = user.GroupId,
                CreationTime = _dater.UtcNow(),
                LastUpdatetime = _dater.UtcNow(),
                Status = mergeTemplates.IsOneTimeUseTemplate ? TemplateStatus.OneTimeUse : TemplateStatus.MultipleUse,
                Fields = existingFields
            };

            List<string> templatesContents= new List<string>();
            try
            {
                for (int i = 0; i < mergeTemplates.Templates.Count; ++i)
                {
                    if (mergeTemplates.Templates[i].Id != Guid.Empty)
                    {
                        _templatePdf.SetId(mergeTemplates.Templates[i].Id);
                        templatesContents.Add(Convert.ToBase64String(_templatePdf.Download()));
                        
                    }
                    else
                    {
                        templatesContents.Add(
                            mergeTemplates.Templates[i].FileType != FileType.PDF ?
                        ConvertToPdf(mergeTemplates.Templates[i].Base64File)
                        : _dataUriScheme.Getbase64Content(mergeTemplates.Templates[i].Base64File));
                    }
                }

                string base64content =  await _externalPDFService.Merge(templatesContents);
                await _templateConnector.Create(template);
                if (!_templatePdf.Create(template.Id, base64content))
                {
                    throw new Exception($"Failed to create template PDF for: Template - Name: {template.Name}, Id: [{template.Id}]");
                }

                _templatePdf.CreateImagesFromPdfInFileSystem();
                template.Images = new Common.Models.Files.PDF.PdfImage[_templatePdf.GetPagesCount()];

                PDFFields pdfFields = _templatePdf.GetAllFields(false);
                if (pdfFields.TotalFields() > 0)
                {
                    FillTemplateFieldsMissingData(template.Fields, pdfFields); // FIll missing data
                }
                if (template.Status == TemplateStatus.MultipleUse)
                {
                    await _programUtilizationConnector.UpdateTemplatesAmount(user, CalcOperation.Add);
                }

                return template;
               
            }
            catch
            {
                if (await _templateConnector.Exists(template))
                {
                    await _templateConnector.Delete(template);
                }
                throw;
            }
                
                

        }

        public async Task Create(Template template)
        {
            (User user,  _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            if (!await _programConnector.CanAddTemplate(user) && template.Status == TemplateStatus.MultipleUse)
            {
                throw new InvalidOperationException(ResultCode.ProgramUtilizationGetToMax.GetNumericString());
            }
            template.Base64File = (await _validator.ValidateIsCleanFile(template.Base64File))?.CleanFile;
            if (!string.IsNullOrWhiteSpace(template.MetaData))
            {
                template.MetaData = (await _validator.ValidateIsCleanFile(template.MetaData))?.CleanFile;
            }
            if (_dataUriScheme.IsValidFileType(template.Base64File, out FileType fileType))
            {
                template.FileType = fileType;
            }

            
            template.Name = _filesWrapper.Documents.GetFileNameWithoutExtension(template.Name); 
           IEnumerable<Template> templates = _templateConnector.Read(user, template.Name, null, null, 0, 20, true, true, out _);
            if (templates.Any())
            {
                template.Name = CreateDuplicateTemplateName(template.Name);
            }

            //db creation
            template.UserId = user.Id;
            template.GroupId = user.GroupId;
            template.CreationTime = _dater.UtcNow();
            template.LastUpdatetime = _dater.UtcNow();
            await _templateConnector.Create(template);
            try
            {
                string base64content = template.FileType != FileType.PDF ?
                    ConvertToPdf(template.Base64File)
                    : _dataUriScheme.Getbase64Content(template.Base64File);

                if (!_templatePdf.Create(template.Id, base64content))
                {
                    throw new Exception($"Failed to create template PDF for: Template - Name: {template.Name}, Id: [{template.Id}]");
                }
              
                _templatePdf.CreateImagesFromPdfInFileSystem();
                template.Images = new PdfImage[_templatePdf.GetPagesCount()];

                PDFFields pdfFields = _templatePdf.GetAllFields(false);
                if (pdfFields.TotalFields() > 0)
                {
                    FillTemplateFieldsMissingData(template.Fields, pdfFields); // FIll missing data
                }

                if (!string.IsNullOrEmpty(template.MetaData))
                {
                    _logger.Information("{UserEmail} trying to use meta-data", user.Email);
                    await HandlePdfMetaData(template);
                }
                if (template.Status == TemplateStatus.MultipleUse)
                {
                    await _programUtilizationConnector.UpdateTemplatesAmount(user, CalcOperation.Add);
                }
                _logger.Information("Successfully create template [{TemplateId}: {TemplateName}] by user [{UserId}: {UserName}]", template.Id, template.Name, template.UserId, template.UserName);
            }
            catch
            {
                await _templateConnector.Delete(template);
                throw;
            }
        }

        public async Task<bool> IsTemplateInUse(Template template)
        {
            Document document = await _documentConnector.Read(template);             
            return document != null;
        }
        public async Task DeleteBatch(RecordsBatch templatesBatch)
        {
            (User user,  _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);

            IEnumerable<Template> templates = _templateConnector.Read(templatesBatch.Ids);
            
           
            Dictionary<Guid, string> templateInfosForLogs = new Dictionary<Guid, string>();
            foreach (Template template in templates)
            {
                if (template?.GroupId != user.GroupId)
                {
                    throw new InvalidOperationException(ResultCode.TemplateNotBelongToUserGroup.GetNumericString());
                }
                if (!templateInfosForLogs.ContainsKey(template.Id))
                {
                    templateInfosForLogs.Add(template.Id, template.Name);
                }

            }
            _logger.Information("User {UserId}: {UserEmail} deleted template batch {TemplateInfosForLogs}", user.Id, user.Email, templateInfosForLogs );
            foreach (Template template in templates)
            {
                await _templateConnector.Delete(template);
            }
            await _programUtilizationConnector.UpdateTemplatesAmount(user, CalcOperation.Substruct, templates.Count());
        }

        public async Task Delete(Template template)
        {
            (User user, _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            Template dbTemplate = await ReadAndValidateTempalteGroup(template);
               
            _logger.Information("User {UserId}: {UserEmail} deleted template {TemplateId} : {TemplateName}", user.Id, user.Email, dbTemplate.Id, dbTemplate.Name);
            await _templateConnector.Delete(dbTemplate);
            await _programUtilizationConnector.UpdateTemplatesAmount(user, CalcOperation.Substruct);
        }

        public IEnumerable<Template> ReadDeletedTemplates()
        {
            return _templateConnector.ReadDeletedTemplates();
        }

        public async Task<Template> DuplicateTemplate(Template template)
        {
            (User user, _) = await _users.GetUser();
            await _validator.ValidateEditorUserPermissions(user);
            if (!await _programConnector.CanAddTemplate(user) && 
                template.Status == TemplateStatus.MultipleUse)
            {
                throw new InvalidOperationException(ResultCode.ProgramUtilizationGetToMax.GetNumericString());
            }
            Template dbTemplate = await ReadAndValidateTempalteGroup(template);
            
            _templatePdf.Load(template.Id);
          
            
            PDFFields pdfFields = _templatePdf.GetAllFields();
            FillTemplateFieldsMissingData(dbTemplate.Fields, pdfFields); // FIll missing data
            string tempalteName = dbTemplate.Name;
            if (template.Status != TemplateStatus.OneTimeUse)
            {
                tempalteName = CreateDuplicateTemplateName(dbTemplate.Name);
            }
            Template duplicateTemplate = new Template()
            {
                Name = tempalteName,
                CreationTime = _dater.UtcNow(),
                LastUpdatetime = _dater.UtcNow(),
                UserId = user.Id,
                GroupId = user.GroupId,
                Fields = pdfFields,
                Status = template.Status
            };
            await _templateConnector.Create(duplicateTemplate);

            try
            {
                _templatePdf.Duplicate(duplicateTemplate.Id);
                
                if(template.Status == TemplateStatus.MultipleUse)
                {
                    await _programUtilizationConnector.UpdateTemplatesAmount(user, CalcOperation.Add);
                }
                dbTemplate.LastUsedTime = _dater.UtcNow();
                await _templateConnector.UpdateLastUsed(dbTemplate);

                _logger.Information("Successfully duplicated template [{DbTemplateId} : {DbTemplateName}] - by user [{UserId} : {UserEmail}] the template name is {DuplicateTemplateName}", dbTemplate.Id, dbTemplate.Name, user.Id, user.Email, duplicateTemplate.Name);
                return duplicateTemplate;
            }
            catch
            {
                await _templateConnector.Delete(duplicateTemplate);

                throw;
            }
        }

        public async Task GetPageByTemplateId(Template template, int page)
        {

            Template dbTemplate = await ReadAndValidateTempalteGroup(template);

            _templatePdf.SetId(dbTemplate.Id);
            PdfImage pdfImage = _templatePdf.GetPdfImageByIndex(page, template.Id);
           
            template.Fields = _templatePdf.GetAllFields(page, page);
           

            template.Name = dbTemplate.Name;
            FillTemplateFieldsMissingData(dbTemplate.Fields, template.Fields);
            pdfImage.Base64Image = $"data:image/jpeg;base64,{pdfImage.Base64Image}";
            template.Images = new List<PdfImage>()
            {
                { pdfImage}
            };
         
        }

        public async Task<string> GetOcrHtmlFromImage(string base64Image)
        {
            return await _ocrService.GenerateOcrHtmlFromBase64ImageAsync(base64Image);
        }

        public async Task GetPagesByTemplateId(Template template, int offset, int limit)
        {
            (User user, _) = await _users.GetUser();
            Template dbTemplate = _memoryCache.Get<Template>(GetReadTemplateInfoFlow(template, user));

            if (dbTemplate == null)
            {
                dbTemplate =await ReadAndValidateTempalteGroup(template);
            }
            
            _templatePdf.SetId(template.Id);
            DocumentMemoryCache documentMemoryCache = _memoryCache.Get<DocumentMemoryCache>(template.Id);
            int pagesCount ;
           
            if (documentMemoryCache == null)
            {
                pagesCount = _templatePdf.GetPagesCount();
            }
            else
            {
                pagesCount = documentMemoryCache.PageCount;
            }
            int endPage = offset + limit >  pagesCount ? pagesCount + 1 : offset + limit;

            IList<PdfImage> pdfImages = _templatePdf.GetPdfImages(offset, endPage, template.Id);

            if (documentMemoryCache == null)
            {
                template.Fields = _templatePdf.GetAllFields(offset, endPage);
            }
            else
            {
                template.Fields = _templatePdf.GetAllFieldsInRange(offset, endPage, documentMemoryCache.pdfFields);
            }
           
            template.Name = dbTemplate.Name;
            FillTemplateFieldsMissingData(dbTemplate.Fields, template.Fields);

            foreach (PdfImage image in pdfImages)
            {
                image.Base64Image = $"data:image/jpeg;base64,{image.Base64Image}";
            }
            template.Images = pdfImages;
        }

        public async Task<Template> GetPagesCountByTemplateId(Template template)
        {
            (User user, _) = await _users.GetUser();
            Template dbTemplate = await ReadAndValidateTempalteGroup(template);

            await GetUserNameForTemplates(user, new[] { dbTemplate });

            _templatePdf.Load(dbTemplate.Id);

            DocumentMemoryCache documentMemoryCache = new DocumentMemoryCache
            {
                pdfFields = _templatePdf.GetAllFields(),
                PageCount = _templatePdf.GetPagesCount()
            };
            _memoryCache.Set(dbTemplate.Id, documentMemoryCache, TimeSpan.FromSeconds(20));

            
            dbTemplate.Images = new Common.Models.Files.PDF.PdfImage[documentMemoryCache.PageCount];
            _memoryCache.Set(GetReadTemplateInfoFlow(dbTemplate, user), dbTemplate, TimeSpan.FromSeconds(10));

           return dbTemplate;
        }

        public async Task<(IEnumerable<Template>,int)> Read(string key, string from, string to, int offset, int limit, bool popular, bool recent)
        {
            (User user, _) =await _users.GetUser();
            IEnumerable<Template> templates = _templateConnector.Read(user, key, from, to, offset, limit, popular, recent, out int totalCount);
            templates = templates.ToList();
            await GetUserNameForTemplates(user, templates);

            return (templates, totalCount);
        }
       
        public async Task<Template> GetTemplateByTemplateId(Template template)
        {
            Template resultTemplate = await ReadAndValidateTempalteGroup(template);
            
            _templatePdf.Load(template.Id);
            resultTemplate.Images = _templatePdf.Images;
            resultTemplate.Fields = _templatePdf.GetAllFields();
            return resultTemplate;

        }

        public async Task<(string name, byte[] content)> Download(Template template)
        {
            (User user, _) = await _users.GetUser();
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }

            Template dbTemplate = await ReadAndValidateTempalteGroup(template);
            
            if (!_filesWrapper.Documents.IsDocumentExist(DocumentType.Template, template.Id))
            {
                throw new Exception($"File type [{DocumentType.Template}] [{template.Id}]  not exist");
            }

            dbTemplate.LastUsedTime = _dater.UtcNow();
            await _templateConnector.UpdateLastUsed(dbTemplate);
            _logger.Debug("User {UserName} id {UserID} downloading template {DocumentName} id {DocumentId}",
                user.Name, user.Id, dbTemplate.Name, dbTemplate.Id);
            return (dbTemplate.Name, _filesWrapper.Documents.ReadDocument(DocumentType.Template,template.Id));
        }

        public async Task Update(Template template)
        {
            (User user, _) = await _users.GetUser();
            
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
            await ReadAndValidateTempalteGroup(template);

            _memoryCache.Remove(template.Id);
            UpdateFields(template);
            PDFFields pdfFields = _templatePdf.GetAllFields(includeSignatureImages: false);
            FillTemplateFieldsMissingData(template.Fields, pdfFields);
            template.GroupId = user.GroupId;
            template.LastUsedTime = _dater.UtcNow();
            await _templateConnector.Update(template);
            _logger.Information("Successfully update template [{TemplateId}: {TemplateName}] by user [{UserId}: {UserEmail}]", template.Id, template.Name, user.Id, user.Email);

        }


        #region private

        private  Task HandlePdfMetaData(Template template)
        {
            PDFMetaData pdfMetaData = _xmlHandler.ConvertBase64ToModel(template.MetaData);
            IList<FieldCoordinate> placeholders = _templatePdf.GetPlaceholdersFromPdf(pdfMetaData.Parenthesis, pdfMetaData.PlaceholderColor);
            bool skipValidation = !string.IsNullOrEmpty(pdfMetaData.SkipValidation) && bool.Parse(pdfMetaData.SkipValidation);
            if (!skipValidation && placeholders?.Count != pdfMetaData.Fields?.Field.Count)
            {
                throw new InvalidOperationException(ResultCode.InvalidXMLMisMatchPlaceolders.GetNumericString());
            }
            PDFFields metaDataFields = pdfMetaData.ToPdfFields(placeholders, skipValidation);
            template.Fields.Merge(metaDataFields);
            UpdateFields(template);
            return _templateConnector.Update(template);
        }

        private void UpdateFields(Template template)
        {
            _templatePdf.Load(template.Id);
            RemoveDuplicateFields(template);
            ValidateFieldsInTemplatesPagesRange(template);
            _templatePdf.DoUpdateFields(template.Fields);

     
            
        }
      
        private void ValidateFieldsInTemplatesPagesRange(Template template)
        {
            int pagesCount = _templatePdf.GetPagesCount();
            foreach (TextField field in template.Fields.TextFields)
            {
                if (field.Page < 1 || field.Page > pagesCount)
                {
                    throw new InvalidOperationException(ResultCode.InvalidPageNumber.GetNumericString());
                }
            }
        }

        private void RemoveDuplicateFields(Template template)
        {
            template.Fields.TextFields = template.Fields.TextFields.GroupBy(x => new { x.Name, x.Page, x.Height, x.Width, x.X, x.Y }).Select(y => y.FirstOrDefault()).ToList();
            template.Fields.CheckBoxFields = template.Fields.CheckBoxFields.GroupBy(x => new { x.Name, x.Page, x.Height, x.Width, x.X, x.Y }).Select(y => y.FirstOrDefault()).ToList();
            template.Fields.ChoiceFields = template.Fields.ChoiceFields.GroupBy(x => new { x.Name, x.Page, x.Height, x.Width, x.X, x.Y }).Select(y => y.FirstOrDefault()).ToList();
            template.Fields.SignatureFields = template.Fields.SignatureFields.GroupBy(x => new { x.Name, x.Page, x.Height, x.Width, x.X, x.Y }).Select(y => y.FirstOrDefault()).ToList();
            foreach (RadioGroupField group in template.Fields.RadioGroupFields)
            {
                group.RadioFields = group.RadioFields.GroupBy(x => new { x.Name, x.Page, x.Height, x.Width, x.X, x.Y }).Select(y => y.FirstOrDefault()).ToArray();
            }
            
        }

        private string ConvertToPdf(string base64file)
        {
            //ImageType
            if (_dataUriScheme.IsValidImageType(base64file, out _))
            {
                return _templatePdf.ConvertImageToPdf(_dataUriScheme.Getbase64Content(base64file));
            }
            //WordType 
            string content = _dataUriScheme.Getbase64Content(base64file);
            string pdfBase64string = _pdfConverter.Convert(content, base64file?.Split(new char[] { ',' })?.FirstOrDefault());
            return pdfBase64string;
           
         
        }

        private async Task<bool> IsTemplateBelongToUserGroup(Template template, bool fetchTemplate)
        {
            (User user, _) = await _users.GetUser();
            if(fetchTemplate)
            {
                template = await _templateConnector.Read(template);
            }
            

            return template?.GroupId == user.GroupId;
        }

        private string CreateDuplicateTemplateName(string name)
        {
            name = GenerateNameWithCopySuffix(name);
            string result = "";
            byte[] randomNumber = new byte[6];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                result = $"{name}_{Convert.ToBase64String(randomNumber)}";
            }
            return result;
        }

        private string GenerateNameWithCopySuffix(string name)
        {
            if (name.Contains(COPY_TEMPLATE_SUFFIX))
            {
                int underscoreIndex = name.IndexOf(COPY_TEMPLATE_SUFFIX) + COPY_TEMPLATE_SUFFIX.Length;
                if (underscoreIndex < name.Length && name.ElementAt(underscoreIndex) == '_')
                {
                    name = name[..underscoreIndex];
                 
                }
            }
            else
            {
                name = $"{name}{COPY_TEMPLATE_SUFFIX}";
            }

            return name;
        }

        /// <summary>
        /// Method fills missing additional data to Debenu fields.
        /// </summary>
        /// <param name="templateFields"></param>
        /// <param name="debenuFields"></param>
        private void FillTemplateFieldsMissingData(PDFFields templateFields, PDFFields debenuFields)
        {
            if (templateFields == null || debenuFields == null)
                return;
            IEnumerable<TextField> uniqueTextFieldItems = templateFields.TextFields.GroupBy(x => x.Name).Select(x => x.First());
            Dictionary<string, TextField> textFieldMap = uniqueTextFieldItems.ToDictionary(x => x.Name, x => x);
            foreach (TextField item in debenuFields.TextFields ?? Enumerable.Empty<TextField>())
            {
                if (textFieldMap.ContainsKey(item.Name))
                {
                    item.TextFieldType = textFieldMap[item.Name].TextFieldType;
                    item.CustomerRegex = textFieldMap[item.Name].CustomerRegex;
                }
            }
            
            foreach (SignatureField item in debenuFields.SignatureFields ?? Enumerable.Empty<SignatureField>())
            {
                SignatureField sigField = templateFields.SignatureFields.FirstOrDefault(x => x.Name == item.Name);
                if (sigField != null)
                {
                    item.SigningType = sigField?.SigningType ?? SignatureFieldType.Graphic;
                    item.SignatureKind = sigField?.SignatureKind ?? SignatureFieldKind.Simple;
                }
                else
                {
                    item.SigningType = SignatureFieldType.Graphic;
                    item.SignatureKind = SignatureFieldKind.Simple;
                }
                
            }
        }
        private string GetReadTemplateInfoFlow(Template dbTemplate, User user)
        {
            return $"{dbTemplate.Id}_{user.Id}_ReadTemplateFlow";
        }


        private async Task GetUserNameForTemplates(User user, IEnumerable<Template> templates)
        {
            Dictionary<Guid, string> users = new Dictionary<Guid, string>();
            foreach (Template template in templates ?? Enumerable.Empty<Template>())
            {
                if (template.UserId == user.Id)
                {
                    template.UserName = user.Name;
                    continue;
                }
                if (users.ContainsKey(template.UserId))
                {
                    template.UserName = users[template.UserId];
                    continue;
                }
                string name = (await _userConnector.Read(new User { Id = template.UserId }))?.Name;                
                template.UserName = name;
                users[template.UserId] = name;
            }
        }

       private async Task<Template> ReadAndValidateTempalteGroup(Template template)
        {
            Template dbTemplate = await _templateConnector.Read(template);
            if (dbTemplate == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
            }
            if (!await IsTemplateBelongToUserGroup(dbTemplate, false))
            {
                throw new InvalidOperationException(ResultCode.TemplateNotBelongToUserGroup.GetNumericString());
            }

            return dbTemplate;
        }

        #endregion
    }
}
