namespace WeSign.Models.Templates
{
    public class MergeTemplatesDTO
    {
        public string[] Templates { get; set; }
        public string Name { get; set; }        
        public bool IsOneTimeUseTemplate { get; set; }
    }
}
