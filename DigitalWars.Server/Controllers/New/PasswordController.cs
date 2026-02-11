using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalWars.Server.Dtos;
using Asp.Versioning;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class PasswordController : BaseApiController
    {
        private readonly IAuthService _authService;

        public PasswordController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("validate-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateToken([FromQuery] string token)
        {
            var isValid = await _authService.IsPasswordResetTokenValidAsync(token);
            return Ok(new { valid = isValid });
        }

        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithTokenRequest request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
                return Ok(new { success = true, message = "Hasło zresetowane." });
            }
            catch
            {
                return BadRequest(CreateError("RESET_FAILED", "resetFailed"));
            }
        }

        [HttpPost("reset-request")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.InitiatePasswordResetAsync(request.Email);
            if (!result)
                return Conflict(CreateError("USER_NOT_FOUND", "userNotFound"));

            return Ok(new { success = true, message = "Link wysłany." });
        }
    }
}