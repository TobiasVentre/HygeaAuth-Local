using Application.Attributes;
using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IServices;
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
    [RequireDoctor]
    public class DoctorController : ControllerBase
    {
        private readonly CustomAuthService _authorizationService;
        private readonly IUserQuery _userQuery;
        private readonly IUserPutServices _userPutService;

        public DoctorController(CustomAuthService authorizationService, IUserQuery userQuery, IUserPutServices userPutService)
        {
            _authorizationService = authorizationService;
            _userQuery = userQuery;
            _userPutService = userPutService;
        }

        /// <summary>
        /// Obtiene el perfil del médico autenticado
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
        /// Actualiza el perfil del médico autenticado
        /// </summary>
        /// <param name="request">Datos actualizados del médico</param>
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
                // (ya está garantizado por RequireDoctor, pero por seguridad adicional)
                
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
        /// Obtiene la agenda del médico autenticado
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("schedule")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> GetMySchedule()
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                // Implementar lógica para obtener agenda del médico
                return Ok(new GenericResponse { Message = "Agenda del médico" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los pacientes del médico autenticado
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("patients")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> GetMyPatients()
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                // Implementar lógica para obtener pacientes del médico
                return Ok(new GenericResponse { Message = "Lista de pacientes del médico" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial médico de un paciente específico
        /// </summary>
        /// <param name="patientId">ID del paciente</param>
        /// <response code="200">Success</response>
        [HttpGet("patients/{patientId}/medical-history")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        [ProducesResponseType(typeof(ApiError), 403)]
        public async Task<IActionResult> GetPatientMedicalHistory(int patientId)
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                // Verificar que el médico puede acceder a este paciente
                if (!_authorizationService.CanAccessUserData(patientId))
                {
                    return Forbid("No tienes permisos para acceder a este paciente");
                }

                // Implementar lógica para obtener historial médico del paciente
                return Ok(new GenericResponse { Message = $"Historial médico del paciente {patientId}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva cita médica
        /// </summary>
        /// <param name="request">Datos de la cita</param>
        /// <response code="201">Success</response>
        [HttpPost("appointments")]
        [ProducesResponseType(typeof(GenericResponse), 201)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> CreateAppointment([FromBody] object request)
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                // Implementar lógica para crear cita médica
                return CreatedAtAction(nameof(GetMySchedule), new GenericResponse { Message = "Cita médica creada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }
    }
}
