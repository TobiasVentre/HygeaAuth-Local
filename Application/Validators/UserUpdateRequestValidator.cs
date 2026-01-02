using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators 
{
    public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
    {
        public UserUpdateRequestValidator()
        {
            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("El nombre del usuario es obligatorio.")
                .MinimumLength(1).WithMessage("El nombre del usuario debe tener al menos 1 carácter.");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("El apellido del usuario es obligatorio.")
                .MinimumLength(1).WithMessage("El apellido del usuario debe tener al menos 1 carácter.");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress().WithMessage("El correo electrónico no es válido.");

            RuleFor(u => u.Dni)
                .NotEmpty().WithMessage("El DNI es obligatorio.")
                .Matches(@"^[A-Za-z0-9]{6,12}$").WithMessage("El DNI debe contener entre 6 y 12 caracteres alfanuméricos.");
        }
    }
}
