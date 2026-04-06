using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Application.Interfaces.IServices.ICryptographyService;
using Domain.Entities;

namespace Application.UseCase.AuthServices
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;
        private readonly ICryptographyService _cryptographyService;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IPasswordResetCommand _passwordResetCommand;
        private readonly IPasswordResetQuery _passwordResetQuery;
        private readonly IEmailService _emailService;
        private readonly IResetCodeGenerator _resetCodeGenerator;
        private readonly ITimeProvider _timeProvider;

        public PasswordResetService(
            IUserQuery userQuery, 
            IUserCommand userCommand, 
            ICryptographyService cryptographyService, 
            ICurrentUserContext currentUserContext, 
            IPasswordResetCommand passwordResetCommand,
            IPasswordResetQuery passwordResetQuery,
            IEmailService emailService,
            IResetCodeGenerator resetCodeGenerator,
            ITimeProvider timeProvider
        )
        {
            _userQuery = userQuery;
            _userCommand = userCommand;
            _cryptographyService = cryptographyService;
            _currentUserContext = currentUserContext;
            _passwordResetCommand = passwordResetCommand;
            _passwordResetQuery = passwordResetQuery;
            _emailService = emailService;
            _resetCodeGenerator = resetCodeGenerator;
            _timeProvider = timeProvider;
        }

        public async Task<GenericResponse> ChangePassword(PasswordChangeRequest request)
        {
            var userId = _currentUserContext.GetCurrentUserId();
            if (userId is null)
            {
                throw new InvalidValueException("El usuario no está autenticado o el ID de usuario es inválido.");
            }

            var user = await _userQuery.GetUserById(userId.Value);

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
            var now = _timeProvider.Now;

            var passwordResetToken = new PasswordResetToken
            {
                Email = email,
                Token = resetCode,
                Expiration = now.AddMinutes(10)
            };
            
            await _passwordResetCommand.Insert(passwordResetToken);            
            await _emailService.SendPasswordResetEmail(email, resetCode);

            return new GenericResponse { Message = "¡El código de restablecimiento de contraseña ha sido enviado a tu email!" };
        }

        public async Task<GenericResponse> ValidateResetCode(PasswordResetConfirmRequest request)
        {
            var now = _timeProvider.Now;
            var token = await _passwordResetQuery.GetByEmailAndCode(request.Email, request.ResetCode);
            if (token == null || token.Expiration < now)
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
