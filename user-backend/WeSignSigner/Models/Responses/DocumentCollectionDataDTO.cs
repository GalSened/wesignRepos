using Common.Enums.Documents;
using System;

namespace WeSignSigner.Models.Responses
{
    public class DocumentCollectionDataDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DocumentMode Mode { get; set; }
    }
}
