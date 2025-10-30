using Common.Enums.Documents;

namespace WeSignSigner.Models.Responses
{
    public class SplitDocumentProcessDTO
    {
        public SplitSignProcessStep ProcessStep { get;  set; }
        public string Url { get;  set; }
    }
}
