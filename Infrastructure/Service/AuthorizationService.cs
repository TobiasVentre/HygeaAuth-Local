using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace Infrastructure.Service
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool HasRole(string role)
        {
            var user = GetCurrentUser();
            return user?.IsInRole(role) == true;
        }

        public bool HasAnyRole(params string[] roles)
        {
            var user = GetCurrentUser();
            return user != null && roles.Any(role => user.IsInRole(role));
        }

        public int? GetCurrentUserId()
        {
            var user = GetCurrentUser();
            var claimValue = user?.FindFirstValue(CustomClaims.UserId)
                             ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(claimValue, out var userId) ? userId : null;
        }

        public string? GetCurrentUserRole()
        {
            var user = GetCurrentUser();
            return user?.FindFirstValue(ClaimTypes.Role)
                   ?? user?.FindFirstValue(CustomClaims.UserRole);
        }

        public bool CanAccessUserData(int targetUserId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return false;
            }

            if (currentUserId == targetUserId)
            {
                return true;
            }

            return HasRole(UserRoles.Fumigator);
        }

        public bool IsFumigator()
        {
            return HasRole(UserRoles.Fumigator);
        }

        public bool IsClient()
        {
            return HasRole(UserRoles.Client);
        }

        public ClaimsPrincipal? GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User;
        }
    }
}
