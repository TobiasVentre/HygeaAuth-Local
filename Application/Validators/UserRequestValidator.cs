using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class UserRequestValidator : AbstractValidator<UserRequest>
    {
        public UserRequestValidator()
        {
            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("El nombre del usuario es obligatorio.")
                .MinimumLength(1).WithMessage("El nombre del usuario debe tener al menos 1 carácter.");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("El apellido del usuario es obligatorio.")
                .MinimumLength(1).WithMessage("El apellido del usuario debe tener al menos 1 carácter.");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .EmailAddress().WithMessage("El email no es válido.");

            RuleFor(u => u.Dni)
                .NotEmpty().WithMessage("El DNI es obligatorio.")
                .Matches(@"^[A-Za-z0-9]{6,12}$").WithMessage("El DNI debe contener entre 6 y 12 caracteres alfanuméricos.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_]).+$")
                .WithMessage("La contraseña debe contener al menos una letra mayúscula, una letra minúscula, un dígito y un símbolo especial (@$!%*?&_).");

            // El rol es opcional, pero si se proporciona debe ser válido
            RuleFor(u => u.Role)
                .Must(role => string.IsNullOrWhiteSpace(role) || role == "Patient" || role == "Doctor")
                .WithMessage("El rol debe ser 'Patient' o 'Doctor' si se proporciona.");

           
        }
    }
}
