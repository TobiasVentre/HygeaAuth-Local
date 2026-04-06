using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces.IServices.IAuthServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthMS.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController(
        IPasswordResetService passwordResetService,
        ILoginService loginService,
        ILogoutService logoutService,
        IRefreshTokenService refreshTokenService,
        IEmailVerificationService emailVerificationService) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> Login(LoginRequest request)
            => Ok(await loginService.Login(request));

        [Authorize]
        [HttpPost("Logout")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> Logout(LogoutRequest request)
            => Ok(await logoutService.Logout(request));

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
            => Ok(await refreshTokenService.RefreshAccessToken(request));

        [Authorize]
        [HttpPost("ChangePassword")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> ChangePassword(PasswordChangeRequest request)
            => Ok(await passwordResetService.ChangePassword(request));

        [AllowAnonymous]
        [HttpPost("PasswordResetRequest")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> PasswordResetRequest(PasswordResetRequest request)
            => Ok(await passwordResetService.GenerateResetCode(request.Email));

        [AllowAnonymous]
        [HttpPost("PasswordResetConfirm")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> PasswordResetConfirm(PasswordResetConfirmRequest request)
            => Ok(await passwordResetService.ValidateResetCode(request));

        [AllowAnonymous]
        [HttpPost("VerifyEmail")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> VerifyEmail(EmailVerificationRequest request)
            => Ok(await emailVerificationService.ValidateVerificationCode(request));

        [AllowAnonymous]
        [HttpPost("ResendVerificationEmail")]
        [ProducesResponseType(typeof(GenericResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> ResendVerificationEmail(EmailResendVerificationRequest request)
            => Ok(await emailVerificationService.SendVerificationEmail(request.Email));
    }
}
