using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class PasswordResetConfirmRequestValidator : AbstractValidator<PasswordResetConfirmRequest>
    {
        public PasswordResetConfirmRequestValidator()
        {
            RuleFor(x => x.Email)
                 .NotEmpty()
                 .WithMessage("El email es obligatorio.")
                 .EmailAddress()
                 .WithMessage("El formato del email no es válido.");

            RuleFor(x => x.ResetCode)
                .NotEmpty().WithMessage("El código de restablecimiento es obligatorio.")
                .Length(6).WithMessage("El código de restablecimiento debe tener 6 caracteres.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("La nueva contraseña es obligatoria.")                
                .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_]).+$")
                .WithMessage("La nueva contraseña debe contener al menos una letra mayúscula, una letra minúscula, un dígito y un símbolo especial (@$!%*?&_).");
        }
    }
    
}
