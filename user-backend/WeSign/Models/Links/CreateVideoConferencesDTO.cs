using Common.Models.Links;
using System.Collections.Generic;

namespace WeSign.Models.Links
{
    public class CreateVideoConferencesDTO
    {
        public string DocumentCollectionName { get; set; }
        public List<VideoConferenceUser> VideoConferenceUsers { get; set; }
    }
}
