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
        int? GetCurrentUserId();

        /// <summary>
        /// Obtiene el rol del usuario actual desde los claims
        /// </summary>
        string? GetCurrentUserRole();

        /// <summary>
        /// Verifica si el usuario actual puede acceder a los datos de otro usuario
        /// </summary>
        bool CanAccessUserData(int targetUserId);


        /// <summary>
        /// Verifica si el usuario actual es médico
        /// </summary>
        bool IsDoctor();

        /// <summary>
        /// Verifica si el usuario actual es paciente
        /// </summary>
        bool IsPatient();

        /// <summary>
        /// Obtiene todos los claims del usuario actual
        /// </summary>
        ClaimsPrincipal? GetCurrentUser();
    }
}
