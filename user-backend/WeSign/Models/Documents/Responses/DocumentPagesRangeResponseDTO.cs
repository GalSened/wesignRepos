using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Documents.Responses
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
