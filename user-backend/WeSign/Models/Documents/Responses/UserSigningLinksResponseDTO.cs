using Common.Enums.Documents;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;

namespace WeSign.Models.Documents.Responses
{
    public class UserSigningLinksResponseDTO
    {
        public IEnumerable<UserSigningLinkResponseDTO> DocumentCollections { get; set; }
        
        public UserSigningLinksResponseDTO(IEnumerable<(DocumentCollection DocumentCollection, string SigningLink)> links)
        {
            var result = new List<UserSigningLinkResponseDTO>();
            links.ForEach(x =>
            {
                result.Add(new UserSigningLinkResponseDTO
                {
                    Name = x.DocumentCollection.Name,
                    CreationTime = x.DocumentCollection.CreationTime,
                    DocumentCollectionId = x.DocumentCollection.Id,
                    Mode = x.DocumentCollection.Mode,
                    SigningLink = x.SigningLink,
                    DocumentsIds = x.DocumentCollection.Documents.Select(_ => _.Id),
                    Status = x.DocumentCollection.DocumentStatus
                });
            });
            DocumentCollections = result;
        }

    }

    public class UserSigningLinkResponseDTO
    {
        public Guid DocumentCollectionId { get; set; }
        public string Name { get; set; }
        public DocumentMode Mode { get; set; }
        public DateTime CreationTime { get; set; }
        public string SigningLink { get; set; }
        public IEnumerable<Guid> DocumentsIds { get; set; }
        public DocumentStatus Status { get; set; }
    }
}
