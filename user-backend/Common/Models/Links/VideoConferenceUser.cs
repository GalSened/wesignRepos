using Common.Enums.Documents;

namespace Common.Models.Links
{
    public class VideoConferenceUser
    {
        public string Means { get; set; }
        public string FullName { get; set; }
        public SendingMethod SendingMethod{ get; set; }
        public string PhoneExtension{ get; set; }
}
}
