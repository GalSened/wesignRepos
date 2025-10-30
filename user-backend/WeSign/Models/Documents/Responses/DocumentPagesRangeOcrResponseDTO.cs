using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Documents.Responses
{
    public class DocumentPagesRangeOcrResponseDTO
    {
        public List<DocumentPageOcrResponseDTO> DocumentPages { get; set; }

        public DocumentPagesRangeOcrResponseDTO()
        {
            DocumentPages = new List<DocumentPageOcrResponseDTO>();
        }

      
    }
}
