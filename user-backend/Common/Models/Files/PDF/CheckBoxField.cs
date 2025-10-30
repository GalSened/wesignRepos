namespace Common.Models.Files.PDF
{
    public class CheckBoxField : BaseField
    {
        public bool IsChecked { get; set; }

        public CheckBoxField() { }
        public CheckBoxField(BaseField baseField)
        {
            foreach (var prop in baseField.GetType().GetProperties())
            {
                var baseProp = GetType().GetProperty(prop.Name);
                baseProp.SetValue(this, prop.GetValue(baseField));
            }
        }
    }
}
