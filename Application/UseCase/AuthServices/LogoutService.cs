using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IAuthServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase.AuthServices
{
    public class LogoutService : ILogoutService
    {              
        private readonly IRefreshTokenCommand _refreshTokenCommand;
        private readonly IRefreshTokenQuery _refreshTokenQuery;

        public LogoutService(IRefreshTokenCommand refreshTokenCommand, IRefreshTokenQuery refreshTokenQuery)
        {                      
            _refreshTokenCommand = refreshTokenCommand;
            _refreshTokenQuery = refreshTokenQuery;
        }

        public async Task<GenericResponse> Logout(LogoutRequest request)
        {
            //Se debe eliminar el refreshToken para desloguear de dicha seccion.
            var refreshToken = await _refreshTokenQuery.GetByToken(request.RefreshToken);

            if (refreshToken == null)
            {
                throw new NotFoundException("No se encontró el Refresh Token.");
            }

            await _refreshTokenCommand.Delete(refreshToken);

            return new GenericResponse { Message = "Cierre de sesión exitoso." };
        }
    }
}
