namespace WeSign.Models.Templates
{
    using Common.Enums;

    public class CreateTemplateDTO
    {
        public string Base64File { get; set; }
        public string Name { get; set; }
        public string MetaData { get; set; }
        public bool IsOneTimeUseTemplate { get; set; }        
    }
}
