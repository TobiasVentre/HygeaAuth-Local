using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IServices.IUserServices;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AuthMS.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserPostServices _userPostService;
        private readonly IUserPutServices _userPutService;
        private readonly IUserPatchServices _userPatchService;
        private readonly IUserGetServices _userGetService;

        public UserController(IUserPostServices userPostService, IUserPutServices userPutService, IUserPatchServices userPatchService, IUserGetServices userGetService)
        {
            _userPostService = userPostService;
            _userPutService = userPutService;
            _userPatchService = userPatchService;
            _userGetService = userGetService;

        }

        /// <summary>
        /// Creates a new user with the provided details.
        /// </summary> 
        /// <param name="request">The details of the user to be created.</param>
        /// <response code="201">Success</response>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), 201)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> RegisterUser(UserRequest request)
        {
            // Si hay errores de validación de ModelState (de FluentValidationAutoValidation)
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();
                
                var errorMessage = string.Join("; ", errors);
                return BadRequest(new ApiError { Message = errorMessage });
            }
            
            try
            {
                var result = await _userPostService.Register(request);
                return new JsonResult(result) { StatusCode = 201 };
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing user with the specified details.
        /// </summary>
        /// <param name="Id">The unique identifier of the user to be updated.</param>
        /// <param name="request">The updated details of the user.</param>
        /// <response code="200">Success</response>
        [Authorize(Policy = "CanEditOwnProfile")]
        [HttpPut("{Id}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 404)]
        public async Task<IActionResult> UpdateUser(int Id, UserUpdateRequest request)
        {
            try
            {
                var result = await _userPutService.UpdateUser(Id, request);
                return new JsonResult(result) { StatusCode = 200 };
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Remove the image of a user with the specified Id.
        /// </summary>
        /// <param name="Id"> The unique identifier of the user whose image is to be removed.</param>
        /// <response code="200">Success</response> 
        [Authorize(Policy = "CanEditOwnProfile")]
        [HttpPatch("RemoveImage/{Id}")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> RemoveUserImage(int Id)
        {
            try
            {
                var result = await _userPatchService.RemoveUserImage(Id);
                return new JsonResult(result) { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }


        /// <summary>
        /// Recupera información detallada sobre un usuario específico a partir de su ID.
        /// </summary>
        /// <param name="id">ID único del usuario</param>
        /// <response code="200">Success</response>
        [Authorize(Policy = "CanViewPatientInfo")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 404)]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            try
            {
                var result = await _userGetService.GetUserById(id);
                return new JsonResult(result) { StatusCode = 200 };
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {               
                return NotFound(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene información básica de doctores (ejemplo de uso de políticas)
        /// </summary>
        /// <response code="200">Success</response>
        [Authorize(Policy = "CanViewDoctorInfo")]
        [HttpGet("doctors/info")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> GetDoctorsInfo()
        {
            try
            {
                // Ejemplo de endpoint que requiere política específica
                return Ok(new GenericResponse { Message = "Información de doctores obtenida exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene información del usuario autenticado (ejemplo de uso de claims)
        /// </summary>
        /// <response code="200">Success</response>
        [Authorize(Policy = "ActiveUser")]
        [HttpGet("me")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> GetMyInfo()
        {
            try
            {
                // Ejemplo de endpoint que requiere usuario activo
                return Ok(new GenericResponse { Message = "Información personal obtenida exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }
    }
}
