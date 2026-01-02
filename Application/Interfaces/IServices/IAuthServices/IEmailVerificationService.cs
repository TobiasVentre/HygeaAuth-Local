using Application.Dtos.Request;
using Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IAuthServices
{
    public interface IEmailVerificationService
    {
        Task<GenericResponse> ValidateVerificationCode(EmailVerificationRequest request);
        Task<GenericResponse> SendVerificationEmail(string email);
    }
}
