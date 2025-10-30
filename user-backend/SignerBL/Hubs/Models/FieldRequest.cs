using Common.Enums.PDF;

namespace SignerBL.Hubs.Models
{
    public class FieldRequest
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public string FieldDescription { get; set; }
        public WeSignFieldType FieldType { get; set; }
    }
}
