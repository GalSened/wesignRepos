namespace Common.Interfaces.DB
{
    using Common.Enums.PDF;
    using Common.Models;
    using Common.Models.Files.PDF;
    using Common.Models.ManagementApp.Reports;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITemplateConnector
    {
        Task Create(Template template);
        Task Delete(Template template);
        Task Delete(List<Template> templates);
        
        Task Delete(List<Template> templates, Action<List<Template>> deleteTemplateFromFS);

        IEnumerable<Template> Read(User user, string key, string from, string to, int offset, int limit, bool popular, bool recent, out int totalCount);
        Task<Template> Read(Template template);
        IEnumerable<Template> Read(List<Guid> Ids);
        IEnumerable<Template> ReadWithFieldsData(List<Guid> Ids);
        IEnumerable<Template> Read(Group group);
        IEnumerable<Template> ReadDeletedTemplates();
        IEnumerable<TemplatesByUsageReport> ReadTemplatesByUsage(Guid companyId, IEnumerable<Guid> groupIds, DateTime from, DateTime to, int offset, int limit, out int totalCount);
        IEnumerable<Template> ReadDeletedTemplatesWithoutConnectedDocuments();
        IEnumerable<Template> ReadOneTimeTemplatesWithoutConnectedDocuments();
        IEnumerable<Guid> ReadAllIds();
        bool ReadTemplateSignatureFieldMandatory(string fieldName);
        Task<bool> Exists(Template template);
        Task Update(Template template);
        Task UpdateLastUsed(Template template);
    
        IEnumerable<Template> ReadOneTimeTemplates();
        Task IncrementUseCount(Template template, int delta);
        IEnumerable<TextField> GetTextFieldsByType(Template template, TextFieldType textFieldType);
      
        List<Template> GetUnusedTemplates(DateTime dateTime);
        Task RemoveAllOneTimeTemplatesFromCollectionGroup(DocumentCollection documentGroup);
        Task<bool> ExistNotDeletedTemplatesInGroup(Group group);
        Task DeleteUnDeletedTemplatesInGroup(Group group);
        Task DeleteUnusedTemplates(DateTime dateTime);


        List<SingleLinkAdditionalResource> ReadSingleLink(Template template);
        Task UpdateCreateSingleLink(TemplateSingleLink resource);

    }
}
