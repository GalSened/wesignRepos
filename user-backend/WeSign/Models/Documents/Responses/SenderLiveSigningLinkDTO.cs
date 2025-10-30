namespace WeSign.Models.Documents.Responses
{
    public class SenderLiveSigningLinkDTO
    {
        public string link { get; set; }

        public SenderLiveSigningLinkDTO(string link)
        {
            this.link = link;
        }
    }
}
