namespace WeSign.Validators.SelfSignValidators
{
    using Common.Enums.Documents;
    using Common.Enums.PDF;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Models.Files.PDF;
    using FluentValidation;
    using System;
    using WeSign.Models.SelfSign;

    public class UpdateSelfSignDocumentValidator : AbstractValidator<UpdateSelfSignDocumentDTO>
    {
        private readonly Common.Interfaces.IValidator _validator;
        private readonly IDataUriScheme _dataUriScheme;

        public UpdateSelfSignDocumentValidator(IDataUriScheme dataUriScheme, Common.Interfaces.IValidator validator)
        {
            _dataUriScheme = dataUriScheme;
            _validator = validator;
            RuleForEach(x => x.Fields.SignatureFields).ChildRules(signatureField =>
            {
                signatureField.RuleFor(x => x.Image).Must(IsValidImage)
                         .WithMessage(field => $"Field name:[{field.Name}] is not valid image");
            }).When(x => x.Operation == DocumentOperation.Close);
            RuleForEach(x => x.Fields.TextFields).ChildRules(field =>
            {
                field.RuleFor(x => x).Must(BeValidTextField)
                         .WithMessage(field => $"Value for field name:[{field.Name}] is not valid");
            }).When(x => x.Operation == DocumentOperation.Close);
        }

        private bool BeValidTextField(TextField field)
        {
            if(field.TextFieldType == TextFieldType.Email && (field.Mandatory || !string.IsNullOrWhiteSpace(field.Value)))
            {
                return ContactsExtenstions.IsValidEmail(field.Value);
            }
            if (field.TextFieldType == TextFieldType.Phone && (field.Mandatory || !string.IsNullOrWhiteSpace(field.Value)))
            {
                return ContactsExtenstions.IsValidPhone(field.Value);
            }

            return true;
        }

        

        private bool IsValidImage(string base64string)
        {
            return !string.IsNullOrWhiteSpace(base64string) && _dataUriScheme.IsValidImage(base64string);
        }
    }
}
