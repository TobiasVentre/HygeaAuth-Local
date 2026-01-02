using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class EmailVerificationRequestValidator : AbstractValidator<EmailVerificationRequest>
    {
        public EmailVerificationRequestValidator()
        {
            RuleFor(x => x.Email)
                 .NotEmpty()
                 .WithMessage("El email es obligatorio.")
                 .EmailAddress()
                 .WithMessage("El formato del email no es válido.");

            RuleFor(x => x.VerificationCode)
                .NotEmpty().WithMessage("El código de verificación es obligatorio.")
                .Length(6).WithMessage("El código de verificación debe tener 6 caracteres.");
        }
    }
    
}
