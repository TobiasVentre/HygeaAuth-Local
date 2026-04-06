using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IUserServices;
using AuthMS.Api.Attributes;
using Microsoft.AspNetCore.Mvc;
using CustomAuthService = Application.Interfaces.IServices.IAuthorizationService;

namespace AuthMS.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequireTechnician]
    [Obsolete("Legacy controller. Technician profile, schedule, and service history must migrate to DirectoryMS, SchedulingMS, and OrderMS.")]
    public class TechnicianController(
        CustomAuthService authorizationService,
        IUserQuery userQuery,
        IUserPutServices userPutService) : ControllerBase
    {
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = authorizationService.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
            }

            return new JsonResult(await userQuery.GetUserById(userId.Value)) { StatusCode = StatusCodes.Status200OK };
        }

        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> UpdateMyProfile(UserUpdateRequest request)
        {
            var userId = authorizationService.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
            }

            return new JsonResult(await userPutService.UpdateUser(userId.Value, request)) { StatusCode = StatusCodes.Status200OK };
        }

        [HttpGet("schedule")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public IActionResult GetMySchedule()
        {
            if (authorizationService.GetCurrentUserId() == null)
            {
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
            }

            return Ok(new GenericResponse { Message = "Agenda del técnico" });
        }

        [HttpGet("clients")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public IActionResult GetMyClients()
        {
            if (authorizationService.GetCurrentUserId() == null)
            {
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
            }

            return Ok(new GenericResponse { Message = "Lista de clientes del técnico" });
        }

        [HttpGet("client/{clientId}/service-history")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        [ProducesResponseType(typeof(ApiError), 403)]
        public IActionResult GetClientFumigationHistory(Guid clientId)
        {
            if (authorizationService.GetCurrentUserId() == null)
            {
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
            }

            if (!authorizationService.CanAccessUserData(clientId))
            {
                return Forbid("No tienes permisos para acceder a este cliente");
            }

            return Ok(new GenericResponse { Message = $"Historial de servicios del cliente {clientId}" });
        }

        [HttpPost("appointments")]
        [ProducesResponseType(typeof(GenericResponse), 201)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public IActionResult CreateAppointment([FromBody] object request)
        {
            if (authorizationService.GetCurrentUserId() == null)
            {
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
            }

            return CreatedAtAction(nameof(GetMySchedule), new GenericResponse { Message = "Cita de servicio creada exitosamente" });
        }
    }
}
