using Common.Enums.PDF;
using Common.Enums.Results;
using Common.Extensions;
using Common.Models.Files.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common.Models.XMLModels
{

    [XmlRoot(ElementName = "Field")]
    public class Field
    {
        [XmlAttribute(AttributeName = "Type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "Fieldname")]
        public string Fieldname { get; set; }
        [XmlAttribute(AttributeName = "IsChecked")]
        public string IsChecked { get; set; }
        [XmlAttribute(AttributeName = "Value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "IsMandatory")]
        public string IsMandatory { get; set; }
        [XmlAttribute(AttributeName = "IsSelected")]
        public string IsSelected { get; set; }
        [XmlAttribute(AttributeName = "Option")]
        public string Option { get; set; }
        [XmlAttribute(AttributeName = "Description")]
        public string Description { get; set; }
    }

    [XmlRoot(ElementName = "Fields")]
    public class Fields
    {
        [XmlElement(ElementName = "Field")]
        public List<Field> Field { get; set; }
        [XmlElement(ElementName = "GroupField")]
        public List<GroupField> GroupField { get; set; }
    }

    [XmlRoot(ElementName = "GroupField")]
    public class GroupField
    {
        [XmlElement(ElementName = "Field")]
        public List<Field> Field { get; set; }
        [XmlAttribute(AttributeName = "Type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "GroupName")]
        public string GroupName { get; set; }
        [XmlAttribute(AttributeName = "Fieldname")]
        public string Fieldname { get; set; }
        [XmlAttribute(AttributeName = "IsMandatory")]
        public string IsMandatory { get; set; }
        [XmlAttribute(AttributeName = "Description")]
        public string Description { get; set; }
    }

    [XmlRoot(ElementName = "PDFMetaData")]
    public class PDFMetaData
    {
        [XmlElement(ElementName = "PlaceholderColor")]
        public string PlaceholderColor { get; set; }
        [XmlElement(ElementName = "Parenthesis")]
        public string Parenthesis { get; set; }
        [XmlElement(ElementName = "SkipValidation")]
        public string SkipValidation { get; set; }
        [XmlElement(ElementName = "Fields")]
        public Fields Fields { get; set; }


        public PDFFields ToPdfFields(IList<FieldCoordinate> placeholders, bool skipValidation)
        {
            PDFFields fieldsResult = new PDFFields();
            foreach (var field in Fields.Field ?? Enumerable.Empty<Field>())
            {
                var fieldType = field.Type.ParseEnum<DebenuField>();
                var coord = placeholders.FirstOrDefault(x => x.Text.ToLower() == field.Fieldname.ToLower());
                if (coord == null && !skipValidation)
                {
                    throw new InvalidOperationException(ResultCode.InvalidXMLMisMatchPlaceolders.GetNumericString());
                }
                else
                {
                    if (coord == null)
                    {
                        continue;
                    }
                    var baseField = new BaseField()
                    {
                        X = coord.Left,
                        Y = coord.Top,
                        Height = coord.Height,
                        Width = coord.Width,
                        Page = coord.Page,
                        Name = field.Fieldname,
                        Mandatory = !string.IsNullOrEmpty(field.IsMandatory) && bool.Parse(field.IsMandatory),
                        Description = string.IsNullOrEmpty(field.Description) ? string.Empty : field.Description,
                    };
                    switch (fieldType)
                    {
                        case DebenuField.Date:
                        case DebenuField.Number:
                        case DebenuField.Phone:
                        case DebenuField.Email:
                        case DebenuField.Custom:
                        case DebenuField.Time:
                        case DebenuField.Text:
                            {
                                var textField = new TextField(baseField)
                                {
                                    TextFieldType = field.Type.ParseEnum<TextFieldType>(),
                                    Value = string.IsNullOrEmpty(field.Value) ? string.Empty : field.Value
                                };
                                fieldsResult.TextFields.Add(textField);
                                break;
                            }
                        case DebenuField.Checkbox:
                            {
                                var checkbox = new CheckBoxField(baseField)
                                {
                                    IsChecked = !string.IsNullOrEmpty(field.IsChecked) && bool.Parse(field.IsChecked)
                                };
                                fieldsResult.CheckBoxFields.Add(checkbox);
                                break;
                            }
                        case DebenuField.Server_Signature:
                            {
                                SignatureField signatureField = new SignatureField(baseField)
                                {
                                    SigningType = SignatureFieldType.Server
                                };
                                fieldsResult.SignatureFields.Add(signatureField);
                                break;
                            }
                        case DebenuField.Graphic_Signature:
                            {
                                SignatureField signatureField = new SignatureField(baseField)
                                {
                                    SigningType = SignatureFieldType.Graphic
                                };
                                fieldsResult.SignatureFields.Add(signatureField);
                                break;
                            }
                        case DebenuField.SmartCard_Signature:
                            {
                                SignatureField signatureField = new SignatureField(baseField)
                                {
                                    SigningType = SignatureFieldType.SmartCard
                                };
                                fieldsResult.SignatureFields.Add(signatureField);
                                break;
                            }
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            foreach (var groupField in Fields.GroupField ?? Enumerable.Empty<GroupField>())
            {
                var groupType = groupField.Type.ParseEnum<DebenuField>();
                switch (groupType)
                {
                    case DebenuField.RadioGroup:
                        {
                            var radioFields = new List<RadioField>();
                            foreach(var radio in groupField.Field)
                            {
                                var coord = placeholders.FirstOrDefault(x => x.Text.ToLower() == radio.Fieldname.ToLower());
                                if (coord == null && !skipValidation)
                                {
                                    throw new InvalidOperationException(ResultCode.InvalidXMLMisMatchPlaceolders.GetNumericString());
                                }
                                if (coord == null)
                                {
                                    continue;
                                }
                                var baseField = new BaseField()
                                {
                                    X = coord.Left,
                                    Y = coord.Top,
                                    Height = coord.Height,
                                    Width = coord.Width,
                                    Page = coord.Page,
                                    Name = radio.Fieldname,
                                    Mandatory = !string.IsNullOrEmpty(groupField.IsMandatory) && bool.Parse(groupField.IsMandatory),
                                    Description = string.IsNullOrEmpty(groupField.Description) ? string.Empty : groupField.Description,
                                };
                                radioFields.Add(new RadioField(baseField));

                            }
                            fieldsResult.RadioGroupFields.Add(new RadioGroupField()
                            {
                                Name = groupField.GroupName,
                                RadioFields = radioFields.ToArray(),
                                SelectedRadioName = groupField.Field.FirstOrDefault(x => !string.IsNullOrEmpty(x.IsSelected) && bool.Parse(x.IsSelected))?.Fieldname
                            });
                            break;
                        }
                    case DebenuField.ChoiceGroup:
                        {
                            var coord = placeholders.FirstOrDefault(x => x.Text.ToLower() == groupField.Fieldname.ToLower());
                            if (coord == null && !skipValidation)
                            {
                                throw new InvalidOperationException(ResultCode.InvalidXMLMisMatchPlaceolders.GetNumericString());
                            }
                            if (coord == null)
                            {
                                continue;
                            }
                            var baseField = new BaseField()
                            {
                                X = coord.Left,
                                Y = coord.Top,
                                Height = coord.Height,
                                Width = coord.Width,
                                Page = coord.Page,
                                Name = groupField.Fieldname,
                                Mandatory = !string.IsNullOrEmpty(groupField.IsMandatory) && bool.Parse(groupField.IsMandatory),
                                Description = string.IsNullOrEmpty(groupField.Description) ? string.Empty : groupField.Description,
                            };
                            fieldsResult.ChoiceFields.Add(MergeFromXmlToChoice(groupField,baseField));

                            break;
                        }
                }
            }
            return fieldsResult;
        }

        private ChoiceField MergeFromXmlToChoice(GroupField groupField,BaseField baseField)
        {
            var choiceField = new ChoiceField(baseField)
            {
                SelectedOption = groupField.Field.FirstOrDefault(x => !string.IsNullOrEmpty(x.IsSelected) && bool.Parse(x.IsSelected) == true).Option,
                Options= groupField.Field.Select(x => x.Option).ToArray()
            };
            return choiceField;
        }
    }
}


