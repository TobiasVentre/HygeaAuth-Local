using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class PasswordChangeRequestValidator : AbstractValidator<PasswordChangeRequest>
    {
        public PasswordChangeRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("La contraseña actual es obligatoria.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("La nueva contraseña es obligatoria.")
                .Must((request, newPassword) => newPassword != request.CurrentPassword)
                .WithMessage("La nueva contraseña no puede ser igual a la contraseña actual.")
                .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_]).+$")
                .WithMessage("La nueva contraseña debe contener al menos una letra mayúscula, una letra minúscula, un dígito y un símbolo especial (@$!%*?&_).");
        }
    }    
}
