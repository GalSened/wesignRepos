namespace PdfHandler.pdf
{
    using Common.Enums.PDF;
    using Common.Extensions;
    using Common.Interfaces.PDF;
    using Common.Models.Files.PDF;
    using Common.Models.Settings;
    using Common.Models.XMLModels;
    using Microsoft.Extensions.Options;
    using PdfHandler.Enums;
    using PdfHandler.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Serilog;
    using iTextSharp.text.pdf;
    using System.Text;
    using PdfHandler.Properties;
    using System.Drawing.Imaging;
    using iTextSharp.text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Caching.Memory;
    using System.Data;
    using Common.Models.PDF;
    using Common.Enums;
    using Common.Interfaces.Files;
    using System.Reflection;
    using Common.Enums.Documents;

    public abstract class Pdf
    {
        private const string DEFAULT_COLOR = "White";
        private const string CHECKBOX_CHECKED = "Yes";
        private const string CHECKBOX_UNCHECKED = "Off";
        private const int FAILED = 0;
        private const int LOAD_AS_USUAL = 0;       
        private const int RED = 1;
        private const int GREEN = 2;
        private const int BLUE = 3;
        private const int SplitWordsExtractOptions = 4;
        private const int FIRST_PAGE_ROW_NUMBER = 32;
        private bool _isLoaded = false;
        //protected string _directoryPath = "";
        protected Guid _id;

        protected readonly GeneralSettings _generalSettings;
        private readonly IDebenuPdfLibrary _debenu;

        protected readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IFilesWrapper _fileWrapper;
        private readonly IFileSystem _fileSystem;
        private const string DOCUMENT_COLLECTION_AUDIT_TRACE_TEMPLATE = "Resources/BaseTraceTemplate.pdf";
            
        public virtual IList<Common.Models.Files.PDF.PdfImage> Images { get => GetImages(); }
        public IExtendedList<SignatureField> SignatureFields { get => GetFields<Common.Models.Files.PDF.SignatureField>(); }
        public IExtendedList<Common.Models.Files.PDF.TextField> TextFields { get => GetFields<Common.Models.Files.PDF.TextField>(); }
        public IExtendedList<CheckBoxField> CheckBoxFields { get => GetFields<CheckBoxField>(); }
        public IExtendedList<ChoiceField> ChoiceFields { get => GetFields<Common.Models.Files.PDF.ChoiceField>(); }
        public IExtendedList<RadioGroupField> RadioGroupFields { get => GetFields<RadioGroupField>(); }

        public Pdf(IOptions<GeneralSettings> generalSettings, IDebenuPdfLibrary debenu,
             ILogger logger, IServiceScopeFactory scopeFactory, IMemoryCache memoryCache,
            IFilesWrapper fileWrapper, IFileSystem fileSystem
            )
        {
            _debenu = debenu;
            _generalSettings = generalSettings.Value;

            _logger = logger;
            _scopeFactory = scopeFactory;
            _memoryCache = memoryCache;
            _fileWrapper = fileWrapper;
            _fileSystem = fileSystem;
        }



        public void DoUpdateFields(PDFFields templateInputPdfFields)
        {
            var templateFields = GetAllFields();

            var frontFieldsDic = PdfFieldsWithoutRadiosToFieldsDictionary(templateInputPdfFields);
            var templateFieldsDic = PdfFieldsWithoutRadiosToFieldsDictionary(templateFields);

            var deleteThisFieldsDic = new HashSet<string>();
            var addThisFieldsDic = new HashSet<string>();
            var checkThisFieldsDic = new HashSet<string>();
            // Delete (key that exist in template and dosen't exist in front
            foreach (var templateFieldKey in templateFieldsDic.Keys)
            {
                if (!frontFieldsDic.ContainsKey(templateFieldKey))
                {
                    deleteThisFieldsDic.Add(templateFieldKey);
                }
            }

            // Add (if key dosen't exist in template it means that it's a new field)
            foreach (var frontFieldKey in frontFieldsDic.Keys)
            {
                if (!templateFieldsDic.ContainsKey(frontFieldKey))
                {
                    addThisFieldsDic.Add(frontFieldKey);
                }
            }

            // Update (if key exist in both front and template)
            foreach (var templateFieldKey in templateFieldsDic.Keys)
            {
                if (frontFieldsDic.ContainsKey(templateFieldKey))
                {
                    //var type = frontFieldsDic[templateFieldKey].GetType();
                    var templateField = templateFieldsDic[templateFieldKey];
                    var frontField = frontFieldsDic[templateFieldKey];


                    bool areLocationsAndSizesEqual = AreLocationsAndSizesEqual(templateField, frontField);

                    if (!areLocationsAndSizesEqual)
                    {

                        UpdateFormFieldLocation(frontField);



                    }
                    else
                    {
                        checkThisFieldsDic.Add(templateFieldKey);
                    }
                }
            }
            // Delete
            var pdfFieldsToDelete = FieldsSetToPdfFieldsConverter(deleteThisFieldsDic, templateFieldsDic);

            // Add
            var pdfFieldsToAdd = FieldsSetToPdfFieldsConverter(addThisFieldsDic, frontFieldsDic);

            // Update
            var frontPdfFieldsToModify = FieldsSetToPdfFieldsConverter(checkThisFieldsDic, frontFieldsDic);
            var templatePdfFieldsToModify = FieldsSetToPdfFieldsConverter(checkThisFieldsDic, templateFieldsDic);

            // Radio Fields
            RadioFieldsRemoveFormPDF(templateInputPdfFields.RadioGroupFields, templateFields.RadioGroupFields);
            RadioFieldsAddToPDF(templateInputPdfFields.RadioGroupFields, templateFields.RadioGroupFields);
            RadioFieldsModifyFields(templateInputPdfFields.RadioGroupFields, templateFields.RadioGroupFields);

            DeleteFields(pdfFieldsToDelete);
            AddFields(pdfFieldsToAdd);

            ModifyFields(checkThisFieldsDic, templateInputPdfFields);
            SaveDocument();
            EmbadTextDataFields(templateInputPdfFields.TextFields, templateInputPdfFields.ChoiceFields);

            Load(_id);

        }

        private void UpdateFormFieldLocation(Common.Models.Files.PDF.BaseField field)
        {

            var fieldCount = _debenu.FormFieldCount();
            for (int i = 1; i <= fieldCount; i++)
            {

                if (_debenu.GetFormFieldTitle(i) == field.Name)
                {

                    double pageWidth = _debenu.PageWidth();
                    double pageHeight = _debenu.PageHeight();
                    _debenu.SetFormFieldBounds(i,
                                               field.X * pageWidth, field.Y * pageHeight, field.Width * pageWidth, field.Height * pageHeight);
                    _debenu.SetFormFieldPage(i, field.Page);
                    _debenu.SetFormFieldDescription(i, field.Description);
                    _debenu.SetFormFieldRequired(i, (field.Mandatory) ? 1 : 0);
                    break;

                }
            }
        }

        public virtual bool SaveDocument()
        {
            throw new NotImplementedException("SaveDocument");
        }
        public virtual DocumentType GetDocumentType()
        {
            throw new NotImplementedException("GetDocumentType");
        }

        protected byte[] DocumentPagesRotation(byte[] fileData)
        {
            _debenu.LoadFromString(fileData, "");
            var pageCount = _debenu.PageCount();

            bool compress = TryAppendToString().Length >= _generalSettings.CompressFilesOverSizeBytes;
            if (compress)
            {
                _debenu.CompressImages(1);
            }
            for (int i = 1; i <= pageCount; ++i)
            {
                _debenu.SelectPage(i);
                _debenu.SetOrigin(1);



                int rotation = _debenu.PageRotation();
                if (rotation != 0)
                {
                    _debenu.NormalizePage(0);
                }

            }

            if (compress)
            {
                _debenu.CompressImages(0);
                _debenu.ReduceSize(0);

            }
            byte[] document = TryAppendToString();
            string s = Convert.ToBase64String(document);
            if (document.Length >= _generalSettings.CompressFilesOverSizeBytes)
            {
                document = ReducePdfSize(document);
            }
            return document;

        }

        protected byte[] ReducePdfSize(byte[] pdfBytes)
        {
            _debenu.LoadFromString(pdfBytes, "");

            var document = new Document(iTextSharp.text.PageSize.A4);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                var pageCount = _debenu.PageCount();
                for (int i = 1; i <= pageCount; ++i)
                {
                    _debenu.SelectPage(i);
                    int id = 0;
                    try
                    {
                        id = _debenu.GetPageImageList(0);
                        int count = _debenu.GetImageListCount(id);
                        for (int j = 1; j <= count; j++)
                        {


                            int width = _debenu.GetImageListItemIntProperty(id, j, 401);
                            int height = _debenu.GetImageListItemIntProperty(id, j, 402);
                            var pH = _debenu.PageHeight();
                            var pW = _debenu.PageWidth();
                            var oldBytes = _debenu.GetImageListItemDataToString(id, j, 0);

                            if (pW <= width && pH <= height)
                            {
                                // scaned
                                document.NewPage();
                                byte[] newBytes = ShrinkImage(oldBytes, 20);
                                iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance(newBytes);
                                pic.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
                                pic.SetAbsolutePosition(0, 0);
                                document.Add(pic);

                            }
                            else
                            {
                                document.NewPage();
                                byte[] newBytes = _debenu.RenderPageToString(_generalSettings.DPI, i, 1);
                                iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance(newBytes);
                                pic.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
                                pic.SetAbsolutePosition(0, 0);
                                document.Add(pic);
                                
                            }

                        }
                    }
                    finally
                    {
                        if (id > 0)
                        {
                            _debenu.ReleaseImageList(id);
                        }
                    }
                }
                document.Close();
                return memoryStream.ToArray().Length >= pdfBytes.Length ? pdfBytes : memoryStream.ToArray();
            }
        }

        public void SetAllFieldsToReadOnly()
        {
            if (!_isLoaded)
                throw new FileLoadException();
            int fieldCount = _debenu.FormFieldCount();
            for (int i = 1; i <= fieldCount; i++)
            {

                if (_debenu.GetFormFieldType(i) != (int)DebenuFieldType.SignatureField &&
                    _debenu.GetFormFieldType(i) != (int)DebenuFieldType.TextField &&
                    _debenu.GetFormFieldType(i) != (int)DebenuFieldType.ChoiceField)
                {

                    _debenu.SetFormFieldReadOnly(i, 1);

                    _debenu.UpdateAppearanceStream(i);

                }

            }

        }
        private byte[] ShrinkImage(byte[] inputBytes, int jpegQuality = 50)
        {
            using (var inputStream = new MemoryStream(inputBytes))
            {
                using (var image = System.Drawing.Image.FromStream(inputStream))
                {
                    var jpegEncoder = ImageCodecInfo.GetImageDecoders()
                      .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);

                    using (var outputStream = new MemoryStream())
                    {
                        image.Save(outputStream, jpegEncoder, encoderParameters);

                        return outputStream.ToArray();
                    }
                }
            }
        }

        public string ConvertImagesToPdf(string base64)
        {
            int pageId = _debenu.AddImageFromString(Convert.FromBase64String(base64), LOAD_AS_USUAL);
            _debenu.SelectPage(pageId);
            int dpix = _debenu.ImageHorizontalResolution();
            int dpiy = _debenu.ImageVerticalResolution();

            if (dpix == 0 || dpix == 1)
            {
                dpix = 72;
            }
            if (dpiy == 0 || dpiy == 1)
            {
                dpiy = 72;
            }

            double ImageWidthInPoints = (double)_debenu.ImageWidth() / dpix * 72.0;
            double ImageHeightInPoints = (double)_debenu.ImageHeight() / dpiy * 72.0;
            _debenu.SetPageDimensions(ImageWidthInPoints, ImageHeightInPoints);
            _debenu.SetOrigin(1);
            _debenu.DrawImage(0, 0, ImageWidthInPoints, ImageHeightInPoints);
            return Convert.ToBase64String(_debenu.SaveToString());
        }

        public int GetPagesCount()
        {
            if (!_isLoaded)
            {
                if (_id == Guid.Empty || !Load(_id))
                {
                    throw new FileLoadException();
                }
            }
            return _debenu.PageCount();
            //return _cache.GetOrAdd(_id + "_PageCount", () => _debenu.PageCount(), DateTimeOffset.UtcNow.AddSeconds(45));
        }

        public Dictionary<int, SizeF> GetPagesDimensions()
        {
            if (!_isLoaded)
            {
                if (_id == Guid.Empty || !Load(_id))
                {
                    throw new FileLoadException();
                }
            }

            var pagesDimensions = new Dictionary<int, SizeF>();
            var pageCount = _debenu.PageCount();
            for (int i = 1; i <= pageCount; ++i)
            {
                _debenu.SelectPage(i);
                double pageWidth = _debenu.PageWidth();
                double pageHeight = _debenu.PageHeight();
                pagesDimensions.Add(i, new SizeF(Convert.ToSingle(pageWidth), Convert.ToSingle(pageHeight)));
            }
            return pagesDimensions;
            //return _cache.GetOrAdd(_id + "_PageCount", () => _debenu.PageCount(), DateTimeOffset.UtcNow.AddSeconds(45));
        }

        public IList<FieldCoordinate> GetPlaceholdersFromPdf(string parenthesis = null, string placeholdercolor = null)
        {
            var fieldCoordinates = new List<FieldCoordinate>();
            string placeholderColorInPdf = placeholdercolor ?? DEFAULT_COLOR;
            string[] parenthesisInPdf = !string.IsNullOrEmpty(parenthesis) ? new[] { parenthesis[0].ToString(), parenthesis[1].ToString() } : new[] { "{", "}" };
            if (!_isLoaded)
            {
                throw new FileLoadException();
            }

            for (int i = 1; i <= _debenu.PageCount(); i++)
            {
                _debenu.SelectPage(i);
                int id = _debenu.ExtractPageTextBlocks(SplitWordsExtractOptions);
                for (int f = 1; f <= _debenu.GetTextBlockCount(id); f++)
                {
                    (string text, Common.Models.XMLModels.ColorType color) = GetTextAndColorInTextBlock(id, f);

                    if (!color.Equals(placeholderColorInPdf))
                    {
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    FieldCoordinate field = new FieldCoordinate
                    {
                        Text = GetFieldName(parenthesisInPdf, text, ref f, id, out BlockTextType blockTextType),
                        FontName = _debenu.GetTextBlockFontName(id, f),
                        TextSize = _debenu.GetTextBlockFontSize(id, f),
                        TextColor = System.Drawing.Color.FromName(placeholderColorInPdf),//todo - tests..
                        Page = i
                    };

                    SetFieldCoordinates(field, id, f, blockTextType);

                    field.Left /= _debenu.PageWidth();
                    field.Top /= _debenu.PageHeight();
                    field.Width /= _debenu.PageWidth();
                    field.Height /= _debenu.PageWidth();

                    fieldCoordinates.Add(field);

                }
                _debenu.ReleaseTextBlocks(id);
            }
            return fieldCoordinates;

        }

        private PDFFields DoGetAllFields(bool includeSignatures, bool includeSignatureImages)
        {
            if (!_isLoaded)
                throw new FileLoadException();
            var pdfFields = new PDFFields();
            _debenu.SetOrigin(1);
            int fieldCount = _debenu.FormFieldCount();
            for (int i = 1; i <= fieldCount; i++)
            {
                switch (_debenu.GetFormFieldType(i))
                {
                    case (int)DebenuFieldType.TextField:
                        var textField = GetTextFieldByIndex(i);
                        pdfFields.TextFields.Add(textField);
                        break;
                    case (int)DebenuFieldType.ChoiceField:
                        var choiceField = GetChoiceFieldByIndex(i);
                        pdfFields.ChoiceFields.Add(choiceField);
                        break;
                    case (int)DebenuFieldType.CheckBoxField:
                        var checkBox = GetCheckBoxFieldByIndex(i);
                        pdfFields.CheckBoxFields.Add(checkBox);
                        break;
                    case (int)DebenuFieldType.RadioGroupField:
                        var radioGroup = GetRadioGroupFieldByIndex(i);
                        pdfFields.RadioGroupFields.Add(radioGroup);
                        break;
                    case (int)DebenuFieldType.SignatureField:
                        if (includeSignatures)
                        {
                            var signature = GetSignatureFieldByIndex(i, includeSignatureImages);
                            pdfFields.SignatureFields.Add(signature);
                        }
                        break;
                    default:
                        break;
                }
            }
            SetSameValueToAllSameFieldsName(pdfFields.TextFields);
            return pdfFields;
        }

        public PDFFields GetAllFieldsWithoutSigFields()
        {
            return DoGetAllFields(false, false);
        }
        public PDFFields GetAllFields(bool includeSignatureImages = true)
        {
            return DoGetAllFields(true, includeSignatureImages);
         

        }
        public PDFFields GetAllFields(int startPage, int endPage, bool includeSignatureImages = true)
        {
            if (!_isLoaded)
            {
                if (_id == Guid.Empty || !Load(_id))
                {
                    throw new FileLoadException();
                }
            }

            var fields = GetAllFields(includeSignatureImages);
            return GetAllFieldsInRange(startPage, endPage, fields);
        }

        public void SetAllSignatureFieldsMandatory()
        {
            try
            {
                if (!_isLoaded)
                {
                    if (_id == Guid.Empty || !Load(_id))
                    {
                        throw new FileLoadException();
                    }
                }

                int fieldCount = _debenu.FormFieldCount();
                for (int i = 1; i <= fieldCount; i++)
                {
                    if (_debenu.GetFormFieldType(i) == (int)DebenuFieldType.SignatureField)
                    {
                        _debenu.SetFormFieldRequired(i, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to set all signature fields mandatory in template {TemplateId}", _id);
            }
        }
       
        private void SetSameValueToAllSameFieldsName(IEnumerable<Common.Models.Files.PDF.TextField> textFields)
        {

            var fields = textFields.GroupBy(x => x.Name).SelectMany(x => x).Where(v => !string.IsNullOrWhiteSpace(v.Value));

            fields.ForEach(field =>
            {
                textFields.Where(t => t.Name == field.Name).Select(c => { c.Value = field.Value; return c; });
            });
        }

     

        public PDFFields GetAllFieldsInRange(int startPage, int endPage, PDFFields fields)
        {
            fields.TextFields = fields.TextFields.Where(x => Enumerable.Range(startPage, endPage).Contains(x.Page)).ToList();
            fields.TextFields.ForEach(x => { if (string.IsNullOrWhiteSpace(x.Description)) { x.Description = x.Name; } });
            fields.ChoiceFields = fields.ChoiceFields.Where(x => Enumerable.Range(startPage, endPage).Contains(x.Page)).ToList();
            fields.ChoiceFields.ForEach(x => { if (string.IsNullOrWhiteSpace(x.Description)) { x.Description = x.Name; } });
            fields.CheckBoxFields = fields.CheckBoxFields.Where(x => Enumerable.Range(startPage, endPage).Contains(x.Page)).ToList();
            fields.CheckBoxFields.ForEach(x => { if (string.IsNullOrWhiteSpace(x.Description)) { x.Description = x.Name; } });
            fields.SignatureFields = fields.SignatureFields.Where(x => Enumerable.Range(startPage, endPage).Contains(x.Page)).ToList();
            fields.SignatureFields.ForEach(x => { if (string.IsNullOrWhiteSpace(x.Description)) { x.Description = x.Name; } });
            foreach (var radioGroupField in fields.RadioGroupFields)
            {
                radioGroupField.RadioFields = radioGroupField.RadioFields.Where(x => Enumerable.Range(startPage, endPage).Contains(x.Page)).ToArray();
                radioGroupField.RadioFields.ForEach(x => { if (string.IsNullOrWhiteSpace(x.Description)) { x.Description = x.Name; } });
                if (!radioGroupField.RadioFields.Any())
                {
                    radioGroupField.Name = "";
                    radioGroupField.SelectedRadioName = "";
                }
            }
            fields.RadioGroupFields = fields.RadioGroupFields.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();

            return fields;
        }

        public virtual bool Load(Guid id, bool includeMemoryLoading = true)
        {
            throw new NotImplementedException("Load(Guid id) not implemented");
        }

        protected bool IsLoaded(byte[] file, Guid id, bool shouldLoadToMemory = true)
        {
            if (_debenu == null)
            {
                throw new FileLoadException("Debenu not found");
            }
            if (!shouldLoadToMemory)
            {
                _id = id;
                return false;
            }

            if (_debenu.LoadFromString(file, string.Empty) != FAILED)
            {
                _isLoaded = true;

            }
            return _isLoaded;
        }

        protected byte[] Save()
        {
            if (!_isLoaded) throw new FileLoadException();

            // Return original source plus the update section
            int appendMode = 0;

            return TryAppendToString(appendMode);

        }


        protected byte[] Save(int filehandler)
        {
            if (!_isLoaded) throw new FileLoadException();

            return _debenu.AppendToString(0);
        }


        /// <summary>
        /// Use this function if you should use later in UpdateFile function 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        protected (bool isSuccess, int fileHandler) IsLoadedFile(string file, string directoryPath)
        {
            if (_debenu == null)
            {
                throw new FileLoadException("Debenu not found");
            }
            int fileHandler = _debenu.DAOpenFile(file, "");
            if (fileHandler != FAILED)
            {
                _isLoaded = true;

            }
            return (_isLoaded, fileHandler);
        }

        internal void SetFieldsValues<T>(IEnumerable<T> fields)
        {

            if (!_isLoaded) throw new FileLoadException();
            if (typeof(T) == typeof(RadioGroupField))
            {
                UpdateRadioGroupValueInPDF(fields);

                return;
            }

            foreach (var t in fields)
            {
                //if(_debenu.GetNeedAppearances())
                // _debenu.SetNeedAppearances(-1);
                var field = t as Common.Models.Files.PDF.BaseField;
                var fieldType = (DebenuFieldType)Enum.Parse(typeof(DebenuFieldType), typeof(T).Name, true);
                switch (fieldType)
                {
                    case DebenuFieldType.TextField:

                        _debenu.SetFormFieldValueByTitle(field.Name, ((Common.Models.Files.PDF.TextField)field).Value);
                        break;

                    case DebenuFieldType.CheckBoxField:
                        _debenu.SetFormFieldValueByTitle(field.Name, ((CheckBoxField)field).IsChecked ? CHECKBOX_CHECKED : CHECKBOX_UNCHECKED);
                        break;
                    case DebenuFieldType.ChoiceField:
                        var choice = field as Common.Models.Files.PDF.ChoiceField;
                        _debenu.SetFormFieldValueByTitle(choice.Name, choice.SelectedOption);
                        break;
                }
            }
        }

        private void UpdateRadioGroupValueInPDF<T>(IEnumerable<T> fields)
        {
            //SetRadioValue((IEnumerable<RadioGroupField>)fields);
            foreach (var t in fields)
            {
                var field = t as Common.Models.Files.PDF.RadioGroupField;
                var res = _debenu.SetFormFieldValueByTitle(field.Name, field.SelectedRadioName);
                // _debenu.SetNeedAppearances(1);
            }
        }

        internal void AddRange<T>(IEnumerable<T> fields)
        {
            if (!_isLoaded) throw new FileLoadException();
            if (typeof(T) == typeof(RadioGroupField))
            {
                AddRadioGroupRange((IEnumerable<RadioGroupField>)fields);
                return;
            }
            foreach (var item in fields)
            {
                _debenu.SetNeedAppearances(0);

                var field = item as Common.Models.Files.PDF.BaseField;
                var fieldType = (DebenuFieldType)Enum.Parse(typeof(DebenuFieldType), typeof(T).Name, true);
                _debenu.SelectPage(field.Page);
                var newfieldInPDFIndex = _debenu.NewFormField(field.Name, (int)fieldType);
                double pageWidth = _debenu.PageWidth();
                double pageHeight = _debenu.PageHeight();
                _debenu.SetFormFieldBounds(newfieldInPDFIndex,
                                           field.X * pageWidth, field.Y * pageHeight, field.Width * pageWidth, field.Height * pageHeight);
                _debenu.SetFormFieldDescription(newfieldInPDFIndex, field.Description);
                _debenu.SetFormFieldRequired(newfieldInPDFIndex, (field.Mandatory) ? 1 : 0);

                switch (fieldType)
                {
                    case DebenuFieldType.TextField:
                        _debenu.SetFormFieldAlignment(newfieldInPDFIndex, (int)DebenuAlignment.Centered);
                        string value = ((Common.Models.Files.PDF.TextField)field).Value;
                        //  _debenu.SetNeedAppearances(newfieldInPDFIndex);
                        _debenu.SetFormFieldValue(newfieldInPDFIndex, value);
                        _debenu.UpdateAppearanceStream(newfieldInPDFIndex);
                        _debenu.SetFormFieldVisible(newfieldInPDFIndex, ((Common.Models.Files.PDF.TextField)field).IsHidden ? 0 : 1);
                        break;
                    case DebenuFieldType.CheckBoxField:
                        _debenu.SetFormFieldValue(newfieldInPDFIndex, ((CheckBoxField)field).IsChecked ? CHECKBOX_CHECKED : CHECKBOX_UNCHECKED);
                        _debenu.SetFormFieldCheckStyle(newfieldInPDFIndex, (int)DebenuCheckBoxCheckStyle.Check, (int)DebenuCheckBoxCheckPosition.Center);
                        break;
                    case DebenuFieldType.ChoiceField:
                        var choice = item as Common.Models.Files.PDF.ChoiceField;
                        _debenu.SetFormFieldChoiceType(newfieldInPDFIndex, (int)DebenuFieldChoiceType.DropdownComboBox);
                        _debenu.SetFormFieldBorderStyle(newfieldInPDFIndex, 1, 0, 0, 0);
                        _debenu.SetFormFieldBackgroundColor(newfieldInPDFIndex, 0.9, 0.9, 0.9);
                        _debenu.SetFormFieldBorderColor(newfieldInPDFIndex, 0.5, 0.5, 0.5);
                        for (int i = 0; i < choice.Options?.Length; ++i)
                        {
                            _debenu.AddFormFieldChoiceSub(newfieldInPDFIndex, choice.Options[i], choice.Options[i]);
                        }
                        _debenu.SetFormFieldValue(newfieldInPDFIndex, choice.SelectedOption);
                        break;
                }
            }
        }

        internal void Clear<T>()
        {
            var fieldType = (DebenuFieldType)Enum.Parse(typeof(DebenuFieldType), typeof(T).Name, true);
            var count = _debenu.FormFieldCount();
            List<int> indexFieldsToDelete = new List<int>();
            for (int i = 1; i <= count; i++)
            {
                if (_debenu.GetFormFieldType(i) == (int)fieldType)
                {
                    indexFieldsToDelete.Add(i);
                }
            }
            for (int i = indexFieldsToDelete.Count - 1; i >= 0; i--)
            {
                _debenu.DeleteFormField(indexFieldsToDelete.ElementAt(i));
            }
        }

        #region Private

        //From FileSystem
        private IList<Common.Models.Files.PDF.PdfImage> GetImages()
        {
            if (_id != Guid.Empty)
            {
                //        return _cache.GetOrAdd<IList<Common.Models.Files.PDF.PdfImage>>(_id.ToString() + "GetImages", () => GetImagesFromDisk(), DateTimeOffset.UtcNow.AddSeconds(45));
            }
            return GetImagesFromDisk();
        }

        private IList<Common.Models.Files.PDF.PdfImage> GetImagesFromDisk()
        {
            var images = _fileWrapper.Documents.ReadAllImagesOfDocument(GetDocumentType(), _id);

            if (images.Count == 0)
            {
                CreateImagesFromPdfInFileSystem();
                images = _fileWrapper.Documents.ReadAllImagesOfDocument(GetDocumentType(), _id);
            }


            return images;
        }


        public void CreateImagesFromPdfInFileSystem()
        {
            _fileWrapper.Documents.CreateImagesFromData(GetDocumentType(), _id, _debenu.SaveImageToString());
        }

        //private void RemoveAllFieldsFromLoadedPDF(IDebenuPdfLibrary pdfLibrary)
        //{
        //    if (_generalSettings.ShouldDeleteFormFieldsBeforeCreateImage)
        //    {
        //        while (pdfLibrary.FormFieldCount() > 0)       //Remove all Fields from PDF file
        //        {
        //            pdfLibrary.SetNeedAppearances(-1);
        //            pdfLibrary.DeleteFormField(1);
        //        }
        //    }
        //}
        protected int GetPages()
        {
            return _debenu.PageCount();
        }


        public bool AddTextFields(byte[] pdfBytes, List<Common.Models.Files.PDF.TextField> textFields)
        {
            try
            {

                using (MemoryStream outputMemoryStream = new MemoryStream())
                {
                    PdfReader reader = new PdfReader(pdfBytes);
                    PdfStamper stamper = CreateStamper(ref reader, pdfBytes, outputMemoryStream, '\0', true, out _);
                    int pagesCount = GetPagesCount();
                    var pagesDimensions = GetPagesDimensions();
                    foreach (var textField in textFields)
                    {
                        var x = Convert.ToSingle(textField.X) * pagesDimensions[textField.Page].Width;
                        var y = Convert.ToSingle(1 - textField.Y) * pagesDimensions[textField.Page].Height;  // 0,0 down left  and not up left as debenu work
                        var w = Convert.ToSingle(textField.Width) * pagesDimensions[textField.Page].Width;
                        var h = Convert.ToSingle(textField.Height) * pagesDimensions[textField.Page].Height;
                        var rec = new iTextSharp.text.Rectangle(x, y - h, x + w, y);

                        iTextSharp.text.pdf.TextField tf = new iTextSharp.text.pdf.TextField(stamper.Writer, rec, textField.Name);
                        if (textField.TextFieldType == TextFieldType.Multiline)
                        {
                            tf.Options = iTextSharp.text.pdf.TextField.MULTILINE;
                        }
                        if (textField.IsHidden)
                        {
                            tf.Visibility = iTextSharp.text.pdf.BaseField.HIDDEN;
                        }
                        if (textField.Mandatory)
                        {
                            tf.Options = iTextSharp.text.pdf.BaseField.REQUIRED;
                        }


                        PdfFormField itextSharpField = tf.GetTextField();
                        itextSharpField.UserName = textField.Description;
                        stamper.AddAnnotation(itextSharpField, textField.Page);
                    }

                    stamper.Close();
                    reader.Close();
                    byte[] filledPdfBytes = outputMemoryStream.ToArray();
                    _fileWrapper.Documents.SaveDocument(GetDocumentType(), _id, filledPdfBytes);


                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "AddTextFields - failed to add text fields using iTextSharp.");
                return false;
            }
        }
        protected void RemoveFieldsFromPDF(List<string> fieldsName, DebenuFieldType fieldsType)
        {
            if (!_isLoaded) throw new FileLoadException();
            int fieldCount = _debenu.FormFieldCount();
            for (int i = fieldCount; i > 0; i--)
            {

                if (_debenu.GetFormFieldType(i) == (int)fieldsType)
                {

                    if (fieldsName.Contains(_debenu.GetFormFieldTitle(i)))
                    {
                        _debenu.DeleteFormField(i);
                    }
                }
            }
        }

        public virtual void EmbadTextDataFields(List<Common.Models.Files.PDF.TextField> textFields, List<ChoiceField> choiceFields)
        {
            throw new NotImplementedException("EmbadTextDataFields");
        }
        internal void EmbadTextData(List<Common.Models.Files.PDF.TextField> fields, List<ChoiceField> choiceFields,
            byte[] data, bool setFieldAsReadOnly)
        {


            using (MemoryStream outputMemoryStream = new MemoryStream())
            {

                PdfReader reader = new PdfReader(data);
                if (reader.AcroFields.Fields.Count == 0 && (fields.Count + choiceFields.Count) > 0)
                {
                    reader.Close();
                    // the fields not loaded 
                    _logger.Warning("Id {Id} not found fields when trying to embed fonts", _id);
                    Load(_id);
                    data = _debenu.SaveToString();
                    reader = new PdfReader(data);
                }

                PdfStamper stamper = CreateStamper(ref reader, data, outputMemoryStream, '\0', true, out _);

                AddSubstitutionFontToDocument(stamper, fields);

                if (stamper.AcroFields != null)
                {
                    if (!stamper.AcroFields.GenerateAppearances)
                    {
                        stamper.AcroFields.GenerateAppearances = true;
                    }

                    foreach (var field in fields)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(field.Value) && field.Value.Contains("&amp;"))
                            {
                                field.Value = field.Value.Replace("&amp;", "&");
                            }


                            stamper.AcroFields.SetField(field.Name, field.Value ?? "" /*, field.Value*/);
                            if (setFieldAsReadOnly)
                            {
                                stamper.AcroFields.SetFieldProperty(field.Name, "setfflags", PdfFormField.FF_READ_ONLY, null);
                                stamper.AcroFields.SetFieldProperty(field.Name, "setfflags", PdfFormField.FF_MULTILINE, null);
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "EmbadTextData - {FieldName}", field.Name);
                        }
                    }


                    foreach (var field in choiceFields)
                    {
                        try
                        {
                            var currnetFieldValue = stamper.AcroFields.GetField(field.Name) ?? "";
                            if(currnetFieldValue != field.SelectedOption)
                            {
                                stamper.AcroFields.SetField(field.Name, field.SelectedOption ?? "" /*, field.Value*/);
                            }

                            
                            if (setFieldAsReadOnly)
                            {

                                stamper.AcroFields.SetFieldProperty(field.Name, "setfflags", PdfFormField.FF_READ_ONLY, null);
                                stamper.AcroFields.RegenerateField(field.Name);
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "EmbadTextData - {FieldName}", field.Name);
                        }
                    }

                    if (!stamper.AcroFields.GenerateAppearances)
                    {
                        stamper.AcroFields.GenerateAppearances = true;
                    }

                }



                stamper.Close();
                reader.Close();
                byte[] filledPdfBytes = outputMemoryStream.ToArray();
                _fileWrapper.Documents.SaveDocument(GetDocumentType(), _id, filledPdfBytes);
            }
        }

        private void AddSubstitutionFontToDocument(PdfStamper stamper, List<Common.Models.Files.PDF.TextField> fields)
        {

            if (stamper.AcroFields != null)
            {

                AddFont(stamper, "arial.ttf", Resources.Arial);
                AddFont(stamper, "Adobe_Hebrew_Regular.otf", Resources.Adobe_Hebrew_Regular);
                AddFont(stamper, "BonaNova-Regular.ttf", Resources.BonaNova_Regular);
                ///check for chinese chars 
                if (fields.Exists(x =>
                {
                    var res = Regex.Match(x.Value ?? "", "[\u4e00-\u9fa5]+", RegexOptions.IgnoreCase);
                    return res.Success;
                }))
                {
                    AddFont(stamper, "arial-unicode-ms.ttf", Resources.arial_unicode_ms);
                }

            }
        }

        private void AddFont(PdfStamper stamper, string fontName, byte[] fontData)
        {
            try
            {
                var bf = BaseFont.CreateFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, true, fontData, null);
                if (bf != null)
                {
                    stamper.AcroFields.AddSubstitutionFont(bf);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "EmbadTextData- font embedding issue  font {FontName}", fontName);
                throw;
            }
        }

        protected byte[] CreateTraceFile(DocumentCollectionAuditTrace documentCollectionAuditTrace, DocumentMode mode)
        {

            List<byte[]> items = new List<byte[]>();
           
            _debenu.LoadFromString(GetDocumentCollectionAuditTraceBackgroundTemplate(),"");
          
            
            int defualtFontId =  SetTraceHeader(documentCollectionAuditTrace);
            int boldFont = _debenu.AddStandardFont(5);
            int lastHeight = SetSenderTraceInfo(documentCollectionAuditTrace, defualtFontId, boldFont) + 10;
            bool isSelfSign = mode.Equals(DocumentMode.SelfSign);
            for (int i = 0; i< documentCollectionAuditTrace.AuditTraceSigners.Count;++i)
            {

                if (IsNewPageNeeded(i))
                {
                    items.Add(_debenu.SaveToString());
                    _debenu.LoadFromString(GetDocumentCollectionAuditTraceBackgroundTemplate(), "");
                    _debenu.SetOrigin(1);
                    defualtFontId = _debenu.AddUnicodeFont("Arial Unicode MS", 0, 0);
                    boldFont = _debenu.AddStandardFont(5);
                    _debenu.SelectFont(defualtFontId);
                    lastHeight = 165;
                }
                SetSignerTraceInfo(lastHeight, documentCollectionAuditTrace.AuditTraceSigners[i], defualtFontId, boldFont, isSelfSign);
                lastHeight += 120;
                
            }
            items.Add(_debenu.SaveToString());

            if(items.Count > 1)
            {
                return MergeTracePages(items);
            }
            return _debenu.SaveToString();


        }

        private byte[] MergeTracePages(List<byte[]> filesDataList)
        {
            byte[] result = null;
            Guid mergeId = Guid.NewGuid();
            string tempFolder = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), mergeId.ToString());
            try
            {
                int fileIndex = 0;
                _fileSystem.Directory.CreateDirectory(tempFolder);
                foreach (byte[] file in filesDataList)
                {                  
                    var currentFilePath = _fileSystem.Path.Combine(tempFolder, $"{fileIndex}.pdf");
                    _fileSystem.File.WriteAllBytes(currentFilePath, file);

                    _debenu.AddToFileList(mergeId.ToString(), currentFilePath);

                    ++fileIndex;
                }
                string outputhPath = _fileSystem.Path.Combine(tempFolder, $"{mergeId.ToString()}.pdf");
                _debenu.MergeFileList(mergeId.ToString(), outputhPath);
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

        private bool IsNewPageNeeded(int i)
        {
            return i == 3 || i ==7 || i == 11 || i == 15 || i == 19 || i == 23 || i == 27;
        }

        private void SetSignerTraceInfo(int lastHeight, AuditTraceSigner auditTraceSigner, int defualtFontId, int boldFont, bool isSelfSign)
        {
            int heightFactor = 15;
            _debenu.SetTransparency(50);
            _debenu.SetFillColorCMYK(0, 0, 0,0.17);
            _debenu.DrawBox(50, lastHeight, 500, 105,1);
            _debenu.SetTransparency(0);
            _debenu.SetTextColor(0, 0, 0);
            lastHeight += heightFactor;
            ChangeFont(boldFont, 9);
            
            _debenu.DrawText(55, lastHeight, $"Name");
            _debenu.DrawText(280, lastHeight, $"Password");
            _debenu.DrawText(330, lastHeight, $"OTP");
            _debenu.DrawText(370, lastHeight, $"IDP");
            _debenu.DrawText(410, lastHeight, $"IP address");
            _debenu.DrawText(490, lastHeight, $"Phone");
            lastHeight += heightFactor;
            ChangeFont(defualtFontId, 9);
            
            _debenu.DrawUniscribeText(55, lastHeight, auditTraceSigner.Name,0);
            _debenu.DrawText(280, lastHeight, auditTraceSigner.DocumentPassword);
            _debenu.DrawText(330, lastHeight, auditTraceSigner.DocumentOTP);
            _debenu.DrawText(370, lastHeight,  auditTraceSigner.DocumentIDP);
            _debenu.DrawText(410, lastHeight, auditTraceSigner.SignedFromIpAddress);
            _debenu.DrawText(490, lastHeight, auditTraceSigner.Phone);
            lastHeight += heightFactor;
            ChangeFont(boldFont, 9);
            _debenu.DrawText(55, lastHeight, $"Signed by");
            _debenu.DrawText(280, lastHeight, $"E-mail");
            lastHeight += heightFactor;
            ChangeFont(defualtFontId, 9);
            if (!isSelfSign)
            {
                _debenu.DrawText(55, lastHeight, auditTraceSigner.Means);
            }
            _debenu.DrawText(280, lastHeight, auditTraceSigner.Email);
            lastHeight += heightFactor;
            ChangeFont(boldFont, 9);
            _debenu.DrawText(55, lastHeight, $"Device");
            _debenu.DrawText(210, lastHeight, $"Time Sent");
            _debenu.DrawText(330, lastHeight, $"Time Viewed");
            _debenu.DrawText(440, lastHeight, $"Time Signed");

            lastHeight += heightFactor;
            ChangeFont(defualtFontId, 9);
            _debenu.DrawText(55, lastHeight, LimitLength(auditTraceSigner.DeviceInformation, 30));
            _debenu.DrawText(210, lastHeight, $"UTC: {auditTraceSigner.TimeLastSent.ToString("dd MMM yyyy hh:mm")}");
            _debenu.DrawText(330, lastHeight, $"UTC: {auditTraceSigner.TimeViewed.ToString("dd MMM yyyy hh:mm")}");
            _debenu.DrawText(330, lastHeight + 10, $"IP: {auditTraceSigner.FirstViewIPAddress}");
            _debenu.DrawText(440, lastHeight, $"UTC: {auditTraceSigner.TimeSigned.ToString("dd MMM yyyy hh:mm")}");

        }

        private void ChangeFont(int fontId, int fontSize)
        {
            _debenu.SelectFont(fontId);
            _debenu.SetTextSize(fontSize);
            
        }

        private string LimitLength(string source, int maxLength)
        {
            if (source == null)
            {
                return string.Empty;
            }

            if (source.Length <= maxLength)
            {
                return source;
            }

            return source.Substring(0, maxLength);

        }
        private int  SetSenderTraceInfo(DocumentCollectionAuditTrace documentCollectionAuditTrace, 
            int defualtFontId, int boldFont)
        {

            _debenu.SetTextColor(0, 0.5, 1);
            _debenu.DrawText(50, 195, $"Sender Details");
            _debenu.DrawText(50, 265, $"Signers Details");
            _debenu.SetTextColor(0, 0, 0);
            ChangeFont(boldFont, 9);
            int senderTitlesHeight = 210;
            int heightFactor = 13;
            _debenu.DrawText(50, senderTitlesHeight, $"Name");            
            _debenu.DrawText(360, senderTitlesHeight, $"Phone number");
            ChangeFont(defualtFontId, 9);
            senderTitlesHeight += heightFactor;
            _debenu.DrawUniscribeText(50, senderTitlesHeight, documentCollectionAuditTrace.UserName, 0);
            _debenu.DrawText(360, senderTitlesHeight, documentCollectionAuditTrace.UserPhone);
            senderTitlesHeight += heightFactor;
            ChangeFont(boldFont, 9);
            _debenu.DrawText(50, senderTitlesHeight, $"Sent by");
            _debenu.DrawText(280, senderTitlesHeight, $"Creation time");
            _debenu.DrawText(410, senderTitlesHeight, $"IP address");
            senderTitlesHeight += heightFactor;
            ChangeFont(defualtFontId, 9);            
            _debenu.DrawText(50, senderTitlesHeight, documentCollectionAuditTrace.UserEmail);            
            _debenu.DrawText(280, senderTitlesHeight, $"UTC: {documentCollectionAuditTrace.CreationTime.ToString("dd MMM yyyy hh:mm")}");
            _debenu.DrawText(410, senderTitlesHeight, documentCollectionAuditTrace.CreationIp);
           
            senderTitlesHeight += heightFactor + 3;
           

            return senderTitlesHeight;
        }

        private int SetTraceHeader(DocumentCollectionAuditTrace documentCollectionAuditTrace)
        {
            _debenu.SetOrigin(1);
            _debenu.SetTextSize(10);            
            int c = _debenu.AddUnicodeFont("Arial Unicode MS", 0, 0);
            var j = _debenu.SelectFont(c);
            _debenu.DrawText(40, 165, $"Document ID: {documentCollectionAuditTrace.CollectionId}");
            _debenu.DrawUniscribeText(40, 180, $"Document Name: {documentCollectionAuditTrace.CollectionName}", 0);

            return c;

        }

        protected byte[] CreateTraceFile(List<string> result)
        {
            var resultArr = result.ToArray();

            if (resultArr.Length >= FIRST_PAGE_ROW_NUMBER + 1)
            {
                CreateFirstPage(resultArr.Take(FIRST_PAGE_ROW_NUMBER).ToList());
                CreateOtherPages(result.Skip(FIRST_PAGE_ROW_NUMBER).ToList());
            }
            else
            {
                CreateFirstPage(resultArr.Take(FIRST_PAGE_ROW_NUMBER).ToList());
            }

            byte[] traceFileBytes = _debenu.SaveToString();

            return traceFileBytes;
        }

        private void CreateFirstPage(List<string> result)
        {
            // Set page origin to top left, default is the bottom left
            _debenu.SetOrigin(1);
            // Invoice number text
            _debenu.SetTextSize(20);
            _debenu.DrawText(55, 65, "Trace #");
            _debenu.SetTextColor(0, 0.5, 1);
            _debenu.DrawText(140, 65, result.FirstOrDefault(x => x.Contains("Id:")).Split(':').Last());
            _debenu.SetLineColor(0, 0, 0);
            _debenu.DrawLine(20, 100, 592, 100);
            // Text color for body
            _debenu.SetTextColor(0, 0, 0);
            _debenu.SetTextSize(18);
            int y = 145;

            int c = _debenu.AddUnicodeFont("Arial Unicode MS", 0, 0);
            var j = _debenu.SelectFont(c);

            foreach (var item in result)
            {
                _debenu.DrawUniscribeText(50, y, item, 0);
                y += 20;
            }
        }
        private void CreateOtherPages(List<string> result, int rowsInPage = 36)
        {
            int newPagesCount = (result.Count / rowsInPage);
            if (result.Count % rowsInPage != 0)
                newPagesCount++;
            int count = 0;

            for (int i = 0; i < newPagesCount; i++)
            {
                var take = Math.Min(rowsInPage, result.Count - (count * rowsInPage));
                var newPageRows = result.Skip(count * rowsInPage).Take(take);

                _debenu.NewPage();
                _debenu.SetOrigin(1);
                int y = 20;

                foreach (var item in newPageRows)
                {
                    _debenu.DrawUniscribeText(50, y, item, 0);
                    y += 20;
                }

                count++;
            }
        }

        private IExtendedList<T> GetFields<T>()
        {
            if (!_isLoaded) throw new FileLoadException();
            var result = new ExtendedList<T>(this);
            _debenu.SetOrigin(1);
            int fieldCount = _debenu.FormFieldCount();
            for (int i = 1; i <= fieldCount; i++)
            {
                if (typeof(T) == typeof(Common.Models.Files.PDF.SignatureField) && _debenu.GetFormFieldType(i) == (int)DebenuFieldType.SignatureField)
                {
                    var sig = GetSignatureFieldByIndex(i, true);
                    result.InternalAdd((T)Convert.ChangeType(sig, typeof(T)));
                }
                else if (typeof(T) == typeof(Common.Models.Files.PDF.TextField) && _debenu.GetFormFieldType(i) == (int)DebenuFieldType.TextField)
                {
                    var txt = GetTextFieldByIndex(i);
                    result.InternalAdd((T)Convert.ChangeType(txt, typeof(T)));
                }
                else if (typeof(T) == typeof(CheckBoxField) && _debenu.GetFormFieldType(i) == (int)DebenuFieldType.CheckBoxField)
                {
                    var check = GetCheckBoxFieldByIndex(i);
                    result.InternalAdd((T)Convert.ChangeType(check, typeof(T)));
                }
                else if (typeof(T) == typeof(Common.Models.Files.PDF.ChoiceField) && _debenu.GetFormFieldType(i) == (int)DebenuFieldType.ChoiceField)
                {
                    var choice = GetChoiceFieldByIndex(i);
                    result.InternalAdd((T)Convert.ChangeType(choice, typeof(T)));
                }
                else if (typeof(T) == typeof(RadioGroupField) && _debenu.GetFormFieldType(i) == (int)DebenuFieldType.RadioGroupField)
                {
                    var choice = GetRadioGroupFieldByIndex(i);
                    result.InternalAdd((T)Convert.ChangeType(choice, typeof(T)));
                }
            }
            return result;
        }

        private Common.Models.Files.PDF.BaseField GetBaseFieldDataByIndex(int index)
        {
            var page = _debenu.GetFormFieldPage(index);
            _debenu.SelectPage(page);

            var result = new Common.Models.Files.PDF.BaseField
            {
                Page = page,
                Name = _debenu.GetFormFieldTitle(index),
                Mandatory = (_debenu.GetFormFieldRequired(index) == 1),
                Description = _debenu.GetFormFieldDescription(index),
                X = _debenu.GetFormFieldBound(index, (int)DebenuEdgeLocation.Left) / _debenu.PageWidth(),
                Y = _debenu.GetFormFieldBound(index, (int)DebenuEdgeLocation.Top) / _debenu.PageHeight(),
                Width = _debenu.GetFormFieldBound(index, (int)DebenuEdgeLocation.Width) / _debenu.PageWidth(),
                Height = _debenu.GetFormFieldBound(index, (int)DebenuEdgeLocation.Height) / _debenu.PageHeight()
            };

            return result;
        }

        private Common.Models.Files.PDF.TextField GetTextFieldByIndex(int index)
        {
            var textField = new Common.Models.Files.PDF.TextField(GetBaseFieldDataByIndex(index))
            {
                Value = _debenu.GetFormFieldValue(index),
                TextFieldType = TextFieldType.Text,
                IsHidden = _debenu.GetFormFieldVisible(index) == 0
            };
            if (string.IsNullOrEmpty(textField.Name))
            {
                textField.Name = $"Text_{index}";
            }
            return textField;
        }

        private Common.Models.Files.PDF.SignatureField GetSignatureFieldByIndex(int index, bool includeSignatureImages)
        {
            var signatureField = new SignatureField(GetBaseFieldDataByIndex(index));
            if (!string.IsNullOrWhiteSpace(_debenu.GetFormFieldValue(index)))
            {
                if (includeSignatureImages)
                {
                    SetSignatureImage(signatureField, index);
                }
            }

            if (string.IsNullOrEmpty(signatureField.Name)) signatureField.Name = $"Signature_{index}";
            return signatureField;
        }

        private void SetSignatureImage(SignatureField signatureField, int index)
        {
            int il = 0;
            try
            {

                // handle not visible signature- signed outside the page dimensions
                if (signatureField.Width == 0 && signatureField.Height == 0 && signatureField.X == 0 && signatureField.Y == 1)
                {
                    // set default empty image 
                    signatureField.Image = @"data:image/png;base64,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IlIGExYRKYMJywYNDQ3iTUUs5pXE3TzTyfd/Cb2f85JF7mOyGBs5icv5PqwEHFy8eGsSmVBO5OTkXIdmHRYKhX5lxjc3iLdKBAKBxT6f759bWlo2438vMbrbsG393QZ+H1aK2MOygQhkModozs5WtZR8NmPk35KFmLCISBlMWESkDCYsIlIGExYRKYMJi4iUwTsbSaxbt861f//+v2hvbx+FKtsr/cS7REIej+etWCw2KhKJ/IeZb2u49tpra997770bNE0bieexa3/rWVlZ9ZMnTz60apX4ZTO6GB6ApKTCwkItNzd3bDQaNTVhzZgxo7asrIzvm3MoDgmJSBlMWESkDCYsIlIGExYRKYMJi4iUwYRFRMpgwiIiZTBhEZEymLCISBlMWESkDCYsIlIGExYRKYMJiwYCfpg5QzBhUabS3W73OZ/P9wO/3z8U0zkoH4rlnatJRUxYlFE0TetAgqoJBAJXZWVlDamurv5uVVVVM6a7mpqaxnu93iysf8jj8ZzEw5m8FMOERcpDktKRhH6NMiUSiQRnz55dXFlZeby8vLxHQnrttdf0mpqaEBLYmrNnz47C469EeQY9sTasZvJSAL/Aj5R09913u2Kx2FBd1z8bCoXeQgnX1tbKtakrKytzTZkyRdu7d+9nUA3U1dW9/fvf/75zJREREREREREREREREREREREREVFGcbn+Hyux4yu1bpDxAAAAAElFTkSuQmCC";
                    return;
                }

                double errorFactor = 0.0001;


                var imagesLists = _memoryCache.Get<SignautreImagesList>($"{_id}_SIGNATURE_IMAGES_LISTS");
                if (imagesLists == null)
                {
                    imagesLists = new SignautreImagesList();
                    _memoryCache.Set<SignautreImagesList>($"{_id}_SIGNATURE_IMAGES_LISTS", imagesLists, TimeSpan.FromSeconds(15));
                }
                if (!imagesLists.ImagesInPage.ContainsKey(signatureField.Page))
                {
                    ReadAllSignautreImagesListInPage(imagesLists, signatureField.Page);
                    _memoryCache.Set<SignautreImagesList>($"{_id}_SIGNATURE_IMAGES_LISTS", imagesLists, TimeSpan.FromSeconds(15));
                }
                _debenu.SelectPage(signatureField.Page);
                il = _debenu.GetPageImageList(0);
                var imagesInPage = imagesLists.ImagesInPage[signatureField.Page];
                foreach (var image in imagesInPage.signautreImageInPages)
                {
                    if (
                      Math.Round(signatureField.X, 9) <= Math.Round(image.X, 9) && Math.Round((signatureField.X + signatureField.Width  + errorFactor), 9) >= Math.Round((image.X + image.W), 9)
                      &&
                      Math.Round(signatureField.Y, 9) <= Math.Round(image.Y, 9) && Math.Round((signatureField.Y + signatureField.Height + errorFactor), 9) >= Math.Round((image.Y + image.H), 9))
                    {

                        if (image.ImageId != 0)
                        {
                            try
                            {
                                var imageFromDoc = _debenu.GetImageListItemDataToString(il, image.ImageIndex, 0);
                                using (var stream = new MemoryStream(imageFromDoc))
                                {
                                    using (var img = new Bitmap(stream))
                                    {
                                        using (var mem = new MemoryStream())
                                        {
                                            img.Save(mem, ImageFormat.Png);
                                            if (mem.Length > 0)
                                            {

                                                signatureField.Image = $"data:image/png;base64,{Convert.ToBase64String(mem.ToArray())}";
                                            }
                                        }

                                    }
                                }
                            }
                            catch {
                                // do nothing
                            }
                            finally
                            {
                                _debenu.ReleaseImage(image.ImageId);

                            }
                        }
                        break;

                    }
                }



            }

            catch 
            {
                // do nothing
            }
            finally
            {

                if (il > 0)
                {
                    _debenu.ReleaseImageList(il);
                }
            }


        }

        private void ReadAllSignautreImagesListInPage(SignautreImagesList imagesLists, int page)
        {
            int il = 0;
            try
            {
                var imagesInPage = new SignautreImagesInPage();
                imagesInPage.PageNumber = page;

                _debenu.SelectPage(page);

                // Get list of images on the page
                il = _debenu.GetPageImageList(0);
                // Count number of images in the list
                int ic = _debenu.GetImageListCount(il);

                for (int k = 1; k <= ic; k++)
                {
                    int it = 0;
                    int gid = 0;
                    try
                    {
                        var image = new SignautreImageInPage();
                        // Iterate through each image and get the
                        // image type and image ID
                        it = _debenu.GetImageListItemIntProperty(il, k, 400);
                        gid = _debenu.GetImageListItemIntProperty(il, k, 405);
                        image.ImageListId = il;
                        image.ImageId = it;
                        image.ImageIndex = k;
                        image.ImageGID = gid;


                        image.X = _debenu.GetImageListItemDblProperty(il, k, 501) / _debenu.PageWidth();
                        image.Y = _debenu.GetImageListItemDblProperty(il, k, 502) / _debenu.PageHeight();
                        image.W = Math.Abs(_debenu.GetImageListItemDblProperty(il, k, 503) - _debenu.GetImageListItemDblProperty(il, k, 501)) / _debenu.PageWidth();
                        image.H = Math.Abs(_debenu.GetImageListItemDblProperty(il, k, 506) - _debenu.GetImageListItemDblProperty(il, k, 502)) / _debenu.PageHeight();
                        imagesInPage.signautreImageInPages.Add(image);
                    }
                    catch
                    {
                        // do nothing
                    }
                    finally
                    {
                        _debenu.ReleaseImage(k);
                        _debenu.ReleaseImage(it);
                        _debenu.ReleaseImage(gid);
                    }
                }




                if (imagesLists.ImagesInPage.ContainsKey(page))
                {
                    imagesLists.ImagesInPage.Remove(page);
                }
                imagesLists.ImagesInPage.Add(page, imagesInPage);
            }
            finally
            {
                if (il > 0)
                {
                    _debenu.ReleaseImageList(il);
                }




            }

        }


        private ChoiceField GetChoiceFieldByIndex(int index)
        {
            var choiceField = new ChoiceField(GetBaseFieldDataByIndex(index));

            int choicesCount = _debenu.GetFormFieldSubCount(index);
            choiceField.Options = new string[choicesCount];
            for (int j = 1; j <= choicesCount; ++j)
            {
                choiceField.Options[j - 1] = _debenu.GetFormFieldSubName(index, j);
            }
            choiceField.SelectedOption = _debenu.GetFormFieldValue(index);
            return choiceField;
        }

        private CheckBoxField GetCheckBoxFieldByIndex(int index)
        {
            return new CheckBoxField(GetBaseFieldDataByIndex(index))
            {
                IsChecked = _debenu.GetFormFieldValue(index) == CHECKBOX_CHECKED
            };
        }

        private RadioGroupField GetRadioGroupFieldByIndex(int index)
        {
            var radioGroup = new RadioGroupField();
            int radioGroupCount = _debenu.GetFormFieldSubCount(index);
            radioGroup.RadioFields = new RadioField[radioGroupCount];
            bool isGroupMandatory = _debenu.GetFormFieldRequired(index) == 1;
            for (int j = 1; j <= radioGroupCount; ++j)
            {
                radioGroup.RadioFields[j - 1] = new RadioField(GetBaseFieldDataByIndex(_debenu.GetFormFieldKidTempIndex(index, j)))
                {
                    Value = _debenu.GetFormFieldSubName(index, j + 1),
                    Mandatory = isGroupMandatory

                };
                
            }
            radioGroup.SelectedRadioName = _debenu.GetFormFieldValue(index);
            radioGroup.Name = _debenu.GetFormFieldTitle(index);
            radioGroup.RadioFields = radioGroup.RadioFields.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToArray();
            return radioGroup;
        }

        protected void AddRadioGroupRange(IEnumerable<RadioGroupField> fields)
        {
            foreach (var field in fields)
            {
                var group = field as RadioGroupField;
                var newfieldInPDFIndex = _debenu.NewFormField(group.Name, (int)DebenuFieldType.RadioGroupField);
                bool isMandatoryField = group.RadioFields.FirstOrDefault(x => x.Mandatory) != null;
                _debenu.SetFormFieldRequired(newfieldInPDFIndex, isMandatoryField ? 1 : 0);
                for (int i = 0; i < group.RadioFields.Length; ++i)
                {
                    _debenu.SelectPage(group.RadioFields[i].Page);
                    double pageWidth = _debenu.PageWidth();
                    double pageHeight = _debenu.PageHeight();
                    int r1 = _debenu.AddFormFieldSub(newfieldInPDFIndex, group.RadioFields[i].Name);
                    int res = _debenu.SetFormFieldBounds(r1, group.RadioFields[i].X * pageWidth, group.RadioFields[i].Y * pageHeight,
                        group.RadioFields[i].Width * pageWidth, group.RadioFields[i].Height * pageHeight);
                    res = _debenu.SetFormFieldCheckStyle(r1, (int)DebenuCheckBoxCheckStyle.Dot, (int)DebenuCheckBoxCheckPosition.Center);
                    res = _debenu.SetFormFieldValue(r1, string.IsNullOrWhiteSpace(group.RadioFields[i].Value) ? group.RadioFields[i].Name : group.RadioFields[i].Value);
                    res = _debenu.SetFormFieldDescription(r1, string.IsNullOrWhiteSpace(group.RadioFields[i].Description) ? group.RadioFields[i].Name : group.RadioFields[i].Description);

                    res = _debenu.SetFormFieldRequired(r1, isMandatoryField ? 1 : 0);
                }
                _debenu.SetFormFieldValue(newfieldInPDFIndex, group.SelectedRadioName);
                //      _debenu.SetNeedAppearances(1);
            }
        }

        protected void AddRadioToExistGroup(RadioField radioFieldToAdd, RadioGroupField radioGroup)
        {
            int groupIndex = _debenu.FindFormFieldByTitle(radioGroup.Name);
            if (groupIndex > 0)
            {
                _debenu.SelectPage(radioFieldToAdd.Page);
                double pageWidth = _debenu.PageWidth();
                double pageHeight = _debenu.PageHeight();
                int r1 = _debenu.AddFormFieldSub(groupIndex, radioFieldToAdd.Name);
                int res = _debenu.SetFormFieldBounds(r1, radioFieldToAdd.X * pageWidth, radioFieldToAdd.Y * pageHeight,
                   radioFieldToAdd.Width * pageWidth, radioFieldToAdd.Height * pageHeight);
                res = _debenu.SetFormFieldCheckStyle(r1, (int)DebenuCheckBoxCheckStyle.Dot, (int)DebenuCheckBoxCheckPosition.Center);
                res = _debenu.SetFormFieldValue(r1, string.IsNullOrWhiteSpace(radioFieldToAdd.Value) ? radioFieldToAdd.Name : radioFieldToAdd.Value);
                res = _debenu.SetFormFieldDescription(r1, string.IsNullOrWhiteSpace(radioFieldToAdd.Description) ? radioFieldToAdd.Name : radioFieldToAdd.Description);

                if (radioGroup.SelectedRadioName == radioFieldToAdd.Name)
                {
                    _debenu.SetFormFieldValue(groupIndex, radioGroup.SelectedRadioName);
                }

            }

        }

        public string DecodeFromUtf8(string utf8String)
        {
            if (string.IsNullOrWhiteSpace(utf8String))
            {
                return utf8String;
            }

            // read the string as UTF-8 bytes.
            byte[] encodedBytes = Encoding.UTF8.GetBytes(utf8String);

            // convert them into unicode bytes.
            byte[] unicodeBytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, encodedBytes);

            // builds the converted string.
            return Encoding.Unicode.GetString(encodedBytes);
        }

        private (string, ColorType) GetTextAndColorInTextBlock(int blockID, int index)
        {
            return (_debenu.GetTextBlockText(blockID, index), new Common.Models.XMLModels.ColorType(Convert.ToInt32(_debenu.GetTextBlockColor(blockID, index, RED) * 255),
                            Convert.ToInt32(_debenu.GetTextBlockColor(blockID, index, GREEN) * 255), Convert.ToInt32(_debenu.GetTextBlockColor(blockID, index, BLUE) * 255)));
        }

        private string GetFieldName(string[] parenthesis, string text, ref int currentElemetId, int blockId, out BlockTextType blockTextType)
        {
            text = text.Trim();
            blockTextType = BlockTextType.OneRow;
            /*  case 1 = {name}             
             *  case2 = {name  , }             
             *  case3 = { , name  , }
             *  case4 = { , name}             
             *   case 5 = }name{             
             *  case6 = }name  , {             
             *  case7 = } , name  , {
             *  case8 = } , name{             
            */
            // case 5 , 1
            string result = string.Empty;
            if (text.Contains(parenthesis[0]) && text.Contains(parenthesis[1]))
            {
                result = text.Replace(parenthesis[0], "");
                result = result.Replace(parenthesis[1], "");

            }
            //case 3 ,7,4,8
            else if (text == parenthesis[0] || text == parenthesis[1])
            {
                string nextElement = GetTextInNextElement(currentElemetId + 1, blockId).Trim();
                //case 4 , 8
                if (nextElement.Contains(parenthesis[0]) || nextElement.Contains(parenthesis[1]))
                {
                    result = nextElement.Replace(parenthesis[0], "");
                    result = result.Replace(parenthesis[1], "");
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        currentElemetId++;
                        blockTextType = BlockTextType.TwoRows;
                    }
                }
                // case 3 , 7
                else
                {
                    string nextNextElement = GetTextInNextElement(currentElemetId + 2, blockId).Trim();
                    if (nextNextElement == parenthesis[0] || nextNextElement == parenthesis[1])
                    {
                        result = nextElement;
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            currentElemetId += 2;
                            blockTextType = BlockTextType.ThreeRows;
                        }
                    }
                }
            }
            // case 2 , 6
            else if (text.Contains(parenthesis[0]) || text.Contains(parenthesis[1]))
            {
                string nextElement = GetTextInNextElement(currentElemetId + 1, blockId).Trim();
                if (nextElement == parenthesis[0] || nextElement == parenthesis[1])
                {
                    result = text.Replace(parenthesis[0], "");
                    result = result.Replace(parenthesis[1], "");
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        currentElemetId++;
                        blockTextType = BlockTextType.TwoRows;
                    }
                }
            }

            return result.Trim();
        }

        private string GetTextInNextElement(int elementIndex, int blockId)
        {
            string element = string.Empty;

            if (elementIndex <= _debenu.GetTextBlockCount(blockId))
            {
                element = _debenu.GetTextBlockText(blockId, elementIndex);
            }

            return element;
        }

        private void SetFieldCoordinates(FieldCoordinate field, int textBlockId, int currentTextBlockNumber, BlockTextType blockTextType)
        {
            double[] coordinatesLast = new double[9];
            double[] coordinatesFirst = new double[9];
            int firstTextboxIndex = currentTextBlockNumber;
            if (blockTextType == BlockTextType.TwoRows)
            {
                firstTextboxIndex = currentTextBlockNumber - 1;

            }
            else if (blockTextType == BlockTextType.ThreeRows)
            {
                firstTextboxIndex = currentTextBlockNumber - 2;

            }

            for (int j = 1; j <= 8; j++)
            {
                coordinatesLast[j] = _debenu.GetTextBlockBound(textBlockId, currentTextBlockNumber, j);
                coordinatesFirst[j] = _debenu.GetTextBlockBound(textBlockId, firstTextboxIndex, j);
            }
            CalcCoordinates(field, coordinatesFirst, coordinatesLast, blockTextType);
        }
        private void CalcCoordinates(FieldCoordinate field, double[] coordinatesFirst, double[] coordinatesLast, BlockTextType blockTextType)
        {
            field.Left = coordinatesFirst[1];
            field.Top = coordinatesFirst[8];
            field.Height = field.TextSize * 1.07;
            if (blockTextType == BlockTextType.OneRow)
            {
                field.Width = Math.Abs(coordinatesFirst[1] - coordinatesFirst[3]);
            }
            if ((blockTextType == BlockTextType.TwoRows) || (blockTextType == BlockTextType.ThreeRows))
            {
                field.Width = Math.Abs(coordinatesFirst[1] - coordinatesLast[3]);
                field.Left = Math.Min(coordinatesFirst[1], coordinatesLast[1]);
            }

        }




        private Dictionary<string, Common.Models.Files.PDF.BaseField> PdfFieldsWithoutRadiosToFieldsDictionary(Common.Models.Files.PDF.PDFFields frontFields)
        {
            var dic = new Dictionary<string, Common.Models.Files.PDF.BaseField>();
            foreach (var item in frontFields.TextFields)
            {

                if (!dic.ContainsKey(item.Name))
                {
                    dic.Add(item.Name, item);
                }
            }
            foreach (var item in frontFields.ChoiceFields)
            {
                if (!dic.ContainsKey(item.Name))
                {
                    dic.Add(item.Name, item);
                }
            }
            foreach (var item in frontFields.CheckBoxFields)
            {
                if (!dic.ContainsKey(item.Name))
                {
                    dic.Add(item.Name, item);
                }
            }
            foreach (var item in frontFields.SignatureFields)
            {
                if (!dic.ContainsKey(item.Name))
                {
                    dic.Add(item.Name, item);
                }
            }

            return dic;
        }

        private void RadioFieldsRemoveFormPDF(List<RadioGroupField> inputRadioGoups, List<RadioGroupField> templeteRadioGoups)
        {
            List<string> fieldNameToRemove = new List<string>();
            List<string> fieldToAddBack = new List<string>();
            foreach (var templeteGroup in templeteRadioGoups)
            {
                var group = inputRadioGoups.FirstOrDefault(x => x.Name == templeteGroup.Name);
                if (group == null)
                {
                    fieldNameToRemove.Add(templeteGroup.Name);
                }
                else
                {
                    foreach (var radio in templeteGroup.RadioFields)
                    {
                        var radioToRemove = group.RadioFields.FirstOrDefault(x => x.Name == radio.Name);
                        if (radioToRemove == null)
                        {
                            fieldNameToRemove.Add(group.Name);
                            fieldToAddBack.Add(group.Name);
                            break;
                        }
                    }
                }
            }

            if (fieldNameToRemove.Count > 0)
            {

                RemoveFieldsFromPDF(fieldNameToRemove, DebenuFieldType.RadioGroupField);
                foreach (var item in fieldToAddBack ?? Enumerable.Empty<string>())
                {
                    var group = inputRadioGoups.FirstOrDefault(x => x.Name == item);
                    AddRadioGroupRange(new List<RadioGroupField> { group });
                }

            }
        }



        private PDFFields FieldsSetToPdfFieldsConverter(HashSet<string> fieldsSet, Dictionary<string, Common.Models.Files.PDF.BaseField> fieldsDic)
        {
            var pdfFields = new PDFFields();

            foreach (var key in fieldsSet)
            {
                var field = fieldsDic[key];
                if (field is Common.Models.Files.PDF.TextField)
                {
                    pdfFields.TextFields.Add(fieldsDic[key] as Common.Models.Files.PDF.TextField);
                }
                else if (field is SignatureField)
                {
                    pdfFields.SignatureFields.Add(fieldsDic[key] as SignatureField);
                }
                else if (field is CheckBoxField)
                {
                    pdfFields.CheckBoxFields.Add(fieldsDic[key] as CheckBoxField);
                }
                else if (field is ChoiceField)
                {
                    pdfFields.ChoiceFields.Add(fieldsDic[key] as ChoiceField);
                }
                else
                {
                    // Radio field
                }
            }


            return pdfFields;
        }

        private void RadioFieldsAddToPDF(List<RadioGroupField> inputRadioGoups, List<RadioGroupField> templeteRadioGoups)
        {

            foreach (var inputGroup in inputRadioGoups)
            {
                var group = templeteRadioGoups.FirstOrDefault(x => x.Name == inputGroup.Name);
                if (group == null)
                {
                    AddRadioGroupRange(new List<RadioGroupField> { inputGroup });
                }
                else
                {
                    foreach (var inpuRadio in inputGroup.RadioFields)
                    {
                        var radio = group.RadioFields.FirstOrDefault(x => x.Name == inpuRadio.Name);
                        if (radio == null)
                        {
                            AddRadioToExistGroup(inpuRadio, inputGroup);
                        }
                    }
                }
            }

        }




        protected void ModifyFields(HashSet<string> checkThisFieldsDic, Common.Models.Files.PDF.PDFFields inputFields)
        {
            
            if (inputFields.ChoiceFields.Count == 0 && inputFields.CheckBoxFields.Count == 0)
            {
                return;
            }

            var fieldCount = _debenu.FormFieldCount();
            for (int i = 1; i <= fieldCount; i++)
            {


                if (_debenu.GetFormFieldType(i) == (int)DebenuFieldType.ChoiceField &&
                    inputFields.ChoiceFields.Exists(x => x.Name == _debenu.GetFormFieldTitle(i)) )
                {
                   
                        var choice = inputFields.ChoiceFields.Find(x => x.Name == _debenu.GetFormFieldTitle(i));
                        int choicesCount = _debenu.GetFormFieldSubCount(i);
                        List<string> itemsToRemove = new List<string>();
                        var itemsCopy = ((string[])choice.Options.Clone()).ToList();
                        for (int j = 1; j <= choicesCount; ++j)
                        {

                            if (!choice.Options.Contains(_debenu.GetFormFieldSubName(i, j)))
                            {
                                itemsToRemove.Add(_debenu.GetFormFieldSubName(i, j));
                            }
                            itemsCopy.Remove(_debenu.GetFormFieldSubName(i, j));
                        }
                        foreach (var item in itemsCopy ?? Enumerable.Empty<string>())
                        {
                            _debenu.AddFormFieldChoiceSub(i, item, item);
                        }
                        foreach (var item in itemsToRemove ?? Enumerable.Empty<string>())
                        {
                            _debenu.RemoveFormFieldChoiceSub(i, item);

                        }
                    
                }
                if ( _debenu.GetFormFieldType(i) == (int)DebenuFieldType.CheckBoxField
                    && inputFields.CheckBoxFields.Exists(x => x.Name == _debenu.GetFormFieldTitle(i)))
                {
                    var item = inputFields.CheckBoxFields.Find(x => x.Name == _debenu.GetFormFieldTitle(i));
                    if ((item.IsChecked && _debenu.GetFormFieldValue(i) != CHECKBOX_CHECKED) || 
                        (!item.IsChecked && _debenu.GetFormFieldValue(i) != CHECKBOX_UNCHECKED))
                    {
                        _debenu.SetFormFieldValueByTitle(item.Name, item.IsChecked ? CHECKBOX_CHECKED : CHECKBOX_UNCHECKED);
                    }

                }
            }
        }

        protected void AddFields(Common.Models.Files.PDF.PDFFields pdfFields)
        {

            AddRange(pdfFields.TextFields);
            AddRange(pdfFields.CheckBoxFields);
            AddRange(pdfFields.ChoiceFields);
            AddRange(pdfFields.SignatureFields);

        }

        protected void DeleteFields(Common.Models.Files.PDF.PDFFields pdfFields)
        {

            List<string> itemsToDelete = pdfFields.TextFields.Select(x => x.Name).ToList();
            RemoveFieldsFromPDF(itemsToDelete, DebenuFieldType.TextField);
            itemsToDelete = pdfFields.SignatureFields.Where(x => string.IsNullOrWhiteSpace(x.Image)).Select(x => x.Name).ToList();
            RemoveFieldsFromPDF(itemsToDelete, DebenuFieldType.SignatureField);
            itemsToDelete = pdfFields.CheckBoxFields.Select(x => x.Name).ToList();
            RemoveFieldsFromPDF(itemsToDelete, DebenuFieldType.CheckBoxField);
            itemsToDelete = pdfFields.ChoiceFields.Select(x => x.Name).ToList();
            RemoveFieldsFromPDF(itemsToDelete, DebenuFieldType.ChoiceField);

        }

        private void RadioFieldsModifyFields(List<RadioGroupField> inputRadioGoups, List<RadioGroupField> templeteRadioGoups)
        {
            HashSet<string> checkThisFieldsDic = new HashSet<string>();
            List<RadioGroupField> toAdd = new List<RadioGroupField>();

            foreach (var inputGroup in inputRadioGoups)
            {
                var group = templeteRadioGoups.FirstOrDefault(x => x.Name == inputGroup.Name);
                if (group != null)
                {
                    foreach (var inputRadio in inputGroup.RadioFields)
                    {
                        var radio = group.RadioFields.FirstOrDefault(x => x.Name == inputRadio.Name);
                        if (radio != null)
                        {
                            bool areLocationsAndSizesEqual = AreLocationsAndSizesEqual(radio, inputRadio, false);
                            if (!areLocationsAndSizesEqual)
                            {

                                checkThisFieldsDic.Add(group.Name);
                                toAdd.Add(inputGroup);

                                break;
                            }
                        }
                    }
                }
            }

            if (inputRadioGoups.Count > 0)
            {

                if (checkThisFieldsDic.Count > 0)
                {
                    RemoveFieldsFromPDF(checkThisFieldsDic.ToList(), DebenuFieldType.RadioGroupField);
                    AddRadioGroupRange(toAdd);
                }

                var fieldCount = _debenu.FormFieldCount();
                for (int i = 1; i <= fieldCount; i++)
                {


                    if (_debenu.GetFormFieldType(i) == (int)DebenuFieldType.RadioGroupField
                        && inputRadioGoups.FirstOrDefault(x => x.Name == _debenu.GetFormFieldTitle(i)) != null)
                    {
                        var inputGroup = inputRadioGoups.FirstOrDefault(x => x.Name == _debenu.GetFormFieldTitle(i));
                        string groupValue = string.IsNullOrWhiteSpace(inputGroup.SelectedRadioName) ? string.Empty : inputGroup.SelectedRadioName;
                        if (_debenu.GetFormFieldValue(i) != groupValue)
                        {
                            _debenu.SetFormFieldValue(i, inputGroup.SelectedRadioName);
                        }
                    }
                }
            }

        }


        private bool AreLocationsAndSizesEqual(Common.Models.Files.PDF.BaseField templateField, Common.Models.Files.PDF.BaseField frontField, bool needToValidateDescripton = true, double rationalPercentageDeviaton = 0.001)
        {
            bool y = Math.Abs(frontField.Y - templateField.Y) < rationalPercentageDeviaton;
            bool x = Math.Abs(frontField.X - templateField.X) < rationalPercentageDeviaton;
            bool width = Math.Abs(frontField.Width - templateField.Width) < rationalPercentageDeviaton;
            bool height = Math.Abs(frontField.Height - templateField.Height) < rationalPercentageDeviaton;
            bool page = templateField.Page == frontField.Page;
            bool mandaroty = templateField.Mandatory == frontField.Mandatory;
            bool description = needToValidateDescripton ? templateField.Description == frontField.Description : true;

            return y && x && width && height && page && mandaroty && description;

        }

        private PdfStamper CreateStamper(ref PdfReader reader, byte[] file, Stream os, char pdfVersion, bool append, out bool isFileCorrupted)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dependencyService = scope.ServiceProvider.GetService<IDebenuPdfLibrary>();
                try
                {
                    PdfStamper stamper = new PdfStamper(reader, os, pdfVersion, append);
                    isFileCorrupted = false;
                    return stamper;
                }
                catch 
                {
                    dependencyService.LoadFromString(file, "");
                    var workingFile = dependencyService.SaveToString();

                    reader = new PdfReader(workingFile);
                    PdfStamper stamper = new PdfStamper(reader, os, pdfVersion, false);
                    isFileCorrupted = true;
                    return stamper;
                }
            }


        }

        private byte[] TryAppendToString(int appendMode = 0)
        {

            byte[] file = _debenu.AppendToString(appendMode);
            using (MemoryStream outputMemoryStream = new MemoryStream())
            {
                try
                {
                    PdfReader reader = new PdfReader(file);
                    var stamper = CreateStamper(ref reader, file, outputMemoryStream, '\0', true, out bool isCorrupt);
                    stamper.Close();
                    reader.Close();

                    if (!isCorrupt)
                    {
                        return file;
                    }
                }
                catch 
                {
                    // do nothing
                }
                return _debenu.SaveToString();
            }
        }

        private byte[] GetDocumentCollectionAuditTraceBackgroundTemplate()
        {

            byte[] result = _memoryCache.Get<byte[]>("DocumentCollectionAuditTraceTemplate");
            if (result == null || result.Length == 0)
            {
                
                string currentFolder = _fileSystem.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string auditTraceTemplate = _fileSystem.Path.Combine(currentFolder, DOCUMENT_COLLECTION_AUDIT_TRACE_TEMPLATE);
                if (!_fileSystem.File.Exists(auditTraceTemplate))
                {
                    throw new Exception($"Document collection audit trace template not exist in system [{auditTraceTemplate}]");
                }
                result = _fileSystem.File.ReadAllBytes(auditTraceTemplate);
                _memoryCache.Set("DocumentCollectionAuditTraceTemplate", result, TimeSpan.FromMinutes(15));
            }
            return result;
        }

        #endregion
    }







}

