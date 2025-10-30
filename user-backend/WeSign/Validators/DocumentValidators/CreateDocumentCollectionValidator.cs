namespace WeSign.Validators.DocumentValidators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Enums.Documents;
    using FluentValidation;
    using System.Text.RegularExpressions;
    using WeSign.Models.Documents;
    using FluentValidation.Validators;
    using Common.Models.Documents.Signers;
    using Common.Extensions;
    using WeSign.Models.Distribution.Requests;
    using Org.BouncyCastle.Math.EC.Rfc7748;
    using Common.Models.Documents;
    using Common.Enums;

    public class CreateDocumentCollectionValidator : AbstractValidator<CreateDocumentCollectionDTO> // validate URL
    {
        private readonly Common.Interfaces.IValidator _validator;

        public CreateDocumentCollectionValidator(Common.Interfaces.IValidator validator)
        {
            _validator = validator;
            RuleFor(x => x.DocumentName)
                .NotEmpty().WithMessage("Please specify a Name");
            RuleFor(x => x.DocumentMode)
                .NotEmpty().WithMessage("Please specify a DocumentMode")
                .Must(x => x == DocumentMode.OrderedGroupSign || x == DocumentMode.GroupSign || x == DocumentMode.Online)
                .WithMessage("Please specify valid DocumentMode: 1 (OrderedGroupSign ) or 2 (GroupSign) or 3 (Online)");
            RuleFor(x => x.Signers)
                .NotNull()
                .WithMessage("Please specify a signer or signers")
                .Must(AreValidSignersMeans)
                .WithMessage("Please specify valid signers (signer should contain contactId with valid sendingMethod (sms-1,email-2 or tablet-3) or contantMeans with contactName)")
                //.Custom(BeValidSignerFields)
                //.Must(BeValidSignerFields)
                //.When(x => x.DocumentMode == DocumentMode.OrderedGroupSign || x.DocumentMode == DocumentMode.GroupSign)
                //.WithMessage("Please Specify signer fields list for each signer")
                .Must(BeUniqueFieldsForEachSigner)
                .WithMessage("There is field that assign to more than one signer")
                .Must(BeUniqueFieldsWithoutDuplication)
                .WithMessage("There is duplicate field for signer")
                .Must(BeUniqueAttachment)
                .WithMessage("Signer attachments must be unique");
            RuleForEach(x => x.Signers).ChildRules(signer =>
            {
                signer.RuleFor(x => x.OtpMode).IsInEnum().WithMessage("Please specify valid OtpMode: None = 0, CodeRequired = 1, PasswordRequired = 2, CodeAndPasswordRequired = 3");
                signer.RuleFor(x => x.OtpIdentification).NotEmpty().When(x => x.OtpMode == OtpMode.PasswordRequired || x.OtpMode == OtpMode.CodeAndPasswordRequired).WithMessage("Please specify OtpIdentification");
                signer.RuleFor(x => x.AuthenticationMode).IsInEnum().WithMessage("Please specify valid AuthenticationMode: None = 0, IDP = 1");
            });
            RuleFor(x => x.Templates)
                .NotNull()
                .WithMessage("Please specify a template or templates");
            RuleFor(x => x)
                .Must(BeValidTemplatesInSignersFields)
                .When(x => (x.Templates != null && x.Templates?.Length > 0) && (x.Signers != null && x.Signers?.Count() > 0))
                .WithMessage("Templates in signers fields and in read only fields must be from templates collection input");
            RuleFor(x => x)
                .Must(BeUniqeTemplatesWithoutDuplication)
                .When(x => x.Templates != null && x.Templates?.Length > 0)
                .WithMessage("Templates in document collection input and in signers fields must be without duplication")
                .Must(BeValueInReadOnlyFields)
                .When(x => x.ReadOnlyFields != null && x.ReadOnlyFields?.Count() > 0)
                .WithMessage("Read only fields should contain name and value")
                .Custom(BeUniqeReadOnlyFieldsWithoutDuplication);
            RuleFor(x => x.CallBackUrl)
                .Must(BeValidUrl)
                .When(x => (!string.IsNullOrWhiteSpace(x.CallBackUrl)))
                .WithMessage("Invalid URI");
            RuleFor(x => x)
                .Must(BeValidCountOfSigners)
                .WithMessage("Too many signers in one of the shared appendices");



        }

        //private void BeValidSignerFields(IEnumerable<SignerDTO> arg1, CustomContext arg2)
        //{
        //    if (documentCollection.ReadOnlyFields?.Count() > 0 && documentCollection.Signers != null)
        //    {

        //    }
        //}

        private void BeUniqeReadOnlyFieldsWithoutDuplication(CreateDocumentCollectionDTO documentCollection, ValidationContext<CreateDocumentCollectionDTO> customContext)
        {
            if (documentCollection.ReadOnlyFields != null && documentCollection.ReadOnlyFields?.Count() > 0 && documentCollection.Signers != null)
            {
                foreach (var signer in documentCollection.Signers)
                {
                    foreach (var signerField in signer.SignerFields)
                    {
                        var readOnlyField = documentCollection.ReadOnlyFields
                                                    .FirstOrDefault(x => x.TemplateId == signerField.TemplateId && x.FieldName == signerField.FieldName);
                        if (readOnlyField != null)
                        {
                            customContext.AddFailure($"Read only fields and signers fields cannot contain same field name - [{signerField.FieldName}]");
                        }
                    }
                }
            }
        }

        private bool BeValidSignerFields(IEnumerable<SignerDTO> input)
        {
            if (input != null)
            {
                var signers = input.ToArray();
                for (int i = 0; i < signers.Count(); i++)
                {
                    if (signers[i].SignerFields == null || !signers[i].SignerFields.Any())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool BeValueInReadOnlyFields(CreateDocumentCollectionDTO documentCollectionDTO)
        {
            if (documentCollectionDTO.Signers != null)
            {
                foreach (var readOnlyField in documentCollectionDTO.ReadOnlyFields)
                {
                    if (string.IsNullOrWhiteSpace(readOnlyField?.FieldValue) ||
                        string.IsNullOrWhiteSpace(readOnlyField?.FieldName))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool BeUniqeReadOnlyFieldsWithoutDuplication(CreateDocumentCollectionDTO documentCollectionDTO)
        {
            if (documentCollectionDTO.Signers != null)
            {
                foreach (var signer in documentCollectionDTO.Signers)
                {
                    foreach (var signerField in signer.SignerFields)
                    {
                        var readOnlyField = documentCollectionDTO.ReadOnlyFields
                                                    .FirstOrDefault(x => x.TemplateId == signerField.TemplateId && x.FieldName == signerField.FieldName);
                        if (readOnlyField != null)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool BeUniqueAttachment(IEnumerable<SignerDTO> signers)
        {
            if (signers != null)
            {
                foreach (var signer in signers)
                {
                    if (signer.SignerAttachments != null)
                    {
                        var duplicateAttachments = signer.SignerAttachments.ToList().GroupBy(x => x.Name)
                                                                    .Where(g => g.Count() > 1)
                                                                    .Select(c => c.Key).ToList();
                        if (duplicateAttachments.Count() != 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool BeUniqeTemplatesWithoutDuplication(CreateDocumentCollectionDTO documentCollection)
        {
            bool isUnique = documentCollection.Templates.Distinct().Count() == documentCollection.Templates.Count();

            return isUnique;
        }

        private bool BeValidTemplatesInSignersFields(CreateDocumentCollectionDTO documentCollection)
        {
            var templates = documentCollection.Templates;
            foreach (var signer in documentCollection.Signers)
            {
                if (signer.SignerFields != null && signer.SignerFields.Any())
                {
                    foreach (var signerField in signer.SignerFields)
                    {
                        var template = templates.FirstOrDefault(x => x == signerField.TemplateId);
                        if (template == Guid.Empty)
                        {
                            return false;
                        }
                    }
                }
            }
            if (documentCollection.ReadOnlyFields != null && documentCollection.ReadOnlyFields.Any())
            {
                foreach (var readOnlyField in documentCollection.ReadOnlyFields)
                {
                    var template = templates.FirstOrDefault(x => x == readOnlyField.TemplateId);
                    if (template == Guid.Empty)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool BeUniqueFieldsForEachSigner(IEnumerable<SignerDTO> input)
        {
            if (input != null)
            {
                var signers = input.ToArray();
                for (int i = 0; i < signers.Count(); i++)
                {
                    if (signers[i].SignerFields != null && signers[i].SignerFields.Any())
                    {
                        foreach (var signerField in signers[i].SignerFields)
                        {
                            for (int j = 0; j < signers.Count(); j++)
                            {
                                if (i != j && FieldExistOtherSignerFields(signerField, signers[j]))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool FieldExistOtherSignerFields(SignerFieldDTO signerField, SignerDTO signerDTO)
        {
            foreach (var item in signerDTO.SignerFields)
            {
                if (signerField.FieldName == item.FieldName)
                {
                    return true;
                }
            }
            return false;
        }

        private bool AreValidSigners(IEnumerable<SignerDTO> signers)
        {
            if (signers == null || !signers.Any())
            {
                return false;
            }
            foreach (var signer in signers)
            {
                if (signer.SendingMethod != SendingMethod.Email && signer.SendingMethod != SendingMethod.SMS && signer.SendingMethod != SendingMethod.Tablet)
                {
                    return false;
                }
            }
            return true;
        }

        private bool AreValidSignersMeans(IEnumerable<SignerDTO> signers)
        {
            if (signers == null || !signers.Any())
            {
                return false;
            }
            foreach (var signer in signers)
            {
                bool isValidContactWithoutId = IsValidContactWithoutId(signer);
                bool isValidContactWithId = IsValidContactWithId(signer);
                if ((!isValidContactWithoutId && !isValidContactWithId) || (isValidContactWithoutId && isValidContactWithId))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsValidContactWithId(SignerDTO signer)
        {
            return (string.IsNullOrWhiteSpace(signer.ContactMeans) && string.IsNullOrWhiteSpace(signer.ContactName) &&
                    (signer.SendingMethod == SendingMethod.SMS || signer.SendingMethod == SendingMethod.Email) && signer.ContactId != Guid.Empty)
                    ||
                    (signer.SendingMethod == SendingMethod.Tablet && !string.IsNullOrWhiteSpace(signer.ContactName));
        }

        private bool IsValidContactWithoutId(SignerDTO signer)
        {
            return !string.IsNullOrWhiteSpace(signer.ContactMeans) && !string.IsNullOrWhiteSpace(signer.ContactName) &&
                        (ContactsExtenstions.IsValidEmail(signer.ContactMeans) || (ContactsExtenstions.IsValidPhone(signer.ContactMeans) && ContactsExtenstions.IsValidPhoneExtension(signer.PhoneExtension)));
        }

        private bool BeUniqueFieldsWithoutDuplication(IEnumerable<SignerDTO> signers)
        {
            if (signers != null)
            {
                foreach (var signer in signers)
                {
                    bool hasUniqueFields = HasUniqueFields(signer.SignerFields);
                    if (!hasUniqueFields)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool HasUniqueFields(IEnumerable<SignerFieldDTO> signerFields)
        {
            if (signerFields == null)
            {
                return true;
            }
            var duplicateFields = signerFields.ToList().GroupBy(x => x.FieldName)
                                                            .Where(g => g.Count() > 1)
                                                            .Select(c => c.Key).ToList();
            return duplicateFields.Count() == 0;
        }

        //TODO validate that signers fields exist in template
        private bool BeValidUrl(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private bool BeValidCountOfSigners(CreateDocumentCollectionDTO documentCollection)
        {
            int signerCount = documentCollection.Signers.Count();
            foreach (SharedAppendixDTO sharedAppendix in documentCollection.SharedAppendices ?? Enumerable.Empty<SharedAppendixDTO>())
            {
                if (sharedAppendix.SignerIndexes.Count > signerCount)
                    return false;
            }
            return true;
        }
    }

    public class CreateSimpleDocumentValidator : AbstractValidator<CreateSimpleDocumentDTO>
    {
        public CreateSimpleDocumentValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotNull()
                .NotEmpty().
                WithMessage("Template Id can't be empty");
            RuleFor(x => x.SignerMeans)
                .NotNull()
                .NotEmpty().
                Must(data => ContactsExtenstions.IsValidPhone(data) || ContactsExtenstions.IsValidEmail(data)).
                WithMessage("Not valid email or phone number");
            RuleFor(x => x.DocumentName)
              .NotNull()
              .NotEmpty()
              .WithMessage("Document Name Can't be empty");
            RuleFor(x => x.SignerOTPMeans).Must(ValidOTPMeans).WithMessage("OTP Means is not valid plase send a valid email or phone number");
            RuleFor(x => x.CallBackUrl)
             .Must(BeValidUrl)
             .When(x => (!string.IsNullOrWhiteSpace(x.CallBackUrl)))
             .WithMessage("Invalid URI");


        }

        private bool ValidOTPMeans(string optMeans)
        {
            if (!string.IsNullOrWhiteSpace(optMeans))
            {
                return ContactsExtenstions.IsValidPhone(optMeans) || ContactsExtenstions.IsValidEmail(optMeans);
            }

            return true;
        }

        private bool BeValidUrl(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        //TODO validate there is not field that is assign to more than one signer
    }
}
