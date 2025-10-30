namespace WeSign.Validators.TemplateValidators
{
    using Common.Enums.PDF;
    using Common.Models.Files.PDF;
    using FluentValidation;
    using FluentValidation.Validators;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using WeSign.Models.Distribution.Requests;
    using WeSign.Models.Templates;


    public class UpdateTemplateValidator : AbstractValidator<UpdateTemplateDTO>
    {

        public UpdateTemplateValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Please specify template name");
            RuleForEach(x => x.Fields.TextFields).ChildRules(textField =>
            {
                textField.RuleFor(x => x.Name).NotEmpty().Must(x => x!=null && !x.Contains('.'))
                         .WithMessage(field => $"Field name [{field.Name}] cannot include '.' char");
                textField.RuleFor(x => x.Page).Must(x => x > 0).NotEmpty()
                        .WithMessage(field => $"Field [{field.Name}] page must be grater than 0");
                textField.RuleFor(x => x.Value).Must(IsDate)
                         .When(x => x.TextFieldType == TextFieldType.Date)
                         .WithMessage(field => $"Field value [{field.Value}] does not match the field type [{field.TextFieldType}] for a text field named [{field.Name}]");
                textField.RuleFor(x => x.Value).Must(IsTime)
                         .When(x => x.TextFieldType == TextFieldType.Time)
                         .WithMessage(field => $"Field value [{field.Value}] does not match the field type [{field.TextFieldType}] for a text field named [{field.Name}]");
                textField.RuleFor(x => x.Value).Must(IsDigitsOnly)
                         .When(x => x.TextFieldType == TextFieldType.Number)
                         .WithMessage(field => $"Field value [{field.Value}] does not match the field type [{field.TextFieldType}] for a text field named [{field.Name}]");
                textField.RuleFor(x => x.Value).Must(IsPhone)
                         .When(x => x.TextFieldType == TextFieldType.Phone)
                         .WithMessage(field => $"Field value [{field.Value}] does not match the field type [{field.TextFieldType}] for a text field named [{field.Name}]");
                textField.RuleFor(x => x.Value).Must(IsEmail)
                         .When(x => x.TextFieldType == TextFieldType.Email)
                         .WithMessage(field => $"Field value [{field.Value}] does not match the field type [{field.TextFieldType}] for a text field named [{field.Name}]");
                textField.RuleFor(x => x.TextFieldType).NotEmpty().IsInEnum()
                         .WithMessage(field => $"Field type [{field.TextFieldType}] for a text field named [{field.Name}] not supported");
                textField.RuleFor(x=>x.X).GreaterThanOrEqualTo(0).WithMessage(field => $"X of field [{field.Name}] should be greater than or equal to 0");
                textField.RuleFor(x=>x.X).LessThan(1).WithMessage(field => $"X of field [{field.Name}] should be less than 1");
                textField.RuleFor(x => x.Y).GreaterThanOrEqualTo(0).WithMessage(field => $"Y of field [{field.Name}] should be greater than or equal to 0");
                textField.RuleFor(x => x.Y).LessThan(1).WithMessage(field => $"Y of field [{field.Name}] should be less than 1");
                textField.RuleFor(x => x.Width).NotEmpty().GreaterThanOrEqualTo(0.0001).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0.0001");
                textField.RuleFor(x => x.Width).LessThan(1).WithMessage(field => $"Width of field [{field.Name}] should be less than 1");
                textField.RuleFor(x => x.Height).NotEmpty().GreaterThanOrEqualTo(0.0009).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0.0009");
                textField.RuleFor(x => x.Height).LessThan(1).WithMessage(field => $"Height of field [{field.Name}] should be less than 1");
                //textField.RuleFor(x => x.Width).NotEmpty().GreaterThanOrEqualTo(0.08).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0.08");
                //textField.RuleFor(x => x.Width).LessThan(0.51).WithMessage(field => $"Width of field [{field.Name}] should be less than 0.51");
                //textField.RuleFor(x => x.Height).NotEmpty().GreaterThanOrEqualTo(0.01).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0.01");
                //textField.RuleFor(x => x.Height).LessThan(0.09).WithMessage(field => $"Height of field [{field.Name}] should be less than 0.09");
                //textField.RuleFor(x=>x).Must(SumLessThan1).WithMessage(field => $"Height + Y and Width + X of field [{field.Name}] should be less than 1"); 
            });
            RuleFor(x => x.Fields.TextFields).Custom(ValidateUniqueFields);            

            RuleForEach(x => x.Fields.RadioGroupFields).ChildRules(radioGroup => {
                radioGroup.RuleFor(x => x.Name).NotEmpty().Must(x => x != null && !x.Contains('.'))
                         .WithMessage(field => $"Field name [{field.Name}] cannot include '.' char");
                radioGroup.RuleFor(x => x.Name)
                        .NotEmpty().WithMessage("Please specify group name");
                radioGroup.RuleFor(x => x.RadioFields)
                        .Must(y => y.Length > 1).WithMessage("Radio group should contains at least 2 elements")
                        .Must(y=>HasUniqueRadioNamesInGroup(y))
                        .WithMessage(radioGroupField => $"Radio group [{radioGroupField.Name}] should not have duplicate radio fields names");
                radioGroup.RuleForEach(x => x.RadioFields).ChildRules(radio =>
                {
                    radio.RuleFor(x => x.Name).NotEmpty().Must(x => x != null && !x.Contains('.'))
                        .WithMessage(field => $"Field name [{field.Name}] cannot include '.' char");
                    radio.RuleFor(x => x.X).GreaterThanOrEqualTo(0).WithMessage(field => $"X of field [{field.Name}] should be greater than or equal to 0");
                    radio.RuleFor(x => x.X).LessThan(1).WithMessage(field => $"X of field [{field.Name}] should be less than 1");
                    radio.RuleFor(x => x.Y).GreaterThanOrEqualTo(0).WithMessage(field => $"Y of field [{field.Name}] should be greater than or equal to 0");
                    radio.RuleFor(x => x.Y).LessThan(1).WithMessage(field => $"Y of field [{field.Name}] should be less than 1");
                    radio.RuleFor(x => x.Width).NotEmpty().GreaterThanOrEqualTo(0).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0");
                    radio.RuleFor(x => x.Width).LessThan(1).WithMessage(field => $"Width of field [{field.Name}] should be less than 1");
                    radio.RuleFor(x => x.Height).NotEmpty().GreaterThanOrEqualTo(0).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0");
                    radio.RuleFor(x => x.Height).LessThan(1).WithMessage(field => $"Height of field [{field.Name}] should be less than 1");
                    //radio.RuleFor(x => x.Width).GreaterThanOrEqualTo(0.01).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0.01");
                    //radio.RuleFor(x => x.Width).LessThan(0.65).WithMessage(field => $"Width of field [{field.Name}] should be less than 0.65");
                    //radio.RuleFor(x => x.Height).GreaterThanOrEqualTo(0.007).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0.007");
                    //radio.RuleFor(x => x.Height).LessThan(0.045).WithMessage(field => $"Height of field [{field.Name}] should be less than 0.045");
                    //radio.RuleFor(x=>x).Must(SumLessThan1).WithMessage(field => $"Height + Y and Width + X of field [{field.Name}] should be less than 1");
                });
                radioGroup.RuleFor(x => x.RadioFields).Custom(ValidateUniqueFields);
            });
            RuleFor(x => x.Fields.RadioGroupFields).Custom(ValidateUniqueFields);

            RuleForEach(x => x.Fields.ChoiceFields).ChildRules(choiceField =>
            {
                choiceField.RuleFor(x => x.Name).NotEmpty().Must(x => x != null && !x.Contains('.'))
                            .WithMessage(field => $"Field name [{field.Name}] cannot include '.' char");
                choiceField.RuleFor(x => x.Page).Must(x => x > 0)
                        .WithMessage(field => $"Field [{field.Name}] page must be grater than 0");
                choiceField.RuleFor(x => x.X).GreaterThanOrEqualTo(0).WithMessage(field => $"X of field [{field.Name}] should be greater than or equal to 0");
                choiceField.RuleFor(x => x.X).LessThan(1).WithMessage(field => $"X of field [{field.Name}] should be less than 1");
                choiceField.RuleFor(x => x.Y).GreaterThanOrEqualTo(0).WithMessage(field => $"Y of field [{field.Name}] should be greater than or equal to 0");
                choiceField.RuleFor(x => x.Y).LessThan(1).WithMessage(field => $"Y of field [{field.Name}] should be less than 1");
                choiceField.RuleFor(x => x.Width).NotEmpty().GreaterThanOrEqualTo(0).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0");
                choiceField.RuleFor(x => x.Width).LessThan(1).WithMessage(field => $"Width of field [{field.Name}] should be less than 1");
                choiceField.RuleFor(x => x.Height).NotEmpty().GreaterThanOrEqualTo(0).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0");
                choiceField.RuleFor(x => x.Height).LessThan(1).WithMessage(field => $"Height of field [{field.Name}] should be less than 1");
                //choiceField.RuleFor(x => x.Width).GreaterThanOrEqualTo(0.08).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0.08");
                //choiceField.RuleFor(x => x.Width).LessThan(0.51).WithMessage(field => $"Width of field [{field.Name}] should be less than 0.51");
                //choiceField.RuleFor(x => x.Height).GreaterThanOrEqualTo(0.01).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0.01");
                //choiceField.RuleFor(x => x.Height).LessThan(0.09).WithMessage(field => $"Height of field [{field.Name}] should be less than 0.09");
                //choiceField.RuleFor(x => x).Must(SumLessThan1).WithMessage(field => $"Height + Y and Width + X of field [{field.Name}] should be less than 1");
                choiceField.RuleFor(x => x.Name)
                        .NotEmpty().WithMessage("Please specify name");
                choiceField.RuleFor(x => x.Options)
                        .Must(y=>y?.Length > 0 ).WithMessage("Choice field should contains at least 1 element");                        
            });
            RuleFor(x => x.Fields.ChoiceFields).Custom(ValidateUniqueFields);


            RuleForEach(x => x.Fields.CheckBoxFields).ChildRules(checkBoxField=>
            {
                checkBoxField.RuleFor(x => x.Name).NotEmpty().Must(x => x != null && !x.Contains('.'))
                         .WithMessage(field => $"Field name [{field.Name}] cannot include '.' char");
                checkBoxField.RuleFor(x => x.Page).Must(x => x > 0)
                        .WithMessage(field => $"Field [{field.Name}] page must be grater than 0");
                checkBoxField.RuleFor(x => x.X).GreaterThanOrEqualTo(0).WithMessage(field => $"X of field [{field.Name}] should be greater than or equal to 0");
                checkBoxField.RuleFor(x => x.X).LessThan(1).WithMessage(field => $"X of field [{field.Name}] should be less than 1");
                checkBoxField.RuleFor(x => x.Y).GreaterThanOrEqualTo(0).WithMessage(field => $"Y of field [{field.Name}] should be greater than or equal to 0");
                checkBoxField.RuleFor(x => x.Y).LessThan(1).WithMessage(field => $"Y of field [{field.Name}] should be less than 1");
                checkBoxField.RuleFor(x => x.Width).NotEmpty().GreaterThanOrEqualTo(0).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0");
                checkBoxField.RuleFor(x => x.Width).LessThan(1).WithMessage(field => $"Width of field [{field.Name}] should be less than 1");
                checkBoxField.RuleFor(x => x.Height).NotEmpty().GreaterThanOrEqualTo(0).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0");
                checkBoxField.RuleFor(x => x.Height).LessThan(1).WithMessage(field => $"Height of field [{field.Name}] should be less than 1");
                //checkBoxField.RuleFor(x => x.Width).GreaterThanOrEqualTo(0.01).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0.01");
                //checkBoxField.RuleFor(x => x.Width).LessThan(0.65).WithMessage(field => $"Width of field [{field.Name}] should be less than 0.65");
                //checkBoxField.RuleFor(x => x.Height).GreaterThanOrEqualTo(0.007).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0.007");
                //checkBoxField.RuleFor(x => x.Height).LessThan(0.045).WithMessage(field => $"Height of field [{field.Name}] should be less than 0.045");
                //checkBoxField.RuleFor(x => x).Must(SumLessThan1).WithMessage(field => $"Height + Y and Width + X of field [{field.Name}] should be less than 1");
            });
            RuleFor(x => x.Fields.CheckBoxFields).Custom(ValidateUniqueFields);

            RuleForEach(x => x.Fields.SignatureFields).ChildRules(signatureField =>
            {
                signatureField.RuleFor(x => x.Name).Must(x => !x.Contains('.'))
                         .WithMessage(field => $"Field name [{field.Name}] cannot include '.' char");
                signatureField.RuleFor(x => x.Page).Must(x => x > 0)
                        .WithMessage(field => $"Field [{field.Name}] page must be grater than 0");
                signatureField.RuleFor(x => x.SigningType).IsInEnum()
                        .WithMessage(field => $"Field signing type [{field.SigningType}] for a signature field named [{field.Name}] not supported");
                signatureField.RuleFor(x => x.X).GreaterThanOrEqualTo(0).WithMessage(field => $"X of field [{field.Name}] should be greater than or equal to 0");
                signatureField.RuleFor(x => x.X).LessThan(1).WithMessage(field => $"X of field [{field.Name}] should be less than 1");
                signatureField.RuleFor(x => x.Y).GreaterThanOrEqualTo(0).WithMessage(field => $"Y of field [{field.Name}] should be greater than or equal to 0");
                signatureField.RuleFor(x => x.Y).LessThan(1).WithMessage(field => $"Y of field [{field.Name}] should be less than 1");
                signatureField.RuleFor(x => x.Width).NotEmpty().GreaterThanOrEqualTo(0).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0");
                signatureField.RuleFor(x => x.Width).LessThan(1).WithMessage(field => $"Width of field [{field.Name}] should be less than 1");
                signatureField.RuleFor(x => x.Height).NotEmpty().GreaterThanOrEqualTo(0).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0");
                signatureField.RuleFor(x => x.Height).LessThan(1).WithMessage(field => $"Height of field [{field.Name}] should be less than 1");
                //signatureField.RuleFor(x => x.Width).GreaterThanOrEqualTo(0.08).WithMessage(field => $"Width of field [{field.Name}] should be greater than or equal to 0.08");
                //signatureField.RuleFor(x => x.Width).LessThan(0.51).WithMessage(field => $"Width of field [{field.Name}] should be less than 0.51");
                //signatureField.RuleFor(x => x.Height).GreaterThanOrEqualTo(0.01).WithMessage(field => $"Height of field [{field.Name}] should be greater than or equal to 0.01");
                //signatureField.RuleFor(x => x.Height).LessThan(0.09).WithMessage(field => $"Height of field [{field.Name}] should be less than 0.09");
                //signatureField.RuleFor(x => x).Must(SumLessThan1).WithMessage(field => $"Height + Y and Width + X of field [{field.Name}] should be less than 1");
            });
            RuleFor(x => x.Fields.SignatureFields).Custom(ValidateUniqueFields);
        }

        private void ValidateUniqueFields(RadioField[] fields, ValidationContext<RadioGroupField> customContext)
        {
            var duplicateFields = fields.ToList().GroupBy(x => x.Name)
                                                                        .Where(g => g.Count() > 1)
                                                                        .Select(c => c.Key).ToList();
            if (duplicateFields.Count() > 0)
            {
                customContext.AddFailure($"RadioGroup fields must be unique, remove duplication fields- [{string.Join(", ", duplicateFields)}]");
            };
        }

        private void ValidateUniqueFields(List<RadioGroupField> fields, ValidationContext<UpdateTemplateDTO>  customContext)
        {
            var duplicateFields = fields.ToList().GroupBy(x => x.Name)
                                                                        .Where(g => g.Count() > 1)
                                                                        .Select(c => c.Key).ToList();
            if (duplicateFields.Count() > 0)
            {
                customContext.AddFailure($"RadioGroup fields must be unique, remove duplication fields- [{string.Join(", ", duplicateFields)}]");
            }
        }

        private void ValidateUniqueFields(RadioField[] fields, ValidationContext<UpdateTemplateDTO> customContext)
        {
            var duplicateFields = fields.ToList().GroupBy(x => x.Name)
                                                            .Where(g => g.Count() > 1)
                                                            .Select(c => c.Key).ToList();
            if (duplicateFields.Count() > 0)
            {
                customContext.AddFailure($"Radio fields must be unique, remove duplication fields- [{string.Join(", ", duplicateFields)}]");
            }
            var duplicateFieldsByCordinate = fields.ToList().GroupBy(x => new { x.X, x.Y, x.Page })
                                                            .Where(g => g.Count() > 1)
                                                            .Select(x => x.Key).ToList();
            if (duplicateFieldsByCordinate.Count() > 0)
            {
                var duplicateFieldsByCordinateString = duplicateFieldsByCordinate.Select(x => $"X={x.X};Y={x.Y}");
                customContext.AddFailure($"Radio fields coordinates must be unique, remove duplication fields with same X , Y - [{string.Join(", ", duplicateFieldsByCordinateString)}]");
            }
        }

        private void ValidateUniqueFields(List<CheckBoxField> fields, ValidationContext<UpdateTemplateDTO> customContext)
        {
            var duplicateFields = fields.ToList().GroupBy(x => x.Name)
                                                            .Where(g => g.Count() > 1)
                                                            .Select(c => c.Key).ToList();
            if (duplicateFields.Count() > 0)
            {
                customContext.AddFailure($"CheckBox fields must be unique, remove duplication fields- [{string.Join(", ", duplicateFields)}]");
            }
            var duplicateFieldsByCordinate = fields.ToList().GroupBy(x => new { x.X, x.Y, x.Page })
                                                            .Where(g => g.Count() > 1)
                                                            .Select(x => x.Key).ToList();
            if (duplicateFieldsByCordinate.Count() > 0)
            {
                var duplicateFieldsByCordinateString = duplicateFieldsByCordinate.Select(x => $"X={x.X};Y={x.Y}");
                customContext.AddFailure($"CheckBox fields coordinates must be unique, remove duplication fields with same X , Y - [{string.Join(", ", duplicateFieldsByCordinateString)}]");
            }
        }

        private void ValidateUniqueFields(List<ChoiceField> fields, ValidationContext<UpdateTemplateDTO> customContext)
        {
            var duplicateFields = fields.ToList().GroupBy(x => x.Name)
                                                            .Where(g => g.Count() > 1)
                                                            .Select(c => c.Key).ToList();
            if (duplicateFields.Count() > 0)
            {
                customContext.AddFailure($"Choice fields must be unique, remove duplication fields- [{string.Join(", ", duplicateFields)}]");
            }
            var duplicateFieldsByCordinate = fields.ToList().GroupBy(x => new { x.X, x.Y, x.Page })
                                                            .Where(g => g.Count() > 1)
                                                            .Select(x => x.Key).ToList();
            if (duplicateFieldsByCordinate.Count() > 0)
            {
                var duplicateFieldsByCordinateString = duplicateFieldsByCordinate.Select(x => $"X={x.X};Y={x.Y}");
                customContext.AddFailure($"Choice fields coordinates must be unique, remove duplication fields with same X , Y - [{string.Join(", ", duplicateFieldsByCordinateString)}]");
            }
        }

        private void ValidateUniqueFields(List<TextField> fields, ValidationContext<UpdateTemplateDTO> customContext)
        {
            var duplicateFieldsByCordinate = fields.ToList().GroupBy(x => new { x.X , x.Y, x.Page })
                                                            .Where(g => g.Count() > 1)
                                                            .Select(x=>x.Key).ToList();            
            if (duplicateFieldsByCordinate.Count() > 0)
            {
                var duplicateFieldsByCordinateString = duplicateFieldsByCordinate.Select(x => $"X={x.X};Y={x.Y}");
                customContext.AddFailure($"Text fields coordinates must be unique, remove duplication fields with same X , Y - [{string.Join(", ", duplicateFieldsByCordinateString)}]");
            }
        }

        private void ValidateUniqueFields(List<SignatureField> fields, ValidationContext<UpdateTemplateDTO> customContext)
        {
            var duplicateFields = fields.ToList().GroupBy(x => x.Name)
                                                            .Where(g => g.Count() > 1)
                                                            .Select(c => c.Key).ToList();
            if (duplicateFields.Count() > 0)
            {
                customContext.AddFailure($"Signature fields must be unique, remove duplication fields- [{string.Join(", ", duplicateFields)}]");
            }
            var duplicateFieldsByCordinate = fields.ToList().GroupBy(x => new { x.X, x.Y, x.Page})
                                                            .Where(g => g.Count() > 1)
                                                            .Select(x => x.Key).ToList();
            if (duplicateFieldsByCordinate.Count() > 0)
            {
                var duplicateFieldsByCordinateString = duplicateFieldsByCordinate.Select(x => $"X={x.X};Y={x.Y}");
                customContext.AddFailure($"Signature fields coordinates must be unique, remove duplication fields with same X , Y - [{string.Join(", ", duplicateFieldsByCordinateString)}]");
            }
        }
        
        private bool SumLessThan1(BaseField field)
        {
            return field.X + field.Width <= 1 && field.Y + field.Height <= 1;
        }

        //TODO check that all pages in fields are grater than 0 and smaller than template pages count

        

        private bool HasUniqueRadioNamesInGroup(RadioField[] radioFields)
        {
            var duplicateRadioFields =  radioFields.ToList().GroupBy(x => x.Name)
                                                            .Where(g => g.Count() > 1)
                                                            .Select(c => c.Key).ToList();
            return duplicateRadioFields.Count() == 0;

        }

        private bool IsPhone(string phone)
        {
            Regex rgx = new Regex("^[0-9\\-\\+]{9,15}$");
            return string.IsNullOrWhiteSpace(phone)? true : rgx.IsMatch(phone);
        }

        private bool IsDigitsOnly(string number)
        {
            if (string.IsNullOrWhiteSpace( number))
                return true;
            if (number[0] == '-' && number.Length > 1) 
                number = number.Substring(1); 
            foreach (char c in number ?? "")
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private bool IsEmail(string email)
        {
            Regex rgx = new Regex("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$");
            return string.IsNullOrWhiteSpace(email) ? true : rgx.IsMatch(email);
        }

        private bool IsDate(string date)
        {
            try
            {

                
                if (string.IsNullOrWhiteSpace(date)) return true;
                var result = DateTime.Parse(date, new CultureInfo("en-US"));
                return true;
            }
            catch
            {
                return false;
            }
        }


        private bool IsTime(string time)
        {
            Regex rgx = new Regex("^([0-1]{1}[0-9]{1}|[2]{1}[0-3]{1}):[0-5]{1}[0-9]{1}$");
            return string.IsNullOrWhiteSpace(time) ? true : rgx.IsMatch(time);
        }

    }
}
