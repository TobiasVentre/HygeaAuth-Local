using Domain.Entities;
using System.Security.Claims;

namespace Application.Interfaces.IServices
{
    /// <summary>
    /// Servicio para manejar autorización y permisos por roles
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Verifica si el usuario actual tiene el rol especificado
        /// </summary>
        bool HasRole(string role);

        /// <summary>
        /// Verifica si el usuario actual tiene alguno de los roles especificados
        /// </summary>
        bool HasAnyRole(params string[] roles);

        /// <summary>
        /// Obtiene el ID del usuario actual desde los claims
        /// </summary>
        Guid? GetCurrentUserId();

        /// <summary>
        /// Obtiene el rol del usuario actual desde los claims
        /// </summary>
        string? GetCurrentUserRole();

        /// <summary>
        /// Verifica si el usuario actual puede acceder a los datos de otro usuario
        /// </summary>
        bool CanAccessUserData(Guid targetUserId);


        /// <summary>
        /// Verifica si el usuario actual es técnico
        /// </summary>
        bool IsTechnician();

        /// <summary>
        /// Verifica si el usuario actual es cliente
        /// </summary>
        bool IsClient();

        /// <summary>
        /// Obtiene todos los claims del usuario actual
        /// </summary>
        ClaimsPrincipal? GetCurrentUser();
    }
}
