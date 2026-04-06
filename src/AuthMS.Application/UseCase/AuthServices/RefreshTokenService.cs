using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IRefreshTokenHasher _refreshTokenHasher;

        public RefreshTokenService(
            IAuthTokenService authTokenService,
            IRefreshTokenCommand refreshTokenCommand,
            IRefreshTokenQuery refreshTokenQuery,
            IUserQuery userQuery,
            ITimeProvider timeProvider,
            IRefreshTokenHasher refreshTokenHasher)
        {
            _authTokenService = authTokenService;            
            _refreshTokenCommand = refreshTokenCommand;
            _refreshTokenQuery = refreshTokenQuery;
            _userQuery = userQuery;
            _timeProvider = timeProvider;
            _refreshTokenHasher = refreshTokenHasher;
            _idleTimeoutMinutes = authTokenService.GetRefreshTokenIdleTimeoutInMinutes();
        }

        public async Task<LoginResponse> RefreshAccessToken(RefreshTokenRequest request)
        {
            var now = _timeProvider.Now;
            var tokenUserId = _authTokenService.GetUserIdFromExpiredAccessToken(request.ExpiredAccessToken);

            var refreshToken = await _refreshTokenQuery.GetByToken(request.RefreshToken);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new InvalidRefreshTokenException("El Refresh Token no existe o es invalido");
            }

            if (refreshToken.UserId != tokenUserId)
            {
                await _refreshTokenCommand.Delete(refreshToken);
                throw new InvalidRefreshTokenException("El Refresh Token no corresponde al usuario.");
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
                        
            var rawRefreshToken = await _authTokenService.GenerateRefreshToken();
            var newRefreshToken = new RefreshToken
            {
                Token = _refreshTokenHasher.Hash(rawRefreshToken),
                CreateDate = now,
                ExpireDate = refreshToken.ExpireDate,
                UserId = user.UserId,
                IsActive = true,
                LastUsed = now
            };

            await _refreshTokenCommand.RotateRefreshToken(refreshToken, newRefreshToken);            

            return new LoginResponse { AccessToken = accessToken, RefreshToken = rawRefreshToken, Result = true, Message = "Tokens renovados exitosamente." };
        }
    }
}
