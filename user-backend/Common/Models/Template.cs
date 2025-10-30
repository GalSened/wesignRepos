namespace Common.Models
{
    using Common.Enums;
    using Common.Enums.Templates;
    using Common.Models.Files.PDF;
    using System;
    using System.Collections.Generic;

    public class Template
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public IList<PdfImage> Images { get; set; }
        public PDFFields Fields { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdatetime { get; set; }
        public DateTime LastUsedTime { get; set; }
        public int UsedCount { get; set; }
        public string Base64File { get; set; }
        public FileType FileType { get; set; }

        public string MetaData { get; set; } = string.Empty;
        public TemplateStatus Status { get; set; }

        public Template()
        {
            Fields = new PDFFields();
         
    }
    }
}