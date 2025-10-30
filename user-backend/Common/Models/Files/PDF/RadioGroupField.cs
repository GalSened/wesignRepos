namespace Common.Models.Files.PDF
{
    public class RadioGroupField
    {
        public string Name { get; set; }
        public RadioField[] RadioFields { get; set; }
        public string SelectedRadioName { get; set; }
        //TODO add IsMandatory in group level , and if it true, all inner RadioGroup Mandatory also
    }
}
