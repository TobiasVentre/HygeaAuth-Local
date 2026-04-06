using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces.IServices.IUserServices;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthMS.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(
        IUserPostServices userPostService,
        IUserPutServices userPutService,
        IUserPatchServices userPatchService,
        IUserGetServices userGetService) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), 201)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> RegisterUser(UserRequest request)
            => new JsonResult(await userPostService.Register(request)) { StatusCode = StatusCodes.Status201Created };

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("provider-admins")]
        [ProducesResponseType(typeof(UserResponse), 201)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 403)]
        public async Task<IActionResult> RegisterProviderAdmin([FromBody] ProviderAdminCreateRequest request)
            => new JsonResult(await userPostService.RegisterProviderAdmin(request)) { StatusCode = StatusCodes.Status201Created };

        [Authorize(Policy = "ProviderAdminOnly")]
        [HttpPost("provider/technicians")]
        [ProducesResponseType(typeof(UserResponse), 201)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 403)]
        public async Task<IActionResult> RegisterTechnicianForProvider([FromBody] ProviderTechnicianCreateRequest request)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Forbid("No se pudo resolver el usuario autenticado.");
            }

            return new JsonResult(await userPostService.RegisterTechnicianForProvider(request, currentUserId.Value))
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        [Authorize(Policy = "CanEditOwnProfile")]
        [HttpPut("{Id}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 404)]
        public async Task<IActionResult> UpdateUser(Guid Id, UserUpdateRequest request)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null || currentUserId.Value != Id)
            {
                return Forbid("No tienes permisos para editar este perfil.");
            }

            return new JsonResult(await userPutService.UpdateUser(Id, request)) { StatusCode = StatusCodes.Status200OK };
        }

        [Authorize(Policy = "CanEditOwnProfile")]
        [HttpPatch("RemoveImage/{Id}")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> RemoveUserImage(Guid Id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null || currentUserId.Value != Id)
            {
                return Forbid("No tienes permisos para editar este perfil.");
            }

            return new JsonResult(await userPatchService.RemoveUserImage(Id)) { StatusCode = StatusCodes.Status200OK };
        }

        [Authorize(Policy = "CanViewClientInfo")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 404)]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
            => new JsonResult(await userGetService.GetUserById(id)) { StatusCode = StatusCodes.Status200OK };

        [Authorize(Policy = "CanViewTechnicianInfo")]
        [HttpGet("technicians/{id}")]
        [ProducesResponseType(typeof(TechnicianPublicProfileResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 404)]
        public async Task<IActionResult> GetTechnicianPublicProfile([FromRoute] Guid id)
            => new JsonResult(await userGetService.GetTechnicianPublicProfileById(id)) { StatusCode = StatusCodes.Status200OK };

        [Authorize(Policy = "CanViewTechnicianInfo")]
        [HttpGet("technicians/info")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public IActionResult GetTechniciansInfo()
            => Ok(new GenericResponse { Message = "Información de técnicos obtenida exitosamente" });

        [Authorize(Policy = "ActiveUser")]
        [HttpGet("me")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public IActionResult GetMyInfo()
            => Ok(new GenericResponse { Message = "Información personal obtenida exitosamente" });

        private Guid? GetCurrentUserId()
        {
            var claimValue = User?.FindFirstValue(CustomClaims.UserId)
                             ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(claimValue, out var userId) ? userId : null;
        }
    }
}
