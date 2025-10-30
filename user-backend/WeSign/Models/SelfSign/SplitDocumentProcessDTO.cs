using Common.Enums.Documents;

namespace WeSign.Models.SelfSign
{
    public class SplitDocumentProcessDTO
    {
        public SplitSignProcessStep ProcessStep { get; set; }
        public string Url { get; set; }
    }
}
