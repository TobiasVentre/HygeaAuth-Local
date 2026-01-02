using Application.Dtos.Request;
using Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IAuthServices
{
    public interface IPasswordResetService
    {
        Task<GenericResponse> ChangePassword(PasswordChangeRequest request);
        Task<GenericResponse> GenerateResetCode(string email);
        Task<GenericResponse> ValidateResetCode(PasswordResetConfirmRequest request);
    }
}
