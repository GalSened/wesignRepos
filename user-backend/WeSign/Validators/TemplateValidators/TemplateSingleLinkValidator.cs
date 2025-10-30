using Common.Models;
using Common.Models.Documents.Signers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using WeSign.Models.Templates;

namespace WeSign.Validators.TemplateValidators
{
    public class TemplateSingleLinkValidator : AbstractValidator<TemplateSingleLinkDTO>
    {
        public TemplateSingleLinkValidator() {
            RuleFor(x => x.SingleLinkAdditionalResources).
              Must(HaveToContainName).WithMessage("attachments must have a name");
            RuleFor(x => x.SingleLinkAdditionalResources).
                Must(BeUnique).WithMessage("attachments must be unique");

            
        }

        private bool HaveToContainName(List<SingleLinkAdditionalResource> list)
        {
            if (list != null)
            {
                if (list.Count > 0)
                {
                    if( list.Where(x => x.Type == Common.Enums.Templates.SingleLinkAdditionalResourceType.Attachment &&
                    string.IsNullOrWhiteSpace(x.Data)).Any())
                    {
                        return false;
                    }

                }
            }
            return true;
        }

        private bool BeUnique(List<SingleLinkAdditionalResource> list)
        {
            if (list != null)
            {
                if (list.Count > 0)
                {
                    var duplicateAttachments = list.Where(x => x.Type == Common.Enums.Templates.SingleLinkAdditionalResourceType.Attachment ).GroupBy( x => x.Data)
                                                  .Where(g => g.Count() > 1)
                                                  .Select(c => c.Key).ToList();

                    if (duplicateAttachments.Count() != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
            
        }
    }
}
