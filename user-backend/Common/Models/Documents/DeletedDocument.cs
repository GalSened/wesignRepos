using System;

namespace Common.Models.Documents
{
    public class DeletedDocument
    {
        public Guid Id { get; set; }
        public DeletedDocumentTemplate Template { get; set; }

        public DeletedDocument(Document document, DeletedDocumentTemplate template)
        {
            Id = document.Id;
            Template = template;
        }
    }
}
