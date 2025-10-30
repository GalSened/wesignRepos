using Common.Models.Files.PDF;
using System.Collections.Generic;

namespace WeSignSigner.Models.Responses
{
    public class DocumentCollectionHtmlDataResponseDTO
    {
        public string HtmlContent { get; set; }
        public string JsContent { get; internal set; }
        public IEnumerable<FieldData> FieldsData { get; set; }
    }
}
