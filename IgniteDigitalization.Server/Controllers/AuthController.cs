using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Dtos;

namespace backend.Controllers
{
    // DTOs for Auth
    public class LoginRequest { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class RegisterRequest { public string Username { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }

    [Route("api/auth")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService; // Używamy serwisu

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var (user, claims) = await _authService.ValidateUserCredentialsAsync(request.Username, request.Password, baseUrl);

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return Ok(new { success = true, user = new { id = user.Users_Id, name = user.Names, email = user.Email } });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var result = await _authService.RegisterUserAsync(request.Username, request.Email, request.Password, baseUrl);

                if (result is ErrorResponseDto error)
                {
                    return Conflict(error);
                }

                return Ok(new { success = true, message = "Rejestracja pomyślna. Sprawdź email, aby aktywować konto." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Wystąpił nieoczekiwany błąd serwera.{ex}");
            }
        }

        [HttpGet("confirm/{token}")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            try
            {
                await _authService.ConfirmUserEmailAsync(token);
                return Ok(new { success = true, message = "Email pomyślnie potwierdzony." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return Unauthorized();

            return Ok(new { id = user.Users_Id, name = user.Names, email = user.Email });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, message = "Wylogowano pomyślnie" });
        }
    }
}