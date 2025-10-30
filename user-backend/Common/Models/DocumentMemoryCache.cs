using Common.Models.Files.PDF;
using System.Collections.Generic;
using System.Linq;


namespace Common.Models
{
   public  class DocumentMemoryCache
    {
        private PDFFields _pdfFields;
        public int PageCount { get; set; }
        public List<PdfImage> Images { get; set; }
        public PDFFields pdfFields { get
            {

                var result = new PDFFields();
                result.TextFields = _pdfFields.TextFields != null && _pdfFields.TextFields.Count > 0 ? _pdfFields.TextFields.GetRange(0, _pdfFields.TextFields.Count) : new List<TextField>();
                result.SignatureFields = _pdfFields.SignatureFields != null && _pdfFields.SignatureFields.Count > 0 ? _pdfFields.SignatureFields.GetRange(0, _pdfFields.SignatureFields.Count) : new List<SignatureField>();
                result.CheckBoxFields = _pdfFields.CheckBoxFields != null && _pdfFields.CheckBoxFields.Count > 0 ? _pdfFields.CheckBoxFields.GetRange(0, _pdfFields.CheckBoxFields.Count) : new List<CheckBoxField>();
                //_pdfFields.ChoiceFields
                result.ChoiceFields = new List<ChoiceField>();
                foreach(var choiceField in _pdfFields?.ChoiceFields ??  Enumerable.Empty<ChoiceField>())
                {
                    var cf = new ChoiceField
                    {
                        Description = choiceField.Description,
                        Height = choiceField.Height,
                        Mandatory = choiceField.Mandatory,
                        Name = choiceField.Name,
                        Options = (string[])choiceField.Options.Clone(),
                        Page = choiceField.Page,
                        SelectedOption = choiceField.SelectedOption,
                        Width = choiceField.Width,
                        X = choiceField.X,
                        Y = choiceField.Y
                    };
                    result.ChoiceFields.Add(cf);
                }

                result.RadioGroupFields = new List<RadioGroupField>();
                foreach (var radioGroup in _pdfFields?.RadioGroupFields ?? Enumerable.Empty<RadioGroupField>())
                {
                    var rgf = new RadioGroupField
                    {
                        SelectedRadioName = radioGroup.SelectedRadioName,
                        RadioFields = (RadioField[])radioGroup.RadioFields.Clone(),
                        Name  = radioGroup.Name

                    };
                    result.RadioGroupFields.Add(rgf);
                }

                return result;



            }
            set
            {
                _pdfFields = value;
            }
                }

        
    }
}
