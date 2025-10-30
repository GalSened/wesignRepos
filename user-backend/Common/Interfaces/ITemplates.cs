namespace Common.Interfaces
{
    using Common.Models;
    using Common.Models.Documents;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITemplates
    {
        Task Create(Template template);
        Task Update(Template template);
        Task Delete(Template template);
        Task<(IEnumerable<Template>,int)> Read(string key, string from, string to, int offset, int limit, bool popular, bool recent);
        Task<Template> DuplicateTemplate(Template template);
        Task<Template> GetPagesCountByTemplateId(Template template);
        Task GetPageByTemplateId(Template template, int page);
        Task GetPagesByTemplateId(Template template, int offset, int limit);
        Task<Template> GetTemplateByTemplateId(Template template);
        Task<(string name, byte[] content)> Download(Template template);
        Task<bool> IsTemplateInUse(Template template);
        IEnumerable<Template> ReadDeletedTemplates();
        Task DeleteBatch(RecordsBatch templatesBatch);
        Task<Template> MergeTemplates(MergeTemplates mergeTemplates);
        Task<string> GetOcrHtmlFromImage(string base64Image);
    }
}
