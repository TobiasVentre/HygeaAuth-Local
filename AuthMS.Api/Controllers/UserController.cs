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

        [Authorize(Policy = "CanEditOwnProfile")]
        [HttpPut("{Id}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 404)]
        public async Task<IActionResult> UpdateUser(int Id, UserUpdateRequest request)
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
        public async Task<IActionResult> RemoveUserImage(int Id)
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
        public async Task<IActionResult> GetUserById([FromRoute] int id)
            => new JsonResult(await userGetService.GetUserById(id)) { StatusCode = StatusCodes.Status200OK };

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

        private int? GetCurrentUserId()
        {
            var claimValue = User?.FindFirstValue(CustomClaims.UserId)
                             ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(claimValue, out var userId) ? userId : null;
        }
    }
}
