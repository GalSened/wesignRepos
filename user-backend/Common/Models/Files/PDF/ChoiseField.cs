namespace Common.Models.Files.PDF
{
    public class ChoiceField : BaseField
    {
        public string[] Options { get; set; }
        public string SelectedOption { get; set; }

        public ChoiceField() { }
        public ChoiceField(BaseField baseField)
        {
            foreach (var prop in baseField.GetType().GetProperties())
            {
                var baseProp = GetType().GetProperty(prop.Name);
                baseProp.SetValue(this, prop.GetValue(baseField));
            }
        }
    }
}
