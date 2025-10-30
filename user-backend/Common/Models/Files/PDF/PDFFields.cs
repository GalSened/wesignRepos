namespace Common.Models.Files.PDF
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Fields")]
    public class PDFFields
    {
        [XmlElement]
        public List<TextField> TextFields { get; set; }
        [XmlElement]
        public List<SignatureField> SignatureFields { get; set; }
        [XmlElement]
        public List<RadioGroupField> RadioGroupFields { get; set; }
        [XmlElement]
        public List<CheckBoxField> CheckBoxFields { get; set; }
        [XmlElement]
        public List<ChoiceField> ChoiceFields { get; set; }

        public PDFFields()
        {
            TextFields = new List<TextField>();
            ChoiceFields = new List<ChoiceField>();
            CheckBoxFields = new List<CheckBoxField>();
            RadioGroupFields = new List<RadioGroupField>();
            SignatureFields = new List<SignatureField>();
        }

        public int? TotalFields()
        {

            var count =
                        TextFields?.Count() +
                        SignatureFields.Count() +
                        RadioGroupFields.Count() +
                        CheckBoxFields?.Count() +
                        ChoiceFields.Count();
            return count;

        }

        public void Merge(PDFFields metaDataFields)
        {
            ((List<TextField>)TextFields).AddRange(metaDataFields.TextFields);
            ((List<SignatureField>)SignatureFields).AddRange(metaDataFields.SignatureFields);
            ((List<RadioGroupField>)RadioGroupFields).AddRange(metaDataFields.RadioGroupFields);
            ((List<CheckBoxField>)CheckBoxFields).AddRange(metaDataFields.CheckBoxFields);
            ((List<ChoiceField>)ChoiceFields).AddRange(metaDataFields.ChoiceFields);
        }


        public PDFFields GetPDFFieldsByPage(int page, bool fetchSignedFields = true, bool fetchNotVisableSignatures = true)
        {
            var texts = TextFields.Where(x => x.Page == page);
            var choices = ChoiceFields.Where(x => x.Page == page);
            var checkBoxes = CheckBoxFields.Where(x => x.Page == page);
            var signatures = SignatureFields.Where(x => x.Page == page);
            if(!fetchSignedFields)
            {
                signatures = signatures.Where(x => string.IsNullOrWhiteSpace(x.Image));
            }
            if(!fetchNotVisableSignatures)
            {
                signatures = signatures.Where(x => 
                x.Width > 0 && x.Height > 0 && x.X >= 0 && x.Y <= 1);
            }

            List<RadioGroupField> radiosGroupList = new List<RadioGroupField>();

            foreach (var radioGroupField in RadioGroupFields)
            {
                if (radioGroupField.RadioFields.FirstOrDefault(x => x.Page == page) != null)
                {
                    var radiosGroup = radiosGroupList.FirstOrDefault(x => x.Name == radioGroupField.Name);
                    if (radiosGroup == null)
                    {
                        radiosGroup = new RadioGroupField()
                        {
                            Name = radioGroupField.Name,
                            SelectedRadioName = radioGroupField.SelectedRadioName,
                            RadioFields = new RadioField[1] { new RadioField() { Name = "" } }

                        };
                        radiosGroupList.Add(radiosGroup);
                    }
                    foreach(var radio  in radioGroupField.RadioFields.Where(x => x.Page == page))
                    {
                        var items = radiosGroup.RadioFields.ToList();
                        items.Add(radio);
                        radiosGroup.RadioFields = items.ToArray();
                    }
                    radiosGroup.RadioFields = radiosGroup.RadioFields.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToArray();

                }
                
            }
            
            var result = new PDFFields
            {
                TextFields = texts.ToList(),
                ChoiceFields = choices.ToList(),
                CheckBoxFields = checkBoxes.ToList(),
                RadioGroupFields = radiosGroupList,
                SignatureFields = signatures.ToList()
            };
            return result;
        }
    }
}
