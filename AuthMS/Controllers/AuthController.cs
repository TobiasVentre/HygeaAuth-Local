using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IServices.IAuthServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthMS.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IPasswordResetService _passwordResetService;
        private readonly ILoginService _loginService;
        private readonly ILogoutService _logoutService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IEmailVerificationService _emailVerificationService;

        public AuthController(
            IPasswordResetService passwordResetService,
            ILoginService loginService,
            ILogoutService logoutService,
            IRefreshTokenService refreshTokenService,
            IEmailVerificationService emailVerificationService)
        {
            _passwordResetService = passwordResetService;
            _loginService = loginService;
            _logoutService = logoutService;
            _refreshTokenService = refreshTokenService;
            _emailVerificationService = emailVerificationService;
        }

        // -------------------------------
        // LOGIN
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var result = await _loginService.Login(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        // -------------------------------
        // LOGOUT
        // -------------------------------
        [Authorize]
        [HttpPost("Logout")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            try
            {
                var result = await _logoutService.Logout(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        // -------------------------------
        // REFRESH TOKEN
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            try
            {
                var result = await _refreshTokenService.RefreshAccessToken(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        // -------------------------------
        // CHANGE PASSWORD
        // -------------------------------
        [Authorize]
        [HttpPost("ChangePassword")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> ChangePassword(PasswordChangeRequest request)
        {
            try
            {
                var result = await _passwordResetService.ChangePassword(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        // -------------------------------
        // PASSWORD RESET - REQUEST
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("PasswordResetRequest")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> PasswordResetRequest(PasswordResetRequest request)
        {
            try
            {
                var result = await _passwordResetService.GenerateResetCode(request.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        // -------------------------------
        // PASSWORD RESET - CONFIRM
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("PasswordResetConfirm")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> PasswordResetConfirm(PasswordResetConfirmRequest request)
        {
            try
            {
                var result = await _passwordResetService.ValidateResetCode(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        // -------------------------------
        // VERIFY EMAIL
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("VerifyEmail")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> VerifyEmail(EmailVerificationRequest request)
        {
            try
            {
                var result = await _emailVerificationService.ValidateVerificationCode(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        // -------------------------------
        // RESEND VERIFICATION EMAIL
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("ResendVerificationEmail")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> ResendVerificationEmail(EmailResendVerificationRequest request)
        {
            try
            {
                var result = await _emailVerificationService.SendVerificationEmail(request.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }
    }
}