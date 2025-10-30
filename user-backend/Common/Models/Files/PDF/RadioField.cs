namespace Common.Models.Files.PDF
{
    public class RadioField : BaseField
    {
        public string Value { get; set; }

        public RadioField() { }
        public RadioField(BaseField baseField)
        {
            foreach (var prop in baseField.GetType().GetProperties())
            {
                var baseProp = GetType().GetProperty(prop.Name);
                baseProp.SetValue(this, prop.GetValue(baseField));
            }
        }
    }
}
