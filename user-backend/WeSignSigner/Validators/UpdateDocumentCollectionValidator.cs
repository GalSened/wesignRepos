using WeSignSigner.Models.Requests;

using Common.Enums.Documents;
using PdfHandler.Enums;
using Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Common.Enums.PDF;

namespace WeSignSigner.Validators
{
    public class UpdateDocumentCollectionValidator : AbstractValidator<UpdateDocumentCollectionDTO>
    {
        private readonly IDataUriScheme _dataUri;

        public UpdateDocumentCollectionValidator(IDataUriScheme dataUri)
        {
            _dataUri = dataUri;

            RuleFor(x => x.Operation)
                .NotEmpty()
                .NotNull()
                .WithMessage("Operation cannot be null or empty")
                .Must(x => x == DocumentOperation.Save || x == DocumentOperation.Decline || x == DocumentOperation.Close)
                .WithMessage("Valid Operation : 1 (Save) or 2 (Decline) or 3 (Close)"); 

            RuleFor(x => x.Documents)
                .NotEmpty()
                .NotNull()
                .When(x => x.Operation == DocumentOperation.Close || x.Operation == DocumentOperation.Save)
                .WithMessage("Documents cannot be null or empty");
            RuleForEach(x => x.Documents).ChildRules(document =>
            {
                document.RuleFor(x => x.DocumentId).NotEmpty()
                         .WithMessage("DocuemntId cannot be null or empty");
                document.RuleForEach(y => y.Fields).ChildRules(field =>
                  {
                      field.RuleFor(x => x.FieldName).NotEmpty()
                              .WithMessage("FieldName cannot be null or empty");
                      field.RuleFor(x => x.FieldType)
                              .Must(x=> x == WeSignFieldType.TextField || x == WeSignFieldType.ChoiceField ||
                                        x == WeSignFieldType.CheckBoxField || x == WeSignFieldType.RadioGroupField ||
                                        x == WeSignFieldType.SignatureField)
                              .WithMessage("Valid WeSignFieldType : 1 (TextField) or 2 (ChoiceField) or 3 (CheckBoxField) or 4 (RadioGroupField) or 5 (SignatureField)");
                  });
                document.RuleFor(y => y.Fields)
                .Must(AreValidSignatures)
                .WithMessage("Invalid base 64 image");
                //document.RuleFor(y => y.Fields)
                //.Must(AreAllUniqueFields)
                //.WithMessage("Duplicate fields in collection");

            })
            .When(x => x.Operation == DocumentOperation.Close);
        }

        //private bool AreAllUniqueFields(IEnumerable<FieldDTO> fields)
        //{
        //    var names = fields.Select(x => x.FieldName);
        //    return names.Count() == names.Distinct().Count();            
        //}

        private bool AreValidSignatures(IEnumerable<FieldDTO> fields)
        {
            foreach (var field in fields ?? Enumerable.Empty<FieldDTO>())
            {
                if(field.FieldType == WeSignFieldType.SignatureField)
                {
                    if (!_dataUri.IsValidImage(field.FieldValue))
                    {
                        return false;
                    }                        
                }
            }
            return true;
        }
    }
}
