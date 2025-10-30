namespace Common.Models.Files.PDF
{
    using Common.Enums.PDF;

    public class TextField : BaseField
    {
        public TextFieldType TextFieldType { get; set; }
        public string CustomerRegex { get; set; } // case TextFiledType Custom
        public string Value { get; set; }
        public bool IsHidden { get; set; }
        public TextField() { }
        public TextField(BaseField baseField)
        {
            foreach (var prop in baseField.GetType().GetProperties())
            {
                var baseProp = GetType().GetProperty(prop.Name);
                baseProp.SetValue(this, prop.GetValue(baseField));
            }
        }
    }
}
