using Common.Enums;
using Common.Interfaces.Files;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Utilities;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace Common.Handlers.Files.Local
{
    public class LocalDocumentFileWrapperHandler : IDocumentFileWrapper
    {
        private readonly FolderSettings _folderSettings;
        private readonly IFileSystem _fileSystem;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GeneralSettings _generalSettings;
        private const string JPG = "jpeg";
        private const string PrevVersionsBackup = "PrevVersionsBackup";


        public LocalDocumentFileWrapperHandler(IOptions<FolderSettings> folderSettings, IOptions<GeneralSettings> generalSettings,
        IFileSystem fileSystem, IServiceScopeFactory scopeFactory)
        {
            _folderSettings = folderSettings.Value;
            _fileSystem = fileSystem;
            _scopeFactory = scopeFactory;
            _generalSettings = generalSettings.Value;
        }



        public void DeleteAttachments(DocumentCollection documentsCollecteion)
        {
            foreach (var signer in documentsCollecteion?.Signers ?? new List<Signer>())
            {

                string folderPath = _fileSystem.Path.Combine(_folderSettings?.Attachments, signer.Id.ToString());
                if (_fileSystem.Directory.Exists(folderPath))
                {
                    _fileSystem.Directory.Delete(folderPath, true);
                }
            }
        }

        public void DeleteAppendices(DocumentCollection documentsCollecteion)
        {
            string folderPath = _fileSystem.Path.Combine(_folderSettings?.Appendices, documentsCollecteion.Id.ToString());
            if (_fileSystem.Directory.Exists(folderPath))
            {
                _fileSystem.Directory.Delete(folderPath, true);
            }


        }
        public string GetFileNameWithoutExtension(string name)
        {
            return _fileSystem.Path.GetFileNameWithoutExtension(name); 
        }

        public void SaveDocumentCopy(DocumentType documentType, Guid id)
        {
            string directoryPath = GetRootPath(documentType);
            string sourceDirectoryPath = _fileSystem.Path.Combine(directoryPath, $"{id}");
            string copyDirectoryPath = _fileSystem.Path.Combine(sourceDirectoryPath, $"{PrevVersionsBackup}");
            if (!_fileSystem.Directory.Exists(copyDirectoryPath))
            {
                _fileSystem.Directory.CreateDirectory(copyDirectoryPath);
            }
            _fileSystem.File.WriteAllBytes(_fileSystem.Path.Combine(copyDirectoryPath, $"{id}_{DateTime.UtcNow.ToString("dd_MM_YYYY-hh_mm_tt")}.pdf"),
                _fileSystem.File.ReadAllBytes(_fileSystem.Path.Combine(sourceDirectoryPath, $"{id}.pdf")));


        }
        public void CreateImagesFromData(DocumentType documentType, Guid id, byte[] data)
        {

            List<IServiceScope> serviceScopes = new List<IServiceScope>();
            try
            {
                var scope = _scopeFactory.CreateScope();
                serviceScopes.Add(scope);
                string directoryPath = GetRootPath(documentType);
                string sourceDirectoryPath = _fileSystem.Path.Combine(directoryPath, $"{id}");
                var debenu = scope.ServiceProvider.GetService<IDebenuPdfLibrary>();
                if (debenu.LoadFromString(data, "") != 0)
                {
                    var pages = debenu.PageCount();
                    var renderTasks = 1;
                    if (pages >= 50)
                        renderTasks = 4;
                    else if (pages > 5)
                        renderTasks = 2;

                    var createDocTask = new Task[renderTasks];
                    var startpage = 1;
                    var endpage = Convert.ToInt32(Math.Round((float)(pages / renderTasks), MidpointRounding.ToEven));
                    var lastEndPage = endpage;
                    createDocTask[0] = Task.Run(() => debenu.RenderDocumentToFile(_generalSettings.DPI, startpage, endpage, 1,
                        _fileSystem.Path.Combine(sourceDirectoryPath, $"{id}_%p.{JPG}")));
                    for (double i = 1; i < renderTasks; i++)
                    {
                        var externalScope = _scopeFactory.CreateScope();
                        serviceScopes.Add(externalScope);
                        var dependencyService = scope.ServiceProvider.GetService<IDebenuPdfLibrary>();
                        dependencyService.LoadFromString(data, "");
                        RemoveAllFieldsFromLoadedPDF(dependencyService);
                        var startpageTask = 1 + lastEndPage;
                        var endpageTask = Convert.ToInt32(Math.Round(pages * ((i + 1) / renderTasks), MidpointRounding.ToEven));
                        lastEndPage = endpageTask;

                        createDocTask[(int)i] = Task.Run(() => dependencyService.RenderDocumentToFile(_generalSettings.DPI, startpageTask, endpageTask, 1
                            , _fileSystem.Path.Combine(sourceDirectoryPath, $"{id}_%p.{JPG}")));
                    }
                    try
                    {
                        Task.WaitAll(createDocTask);
                    }
                    catch 
                    {
                        debenu.RenderDocumentToFile(_generalSettings.DPI, 1, pages, 1, _fileSystem.Path.Combine(sourceDirectoryPath, $"{id}_%p.{JPG}"));
                    }
                }
            }
            finally
            {
                if (serviceScopes.Count > 0)
                {
                    foreach (var scope in serviceScopes)
                    {
                        scope.Dispose();
                    }
                }
            }

        }

        public bool Duplicate(DocumentType documentType, Guid destinationId, Guid sourceId)
        {

            CopyDocumentDataFromSource(documentType, destinationId, documentType, sourceId, false);

            return true;

        }

        public void CopyDocumentDataFromSource(DocumentType destinationDocumentType, Guid destinationId,
            DocumentType sourceDocumentType, Guid sourceId, bool imagesOnly)
        {
            string sourceDirectoryPath = _fileSystem.Path.Combine(GetRootPath(sourceDocumentType), $"{sourceId}");
            string destinationDirectoryPath = _fileSystem.Path.Combine(GetRootPath(destinationDocumentType), $"{destinationId}");
            if (!_fileSystem.Directory.Exists(destinationDirectoryPath))
            {
                _fileSystem.Directory.CreateDirectory(destinationDirectoryPath);
            }
            var files = imagesOnly ? GetPicturesImagesPath(sourceDirectoryPath) :
                                      _fileSystem.Directory.EnumerateFiles(sourceDirectoryPath);
            foreach (var file in files)
            {
                string fileName = _fileSystem.Path.GetFileName(file);
                fileName = fileName.Replace($"{sourceId}", $"{destinationId}");
                _fileSystem.File.Copy(file, _fileSystem.Path.Combine(destinationDirectoryPath, fileName));

            }
        }

        public void SaveDocument(DocumentType documentType, Guid id, byte[] data)
        {

            string directoryPath = GetRootPath(documentType);
            directoryPath = _fileSystem.Path.Combine(directoryPath, $"{id}");
            if (!_fileSystem.Directory.Exists(directoryPath))
            {
                _fileSystem.Directory.CreateDirectory(directoryPath);
            }
            string filePath = _fileSystem.Path.Combine(directoryPath, $"{id}.pdf");
            _fileSystem.File.WriteAllBytes(filePath, data);
        }

        public void CreateImagesFromList(DocumentType documentType, Guid id, List<PdfImage> pdfImages)
        {
            string directoryPath = GetRootPath(documentType);
            directoryPath = _fileSystem.Path.Combine(directoryPath, $"{id}");

            Parallel.For(1, pdfImages.Count() + 1, (i) =>
            {
                string fileName = _fileSystem.Path.Combine(directoryPath, $"{id}_{i}" + JPG);
                _fileSystem.File.WriteAllBytes(fileName, Convert.FromBase64String(pdfImages[i - 1].Base64Image));

            });
        }

        public void DeleteAllDocumentData(DocumentType documentType, Guid id)
        {

            string directoryPath = GetRootPath(documentType);
            directoryPath = _fileSystem.Path.Combine(directoryPath, $"{id}");
            if (_fileSystem.Directory.Exists(directoryPath))
            {
                _fileSystem.Directory.Delete(directoryPath, true);
            }
        }
        public byte[] ReadDocument(DocumentType documentType, Guid id)
        {
            byte[] result = null;
            if (IsDocumentExist(documentType, id))
            {
                string documentFilePath = GetRootPath(documentType);
                documentFilePath = _fileSystem.Path.Combine(documentFilePath, $"{id}", $"{id}.pdf");
                result = _fileSystem.File.ReadAllBytes(documentFilePath);
            }
            return result;
        }

        public bool IsDocumentExist(DocumentType documentType, Guid id)
        {
            string documentFilePath = GetRootPath(documentType);
            documentFilePath = _fileSystem.Path.Combine(documentFilePath, $"{id}", $"{id}.pdf");
            return _fileSystem.File.Exists(documentFilePath);

        }

        public (string HTML, string JS) ReadDocumentHTMLJs(DocumentType documentType, Guid id)
        {
            string documentFilePath = GetRootPath(documentType);
            documentFilePath = _fileSystem.Path.Combine(documentFilePath, $"{id}");
            string html = string.Empty;
            string js = string.Empty;
            if (_fileSystem.Directory.Exists(documentFilePath))
            {

                var files = _fileSystem.Directory.GetFiles(documentFilePath, "*.html");
                if (files.Any())
                {
                    html = _fileSystem.File.ReadAllText(files.FirstOrDefault());
                }
                var jsFiles = _fileSystem.Directory.GetFiles(documentFilePath, "*.js");
                if (jsFiles.Any())
                {
                    js = _fileSystem.File.ReadAllText(jsFiles.FirstOrDefault());
                }

            }
            return (html, js);
        }
        public void SaveTemplateCustomHtml(Template template, byte[] htmlBytes, byte[] jsBytes)
        {
            string htmlFilePath = _fileSystem.Path.Combine(_folderSettings.Templates, template.Id.ToString(), $"{template.Id}.html");
            string jsFilePath = _fileSystem.Path.Combine(_folderSettings.Templates, template.Id.ToString(), $"{template.Id}.js");
            _fileSystem.File.WriteAllBytes(htmlFilePath, htmlBytes);
            _fileSystem.File.WriteAllBytes(jsFilePath, jsBytes);
        }
        public bool IsDocumentsImagesWasCreated(DocumentType documentType, Guid id)
        {
            string directoryPath = GetRootPath(documentType);
            directoryPath = _fileSystem.Path.Combine(directoryPath, $"{id}");
            var picturesPath = GetPicturesImagesPath(directoryPath);
            return picturesPath.Any();
        }

        public List<PdfImage> ReadImagesOfDocumentInRange(DocumentType documentType, Guid id, int startPage, int endPage)
        {
            List<PdfImage> pdfImages = new List<PdfImage>();
            string directoryPath = GetRootPath(documentType);
            directoryPath = _fileSystem.Path.Combine(directoryPath, $"{id}");
            var picturesPath = GetPicturesImagesPath(directoryPath).ToList();
            if (picturesPath.Any())
            {
                for (int i = startPage - 1; i < endPage - 1; i++)
                {
                    pdfImages.Add(GetPdfImageByPath(picturesPath[i]));
                }
            }
            return pdfImages;
        }

        public List<PdfImage> ReadAllImagesOfDocument(DocumentType documentType, Guid id)
        {
            List<PdfImage> pndImagesResult = new List<PdfImage>();
            string directoryPath = GetRootPath(documentType);
            directoryPath = _fileSystem.Path.Combine(directoryPath, $"{id}");
            var picturesPath = GetPicturesImagesPath(directoryPath).ToList();
            if (picturesPath.Any())
            {

                pndImagesResult = picturesPath.Select(picture =>
                {
                    return GetPdfImageByPath(picture);
                }).ToList();

            }



            return pndImagesResult;
        }



        private PdfImage GetPdfImageByPath(string imagePath)
        {
            using (var mem = new MemoryStream(_fileSystem.File.ReadAllBytes(imagePath)))
            {
                using (var img = Image.FromStream(mem))
                {
                    return new PdfImage
                    {
                        Base64Image = Convert.ToBase64String(mem.ToArray()),
                        Height = img.Height,
                        Width = img.Width
                    };
                }
            }
        }

        protected IEnumerable<string> GetPicturesImagesPath(string directoryPath)
        {
            var images = _fileSystem.Directory.EnumerateFiles(directoryPath)
                            .Where(x => _fileSystem.Path.GetExtension(x) == "." + JPG).ToList();

            if (images.Any())
            {
                images.Sort((x, y) =>
                                {
                                    return FileNameCompare(x, y);
                                });
            }
            return images;
        }

        private int FileNameCompare(string x, string y)
        {
            var xPath = _fileSystem.Path.GetFileNameWithoutExtension(x);
            var yPath = _fileSystem.Path.GetFileNameWithoutExtension(y);
            int.TryParse(xPath.Split('_')[1], out int xOut);
            int.TryParse(yPath.Split('_')[1], out int yOut);

            return xOut.CompareTo(yOut);
        }

        private void RemoveAllFieldsFromLoadedPDF(IDebenuPdfLibrary pdfLibrary)
        {
            if (_generalSettings.ShouldDeleteFormFieldsBeforeCreateImage)
            {
                while (pdfLibrary.FormFieldCount() > 0)       //Remove all Fields from PDF file
                {
                    pdfLibrary.SetNeedAppearances(-1);
                    pdfLibrary.DeleteFormField(1);
                }
            }
        }

        private string GetRootPath(DocumentType documentType)
        {
            string rootPath = "";
            if (documentType == DocumentType.Template)
            {
                rootPath = _folderSettings.Templates;
            }
            else
            {
                rootPath = _folderSettings.Documents;
            }

            return rootPath;
        }

    }
}
