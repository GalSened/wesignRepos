using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSign.Models.Documents;

namespace WeSign.Validators.DocumentValidators
{
    public class DocumentBatchDownloadRequestValidator : AbstractValidator<DownloadBatchRequestDTO>
    {
        public DocumentBatchDownloadRequestValidator()
        {
            RuleFor(x => x.Ids).NotNull().WithMessage("Documents ids list can't be empty").NotEmpty().WithMessage("Documents ids list can't be empty").Must(ValidateAllValidGuids).
                WithMessage("One or more of the ids are not in the correct format")
                .Must(ValidateMaxOf20Ids).WithMessage("You passed the maximum of 20 documents per download");
        }
        private bool ValidateMaxOf20Ids(string[] ids)
        {
            return ids.Length <= 20;
        }

        private bool ValidateAllValidGuids(string[] ids)
        {
            foreach (string id in ids)
            {
                if(!Guid.TryParse(id, out Guid guid))
                {
                    return false;
                }
            }
            return true;
        }
    }
    public class DocumentBatchDeleteRequestValidator : AbstractValidator<DeleteBatchRequestDTO>
    {
        public DocumentBatchDeleteRequestValidator()
        {
            RuleFor(x => x.Ids).NotNull().WithMessage("Documents ids list can't be empty").NotEmpty().WithMessage("Documents ids list can't be empty").Must(ValidateAllValidGuids).
                WithMessage("One or more of the ids are not in the correct format");
        }

        private bool ValidateAllValidGuids(string[] ids)
        {
            foreach (string id in ids)
            {
                if (!Guid.TryParse(id, out Guid guid))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
