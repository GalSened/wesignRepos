using Common.Enums.PDF;

namespace Common.Models.Files.PDF
{
    public class FieldData
    {
        public string Name { get; set; }
        public WeSignFieldType Type { get; set; }
        public TextFieldType TextFieldType { get; set; }
        public string  Value { get; set; }
    }
}
