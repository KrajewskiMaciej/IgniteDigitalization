using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace backend.Controllers
{
    // DTOs
    public class ResetPasswordRequest { public string Email { get; set; } = string.Empty; }
    public class ResetPasswordWithTokenRequest { public string Token { get; set; } = string.Empty; public string NewPassword { get; set; } = string.Empty; }

    [Route("api/password")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IAuthService _authService; // Logika przeniesiona do AuthService

        public PasswordController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequest request)
        {
            await _authService.InitiatePasswordResetAsync(request.Email);
            // Zawsze zwracamy OK, aby zapobiec "user enumeration"
            return Ok(new { success = true, message = "Jeśli konto istnieje, link do resetu hasła został wysłany." });
        }

        [HttpGet("validate-token/{token}")]
        public async Task<IActionResult> ValidateToken(string token)
        {
            var isValid = await _authService.IsPasswordResetTokenValidAsync(token);
            return Ok(new { valid = isValid });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithTokenRequest request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
                return Ok(new { success = true, message = "Hasło zresetowane pomyślnie." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}