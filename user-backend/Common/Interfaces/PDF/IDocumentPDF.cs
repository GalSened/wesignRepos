namespace Common.Interfaces.PDF
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Models.Configurations;
    using Common.Models.Documents.Signers;
    using Common.Models.Files.PDF;
    using Common.Models.Files.Pfx;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    public interface IDocumentPdf
    {
        IList<PdfImage> Images { get; }
        IExtendedList<SignatureField> SignatureFields { get; }
        IExtendedList<TextField> TextFields { get; }
        IExtendedList<CheckBoxField> CheckBoxFields { get; }
        IExtendedList<ChoiceField> ChoiceFields { get; }
        IExtendedList<RadioGroupField> RadioGroupFields { get; }

        void EmbadTextDataFields(List<Common.Models.Files.PDF.TextField> textFields, List<ChoiceField> choiceFields);
        bool Load(Guid id, bool shouldLoadToMemory = true);
        bool SaveDocument();               
        bool Create(Guid id, byte[] file, bool formExistTemplate, bool shouldLoadToMemory = true);
        bool Delete();
        bool CreateImagesFromExternalList( IList<PdfImage> images);
        
        Task Sign(SigningInfo SigningInfo, bool isServerWithoutFields = false, bool useForAllFields = false);
      

        void SetAllFieldsToReadOnly();
        void DoUpdateFields(PDFFields templatePdfFields);


        /// <summary>
        /// <para>Get all fields from startPage (include) to endPage (inculde)</para>
        /// <para>If you need single page you need to set startPage and endPage with same value </para>
        /// <para>For example: if you want page 1 so call function like: GetAllFields(1,1) </para>
        /// </summary>
        /// <param name="startPage"></param>
        /// <param name="endPage"></param>
        /// <returns></returns>
        PDFFields GetAllFields(int startPage, int endPage, bool includeSignatureImages = true);
        PDFFields GetAllFields(bool includeSignatureImages = true);
        PDFFields GetAllFieldsWithoutSigFields();
        PDFFields GetAllFieldsInRange(int startPage, int endPage, PDFFields fields);
        

     
        bool IsExists(IBaseField field );
        int GetPagesCount();
        PdfImage GetPdfImageByIndex(int page, Guid id);
        IList<PdfImage> GetPdfImages(int startPage, int endPage, Guid id);
        byte[] CreateTraceFile(List<string> result);
        byte[] CreateTraceFile(DocumentCollectionAuditTrace documentCollectionAuditTrace, DocumentMode mode);


        Guid GetId();
        void SetId(Guid id);

        void SaveCopy(Guid id);
        Task VerifySigner1Credential(SignerAuthentication input, CompanySigner1Details companySigner1Details = null);
        DocumentType GetDocumentType();
        void CopyPagesImagesFromTemplate(Guid origTemplateId);
        void CreateImages();
        void CreateFromExistingTemplate(Guid sourceTemplateId);
    }
}
