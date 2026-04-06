using Application.Interfaces.IServices;
using Domain.Entities;
using System.Security.Claims;
using System.Linq;

namespace Infrastructure.Service
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ICurrentUserContext _currentUserContext;

        public AuthorizationService(ICurrentUserContext currentUserContext)
        {
            _currentUserContext = currentUserContext;
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

        public Guid? GetCurrentUserId()
        {
            var user = GetCurrentUser();
            var claimValue = user?.FindFirst(CustomClaims.UserId)?.Value
                             ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(claimValue, out var userId) ? userId : null;
        }

        public string? GetCurrentUserRole()
        {
            var user = GetCurrentUser();
            return user?.FindFirst(ClaimTypes.Role)?.Value
                   ?? user?.FindFirst(CustomClaims.UserRole)?.Value;
        }

        public bool CanAccessUserData(Guid targetUserId)
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

            return HasRole(UserRoles.Technician);
        }

        public bool IsTechnician()
        {
            return HasRole(UserRoles.Technician);
        }

        public bool IsClient()
        {
            return HasRole(UserRoles.Client);
        }

        public ClaimsPrincipal? GetCurrentUser()
        {
            return _currentUserContext.User;
        }
    }
}
