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
    [RequireFumigator]
    public class FumigatorController : ControllerBase
    {
        private readonly CustomAuthService _authorizationService;
        private readonly IUserQuery _userQuery;
        private readonly IUserPutServices _userPutService;

        public FumigatorController(CustomAuthService authorizationService, IUserQuery userQuery, IUserPutServices userPutService)
        {
            _authorizationService = authorizationService;
            _userQuery = userQuery;
            _userPutService = userPutService;
        }

        /// <summary>
        /// Obtiene el perfil del fumigador autenticado
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
        /// Actualiza el perfil del fumigador autenticado
        /// </summary>
        /// <param name="request">Datos actualizados del fumigador</param>
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
                // (ya está garantizado por RequireFumigator, pero por seguridad adicional)
                
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
        /// Obtiene la agenda del fumigador autenticado
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

                // Implementar lógica para obtener agenda del fumigador
                return Ok(new GenericResponse { Message = "Agenda del fumigador" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los clientes del fumigador autenticado
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("clients")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> GetMyClients()
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                // Implementar lógica para obtener clientes del fumigador
                return Ok(new GenericResponse { Message = "Lista de clientes del fumigador" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial de fumigaciones de un cliente específico
        /// </summary>
        /// <param name="clientId">ID del client</param>
        /// <response code="200">Success</response>
        [HttpGet("client/{clientId}/fumigation-history")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 401)]
        [ProducesResponseType(typeof(ApiError), 403)]
        public async Task<IActionResult> GetClientFumigationHistory(int clientId)
        {
            try
            {
                var userId = _authorizationService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiError { Message = "Usuario no autenticado" });
                }

                // Verificar que el fumigador puede acceder a este cliente
                if (!_authorizationService.CanAccessUserData(clientId))
                {
                    return Forbid("No tienes permisos para acceder a este cliente");
                }

                // Implementar lógica para obtener historial de fumigaciones del cliente
                return Ok(new GenericResponse { Message = $"Historial de fumigaciones del cliente {clientId}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva cita
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

                // Implementar lógica para crear cita de servicio
                return CreatedAtAction(nameof(GetMySchedule), new GenericResponse { Message = "Cita de servicio creada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }
    }
}
