namespace Common.Interfaces.PDF
{
    using Common.Enums;
    using Common.Models;
    using Common.Models.Files.PDF;
    using Common.Models.XMLModels;
    using System;
    using System.Collections.Generic;

    public interface ITemplatePdf
    {
        IList<PdfImage> Images { get; }
        
        IExtendedList<SignatureField> SignatureFields { get; }
        IExtendedList<TextField> TextFields { get; }
        IExtendedList<CheckBoxField> CheckBoxFields { get; }
        IExtendedList<ChoiceField> ChoiceFields { get; }
        IExtendedList<RadioGroupField> RadioGroupFields { get; }
        IList<FieldCoordinate> GetPlaceholdersFromPdf(string parenthesis = null, string placeholdercolor = null);
        PdfImage GetPdfImageByIndex(int page,  Guid id);
        IList<PdfImage> GetPdfImages(int startPage, int endPage, Guid id);
        bool Load(Guid id, bool shouldLoadToMemory = true);
        void SetId(Guid id);
        bool SaveDocument();
        bool Create(Guid id, string base64file);
        bool Create(Guid id, byte[] file, bool IsDuplicate,string directory = "");
        bool Duplicate(Guid newId);
        bool Delete();
        byte[] Download();
        PDFFields GetAllFields(bool includeSignatureImages = true);
        PDFFields GetAllFields(int startPage, int endPage, bool includeSignatureImages = true);
        PDFFields GetAllFieldsInRange(int startPage, int endPage, PDFFields fields);
        void SetAllSignatureFieldsMandatory();
        void DoUpdateFields(PDFFields templatePdfFields);
        

        /// <summary>
        /// <para>Get all fields from startPage (include) to endPage (inculde)</para>
        /// <para>If you need single page you need to set startPage and endPage with same value </para>
        /// <para>For example: if you want page 1 so call function like: GetAllFields(1,1) </para>
        /// </summary>
        /// <param name="startPage"></param>
        /// <param name="endPage"></param>
        /// <returns></returns>
      
      
        string ConvertImageToPdf(string base64);
        int GetPagesCount();
        void EmbadTextDataFields(List<TextField> textFields, List<ChoiceField> choiceFields);
     
        bool AddTextFields( List<TextField> textFields);
        Guid GetId();
        (string HTML, string JS ) GetHtmlTemplate();
        void CreateImagesFromPdfInFileSystem();

        DocumentType GetDocumentType();
        




    }
}
