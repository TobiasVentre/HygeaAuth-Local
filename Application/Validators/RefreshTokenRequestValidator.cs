using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {            
            RuleFor(r => r.ExpiredAccessToken)
                .NotEmpty().WithMessage("El Access Token es obligatorio.");

            RuleFor(r => r.RefreshToken)
                .NotEmpty().WithMessage("El Refresh Token es obligatorio.");
        }
    }
}
