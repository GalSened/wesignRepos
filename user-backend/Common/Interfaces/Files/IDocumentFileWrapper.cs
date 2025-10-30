using Common.Enums;
using Common.Models;
using Common.Models.Files.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Files
{
    public interface IDocumentFileWrapper
    {
        List<PdfImage> ReadAllImagesOfDocument(DocumentType documentType, Guid id);
        List<PdfImage> ReadImagesOfDocumentInRange(DocumentType documentType, Guid id, int startPage, int endPage);
        bool IsDocumentsImagesWasCreated(DocumentType documentType, Guid id);
        byte[] ReadDocument(DocumentType documentType, Guid id);
        bool IsDocumentExist(DocumentType documentType, Guid id);
        void CreateImagesFromList(DocumentType documentType, Guid id, List<PdfImage> pdfImages);
        (string HTML, string JS) ReadDocumentHTMLJs(DocumentType documentType, Guid id);
        void SaveDocument(DocumentType documentType, Guid id, byte[] data);
        void DeleteAllDocumentData(DocumentType documentType, Guid id);
        bool Duplicate(DocumentType documentType, Guid destinationId, Guid sourceId);
        void CreateImagesFromData(DocumentType documentType, Guid id, byte[] bytes);
        void SaveDocumentCopy(DocumentType documentType, Guid id);
        void CopyDocumentDataFromSource(DocumentType destinationDocumentType, Guid destinationId,
           DocumentType sourceDocumentType, Guid sourceId, bool imagesOnly);
        string GetFileNameWithoutExtension(string name);
        void SaveTemplateCustomHtml(Template template, byte[] htmlBytes, byte[] jsBytes);
        void DeleteAttachments(DocumentCollection documentsCollecteion);
        void DeleteAppendices(DocumentCollection documentsCollecteion);
    }
}
