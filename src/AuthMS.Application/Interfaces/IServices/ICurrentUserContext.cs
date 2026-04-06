using System.Security.Claims;

namespace Application.Interfaces.IServices
{
    public interface ICurrentUserContext
    {
        ClaimsPrincipal? User { get; }

        Guid? GetCurrentUserId();
    }
}
