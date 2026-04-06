using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AuthMS.Api.Security
{
    public class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
    {
        public ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

        public int? GetCurrentUserId()
        {
            var claimValue = User?.FindFirstValue(CustomClaims.UserId)
                             ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(claimValue, out var userId) ? userId : null;
        }
    }
}
