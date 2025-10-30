namespace WeSign.Models.Documents.Responses
{
    using System.Collections.Generic;       

    public class AllDocumentCollectionsResposneDTO
    {
        public IEnumerable<DocumentCollectionResposneDTO> DocumentCollections { get; set; }
    }
}
