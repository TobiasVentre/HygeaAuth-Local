using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Application.Interfaces.IServices.ICryptographyService;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase.AuthServices
{
    public class LoginService : ILoginService
    {
        private readonly ICryptographyService _cryptographyService;
        private readonly IAuthTokenService _authTokenService;        
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;
        private readonly IRefreshTokenCommand _refreshTokenCommand;
        private readonly int _maxFailedAccessAttempts;
        private readonly int _lockoutDurationMinutes;
        private readonly int _lifeTimeInMinutes;
        private readonly ITimeProvider _timeProvider;

        public LoginService(ICryptographyService cryptographyService, IAuthTokenService authTokenService, IUserQuery userQuery, IUserCommand userCommand, IRefreshTokenCommand refreshTokenCommand, IConfiguration configuration, ITimeProvider timeProvider)
        {
            _cryptographyService = cryptographyService;
            _authTokenService = authTokenService;            
            _userQuery = userQuery;
            _userCommand = userCommand;
            _refreshTokenCommand = refreshTokenCommand;
            _timeProvider = timeProvider;
            _maxFailedAccessAttempts = configuration.GetValue<int>("LockoutSettings:MaxFailedAccessAttempts", 5);
            _lockoutDurationMinutes = configuration.GetValue<int>("LockoutSettings:LockoutDurationMinutes", 15);
            _lifeTimeInMinutes = configuration.GetValue<int>("RefreshTokenSettings:LifeTimeInMinutes", 60);
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var now = _timeProvider.Now;
            var user = await _userQuery.GetUserByEmail(request.Email);

            if (user == null)
            {
                throw new InvalidEmailException("Email y/o Contraseña incorrectos.");
            }

            if (!user.IsActive)
            {
                throw new InactiveUserException("La cuenta de usuario está inactiva.");
            }

            if (!user.IsEmailVerified)
            {
                throw new InvalidValueException("Debes confirmar tu cuenta mediante el código enviado a tu correo electrónico antes de poder iniciar sesión.");
            }

            if (user.LockoutEndDate.HasValue && user.LockoutEndDate.Value > now)
            {
                var minutesLeft = Math.Ceiling((user.LockoutEndDate.Value - now).TotalMinutes);
                throw new InvalidValueException($"La cuenta se encuentra temporalmente bloqueada por {minutesLeft} minutos.");
            }

            var passwordCorrect = await _cryptographyService.VerifyPassword(user.Password, request.Password);

            if (!passwordCorrect)
            {
                // Contraseña incorrecta: Incrementar AccessFailedCount
                user.AccessFailedCount++;

                if (user.AccessFailedCount >= _maxFailedAccessAttempts)
                {
                    user.LockoutEndDate = now.AddMinutes(_lockoutDurationMinutes);
                }

                await _userCommand.Update(user);

                throw new InvalidValueException("Email y/o Contraseña incorrectos.");
            }

            // Si la contraseña es correcta, resetear
            user.AccessFailedCount = 0;
            user.LockoutEndDate = null;
            await _userCommand.Update(user);

            //Generacion de Refresh Token
            var refreshToken = new RefreshToken
            {
                Token = await _authTokenService.GenerateRefreshToken(),
                CreateDate = now,
                ExpireDate = now.AddMinutes(_lifeTimeInMinutes),
                UserId = user.UserId,
                IsActive = true,
                LastUsed = now
            };

            await _refreshTokenCommand.Insert(refreshToken);

            var accessToken = await _authTokenService.GenerateAccessToken(user);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                Result = true,
                Message = "OK"
            };
        }
    }
}
