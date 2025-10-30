using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSignManagement.Models.Payment;

namespace WeSignManagement.Validators.Payment
{
    public class PaymentValidator : AbstractValidator<UserPaymentRequestDTO>
    {
        public PaymentValidator()
        {
            RuleFor(x => x.UserEmail)
              .NotEmpty().WithMessage("Please specify an Email")
              .EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email");


            RuleFor(x => x.ProgramID)
              .NotEmpty().WithMessage("Please specify an Program ID");
        }
    }
}
