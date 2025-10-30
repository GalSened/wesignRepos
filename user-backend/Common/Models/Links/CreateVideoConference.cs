using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Links
{
    public class CreateVideoConference
    {
        public List<VideoConferenceUser> VideoConferenceUsers { get; set; }
        public string DocumentCollectionName { get; set; }
    }
}
