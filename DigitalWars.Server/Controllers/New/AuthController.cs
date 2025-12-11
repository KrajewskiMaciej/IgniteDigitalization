using DigitalWars.Server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DigitalWars.Server.Dtos;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService; // Używamy serwisu

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (user, claims) = await _authService.ValidateUserCredentialsAsync(request.Username, request.Password);
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return Ok(new { success = true, user = new { id = user.Users_Id, name = user.Names, email = user.Email } });
            }
            catch (Exception)
            {
                return BadRequest(CreateError("LOGIN_FAILED", "loginFailed"));
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterUserAsync(request.Username, request.Email, request.Password);
                if (result is ErrorResponseDto error)
                {
                    return Conflict(error);
                }
                return Ok(new { success = true, message = "Rejestracja pomyślna." });
            }
            catch (Exception)
            {
                return StatusCode(500, CreateError("INTERNAL_ERROR", "internalError"));
            }
        }

        [HttpGet("confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            try
            {
                await _authService.ConfirmUserEmailAsync(token);
                return Ok(new { success = true, message = "Email pomyślnie potwierdzony." });
            }
            catch
            {
                return BadRequest(CreateError("CONFIRMATION_FAILED", "confirmationFailed"));
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return Unauthorized();

            return Ok(new { id = user.Users_Id, name = user.Names, email = user.Email });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, message = "logoutSuccess" });
        }
    }
}