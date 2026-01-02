using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase.AuthServices
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IAuthTokenService _authTokenService;        
        private readonly IRefreshTokenCommand _refreshTokenCommand;
        private readonly IRefreshTokenQuery _refreshTokenQuery;
        private readonly IUserQuery _userQuery;
        private readonly int _idleTimeoutMinutes;
        private readonly ITimeProvider _timeProvider;

        public RefreshTokenService(IAuthTokenService authTokenService, IRefreshTokenCommand refreshTokenCommand, IRefreshTokenQuery refreshTokenQuery, IUserQuery userQuery, IConfiguration configuration, ITimeProvider timeProvider)
        {
            _authTokenService = authTokenService;            
            _refreshTokenCommand = refreshTokenCommand;
            _refreshTokenQuery = refreshTokenQuery;
            _userQuery = userQuery;
            _timeProvider = timeProvider;
            _idleTimeoutMinutes = configuration.GetValue<int>("RefreshTokenSettings:IdleTimeoutMinutes", 15);
        }

        public async Task<LoginResponse> RefreshAccessToken(RefreshTokenRequest request)
        {
            var now = _timeProvider.Now;
            var refreshToken = await _refreshTokenQuery.GetByToken(request.RefreshToken);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new InvalidRefreshTokenException("El Refresh Token no existe o es invalido");
            }

            if (refreshToken.ExpireDate < now)
            {
                await _refreshTokenCommand.Delete(refreshToken);
                throw new InvalidRefreshTokenException("El Refresh Token ha expirado.");
            }
                        
            var lastUsedAgo = now - refreshToken.LastUsed;
            if (lastUsedAgo.TotalMinutes > _idleTimeoutMinutes)
            {               
                await _refreshTokenCommand.Delete(refreshToken);
                throw new InvalidRefreshTokenException("El Refresh Token ha expirado debido a inactividad");
            }

            refreshToken.LastUsed = now;
            
            var user = await _userQuery.GetUserById(refreshToken.UserId);

            if (user == null)
            {
                throw new NotFoundException("No se encontró el usuario.");
            }

            var accessToken = await _authTokenService.GenerateAccessToken(user);
                        
            var newRefreshToken = new RefreshToken
            {
                Token = await _authTokenService.GenerateRefreshToken(),
                CreateDate = now,
                ExpireDate = refreshToken.ExpireDate,
                UserId = user.UserId,
                IsActive = true,
                LastUsed = now
            };

            await _refreshTokenCommand.RotateRefreshToken(refreshToken, newRefreshToken);            

            return new LoginResponse { AccessToken = accessToken, RefreshToken = newRefreshToken.Token, Result = true, Message = "Tokens renovados exitosamente." };
        }
    }
}
