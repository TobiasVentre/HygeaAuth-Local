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
    [RequireClient]
    [Obsolete("Legacy controller. Client profile and operational history must migrate to DirectoryMS, SchedulingMS, and OrderMS.")]
    public class ClientController(
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

        [HttpGet("appointments")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public IActionResult GetMyAppointments()
        {
            if (authorizationService.GetCurrentUserId() == null)
            {
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
            }

            return Ok(new GenericResponse { Message = "Lista de citas del clientes" });
        }

        [HttpGet("medical-history")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public IActionResult GetMyMedicalHistory()
        {
            if (authorizationService.GetCurrentUserId() == null)
            {
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
            }

            return Ok(new GenericResponse { Message = "Historial de servicios del cliente" });
        }
    }
}
