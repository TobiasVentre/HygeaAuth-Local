using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {            
            RuleFor(l => l.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .EmailAddress().WithMessage("Email y/o Contraseña incorrectos");

            RuleFor(l => l.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.");
        }
    }
}
