using Common.Enums.PDF;

namespace WeSignSigner.Models.Requests
{
    public class FieldDTO
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public WeSignFieldType FieldType { get; set; }
    }
}
