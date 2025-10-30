namespace WeSign.Models.SelfSign.Responses
{
    using System;

    public class SelfSignCountResponseDTO
    {
        public Guid DocumentCollectionId { get; set; }
        public Guid DocumentId { get; set; }
        public string Name { get; set; }
        public int PagesCount { get; set; }
    }
}
