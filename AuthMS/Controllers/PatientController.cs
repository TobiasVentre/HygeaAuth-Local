using Application.Attributes;
using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IUserServices;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CustomAuthService = Application.Interfaces.IServices.IAuthorizationService;

namespace AuthMS.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePatient]
    public class PatientController : ControllerBase
    {
        private readonly CustomAuthService _authorizationService;
        private readonly IUserQuery _userQuery;
        private readonly IUserPutServices _userPutService;

        public PatientController(CustomAuthService authorizationService, IUserQuery userQuery, IUserPutServices userPutService)
        {
            _authorizationService = authorizationService;
            _userQuery = userQuery;
            _userPutService = userPutService;
        }

        /// <summary>
        /// Obtiene el perfil del paciente autenticado
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                var result = await _userQuery.GetUserById(userId.Value);
                return new JsonResult(result) { StatusCode = 200 };
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el perfil del paciente autenticado
        /// </summary>
        /// <param name="request">Datos actualizados del paciente</param>
        /// <response code="200">Success</response>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> UpdateMyProfile(UserUpdateRequest request)
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                // Validar que el usuario esté intentando actualizar su propio perfil
                // (ya está garantizado por RequirePatient, pero por seguridad adicional)
                
                // Actualizar el usuario usando el servicio
                var result = await _userPutService.UpdateUser(userId.Value, request);
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
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene las citas del paciente autenticado
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("appointments")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> GetMyAppointments()
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                // Implementar lógica para obtener citas del paciente
                return Ok(new GenericResponse { Message = "Lista de citas del paciente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial médico del paciente autenticado
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("medical-history")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> GetMyMedicalHistory()
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                // Implementar lógica para obtener historial médico
                return Ok(new GenericResponse { Message = "Historial médico del paciente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }
    }
}
