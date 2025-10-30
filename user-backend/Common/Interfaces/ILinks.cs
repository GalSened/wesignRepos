using Common.Models;
using Common.Models.Links;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ILinks
    {
        Task<VideoConferenceResult> CreateVideoConference(CreateVideoConference createVideoConferences);
        Task<(IEnumerable<(DocumentCollection DocumentCollection, string SigningLink)>, int)> Read(string key, int offset, int limit);
        Task<TemplateSingleLink> GetSingleLinkInfo(Template template);
        Task UpdateCreateSingleLinkInfo(TemplateSingleLink templateSingleLink);
    }
}
