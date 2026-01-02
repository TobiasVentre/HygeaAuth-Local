using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Application.Interfaces.IServices.ICryptographyService;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase.AuthServices
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;
        private readonly ICryptographyService _cryptographyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordResetCommand _passwordResetCommand;
        private readonly IPasswordResetQuery _passwordResetQuery;
        private readonly IEmailService _emailService;
        private readonly IResetCodeGenerator _resetCodeGenerator;

        public PasswordResetService(
            IUserQuery userQuery, 
            IUserCommand userCommand, 
            ICryptographyService cryptographyService, 
            IHttpContextAccessor httpContextAccessor, 
            IPasswordResetCommand passwordResetCommand,
            IPasswordResetQuery passwordResetQuery,
            IEmailService emailService,
            IResetCodeGenerator resetCodeGenerator
        )
        {
            _userQuery = userQuery;
            _userCommand = userCommand;
            _cryptographyService = cryptographyService;
            _httpContextAccessor = httpContextAccessor;
            _passwordResetCommand = passwordResetCommand;
            _passwordResetQuery = passwordResetQuery;
            _emailService = emailService;
            _resetCodeGenerator = resetCodeGenerator;
        }

        public async Task<GenericResponse> ChangePassword(PasswordChangeRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;            
            var userIdString = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdString == null || !int.TryParse(userIdString, out var userId))
            {
                throw new InvalidValueException("El usuario no está autenticado o el ID de usuario es inválido.");
            }

            var user = await _userQuery.GetUserById(userId);

            if (user == null)
            {
                throw new InvalidValueException("No se encontró el usuario.");
            }

            bool isPasswordValid = await _cryptographyService.VerifyPassword(user.Password, request.CurrentPassword);

            if (!isPasswordValid)
            {
                throw new BadRequestException("La contraseña actual es incorrecta.");
            }

            user.Password = await _cryptographyService.HashPassword(request.NewPassword);

            await _userCommand.Update(user);

            return new GenericResponse { Message = "¡Tu contraseña ha sido cambiada exitosamente!" };
        }

        public async Task<GenericResponse> GenerateResetCode(string email)
        {
            var expiredTokens = await _passwordResetQuery.GetExpiredTokensByEmail(email);
            foreach (var expired in expiredTokens)
            {
                await _passwordResetCommand.Delete(expired);
            }

            int lengthCode = 6;
            string resetCode = _resetCodeGenerator.GenerateResetCode(lengthCode);

            var passwordResetToken = new PasswordResetToken
            {
                Email = email,
                Token = resetCode,
                Expiration = DateTime.Now.AddMinutes(10) 
            };
            
            await _passwordResetCommand.Insert(passwordResetToken);            
            await _emailService.SendPasswordResetEmail(email, resetCode);

            return new GenericResponse { Message = "¡El código de restablecimiento de contraseña ha sido enviado a tu email!" };
        }

        public async Task<GenericResponse> ValidateResetCode(PasswordResetConfirmRequest request)
        {
            var token = await _passwordResetQuery.GetByEmailAndCode(request.Email, request.ResetCode);
            if (token == null || token.Expiration < DateTime.Now)
            {                
                throw new BadRequestException("El código no es válido o ha expirado.");
            }

            var user = await _userQuery.GetUserByEmail(request.Email);
            if (user == null)
            {                
                throw new NotFoundException("El usuario no existe.");
            }
            
            var hashedPassword = await _cryptographyService.HashPassword(request.NewPassword);           
            user.Password = hashedPassword;
            
            await _userCommand.Update(user);            
            await _passwordResetCommand.Delete(token);

            return new GenericResponse { Message = "¡Tu contraseña ha sido restablecida exitosamente!" };
        }
    }
}
