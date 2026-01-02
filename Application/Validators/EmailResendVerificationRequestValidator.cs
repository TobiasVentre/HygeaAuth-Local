using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class EmailResendVerificationRequestValidator : AbstractValidator<EmailVerificationRequest>
    {
        public EmailResendVerificationRequestValidator()
        {
            RuleFor(x => x.Email)
                 .NotEmpty()
                 .WithMessage("El email es obligatorio.")
                 .EmailAddress()
                 .WithMessage("El formato del email no es válido.");
        }
    }
}
