using System.Collections.Generic;

namespace WeSignSigner.Models.Responses
{
    public class DocumentPagesRangeResponseDTO
    {
        public List<DocumentPageResponseDTO> DocumentPages { get; set; }

        public DocumentPagesRangeResponseDTO()
        {
            DocumentPages = new List<DocumentPageResponseDTO>();
        }
    }
}
