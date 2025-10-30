using System;

namespace WeSignSigner.Models.Responses
{
    public class DocumentCountResponseDTO
    {
        public Guid Id { get; set; }
        public int PagesCount { get; set; }
    }
}
