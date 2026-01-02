using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IAuthServices
{
    public interface IAuthTokenService
    {
        Task<string> GenerateAccessToken(User user);
        Task<string> GenerateRefreshToken();
        Task<int> GetRefreshTokenLifetimeInMinutes();        
    }
}
