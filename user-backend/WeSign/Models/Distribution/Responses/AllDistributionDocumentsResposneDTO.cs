using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSign.Models.Users.Responses;

namespace WeSign.Models.Distribution.Responses
{
    public class AllDistributionDocumentsResposneDTO
    {
        public IEnumerable<DistributionDocumentResposneDTO> DocumentCollections { get; set; }
    }

    public class DistributionDocumentResposneDTO
    {
        public Guid DocumentCollectionId { get; set; }
        public Guid DistributionId { get; }
        public string Name { get; set; }        
        public DateTime CreationTime { get; set; }        
        public bool IsWillDeletedIn24Hours { get; set; }
        public string User { get; set; }        
        public DistributionDocumentResposneDTO() { }

        public DistributionDocumentResposneDTO(DocumentCollection documentCollection)
        {
            if (documentCollection != null)
            {
                DocumentCollectionId = documentCollection.Id;
                DistributionId = documentCollection.DistributionId;
                Name = documentCollection.Name;                
                CreationTime = documentCollection.CreationTime;                
                IsWillDeletedIn24Hours = documentCollection.IsWillDeletedIn24Hours;
                User = documentCollection.User.Name;
            }
        }
    }
}
