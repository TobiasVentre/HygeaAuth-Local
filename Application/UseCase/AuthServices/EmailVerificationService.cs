using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase.AuthServices
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;
        private readonly IEmailVerificationQuery _emailVerificationQuery;
        private readonly IEmailVerificationCommand _emailVerificationCommand;
        private readonly IEmailService _emailService;
        private readonly IResetCodeGenerator _resetCodeGenerator;
        private readonly ILogger<EmailVerificationService> _logger;

        public EmailVerificationService(
            IUserQuery userQuery,
            IUserCommand userCommand,
            IEmailVerificationQuery emailVerificationQuery,
            IEmailVerificationCommand emailVerificationCommand,
            IEmailService emailService,
            IResetCodeGenerator resetCodeGenerator,
            ILogger<EmailVerificationService> logger
        )
        {
            _userQuery = userQuery;
            _userCommand = userCommand;
            _emailVerificationQuery = emailVerificationQuery;
            _emailVerificationCommand = emailVerificationCommand;
            _emailService = emailService;
            _resetCodeGenerator = resetCodeGenerator;
            _logger = logger;
        }

        public async Task<GenericResponse> ValidateVerificationCode(EmailVerificationRequest request)
        {
            _logger.LogInformation("Validando código de verificación para email: {Email}, código: {Code}", request.Email, request.VerificationCode);
            
            // Buscar el token de verificación por email y código (la comparación debe ser case-sensitive)
            var token = await _emailVerificationQuery.GetByEmailAndCode(request.Email, request.VerificationCode);
            
            if (token == null)
            {
                _logger.LogWarning("Token no encontrado para email: {Email}, código: {Code}", request.Email, request.VerificationCode);
                throw new BadRequestException("El código no es válido o ha expirado.");
            }

            var now = DateTime.UtcNow;
            _logger.LogInformation("Token encontrado. Expiración: {Expiration}, Ahora (UTC): {Now}, Diferencia: {Diff} minutos", 
                token.Expiration, now, (token.Expiration - now).TotalMinutes);

            if (token.Expiration < now)
            {
                _logger.LogWarning("Token expirado. Expiración: {Expiration}, Ahora: {Now}", token.Expiration, now);
                throw new BadRequestException("El código no es válido o ha expirado.");
            }

            // Buscar el usuario asociado al email
            var user = await _userQuery.GetUserByEmail(request.Email);
            if (user == null)
            {
                throw new NotFoundException("El usuario no existe.");
            }

            // Verificar el email de la cuenta
            user.IsEmailVerified = true;
            await _userCommand.Update(user);

            // Eliminar el token de verificación tras su uso
            await _emailVerificationCommand.Delete(token);

            return new GenericResponse { Message = "¡Tu cuenta ha sido activada exitosamente!" };
        }

        public async Task<GenericResponse> SendVerificationEmail(string email)
        {
            // Limpia tokens expirados para el email
            var expiredTokens = await _emailVerificationQuery.GetExpiredTokensByEmail(email);
            foreach (var expired in expiredTokens)
            {
                await _emailVerificationCommand.Delete(expired);
            }

            // Genera un nuevo código de verificación (por ejemplo, de 6 caracteres)
            int lengthCode = 6;
            string verificationCode = _resetCodeGenerator.GenerateResetCode(lengthCode);

            var expirationTime = DateTime.UtcNow.AddMinutes(15);
            _logger.LogInformation("Generando código de verificación para {Email}. Código: {Code}, Expira: {Expiration}", 
                email, verificationCode, expirationTime);
            
            var token = new EmailVerificationToken
            {
                Email = email,
                Token = verificationCode,
                Expiration = expirationTime
            };

            await _emailVerificationCommand.Insert(token);
            await _emailService.SendEmailVerification(email, verificationCode);

            return new GenericResponse { Message = "El código de verificación ha sido enviado." };
        }
    }
}
