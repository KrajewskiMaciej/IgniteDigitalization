using DigitalWars.Server.Dtos;
using DigitalWars.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class TeamAuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public TeamAuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] TeamLoginRequestDto request)
        {
            try
            {
                var (teamInfo, claims) = await _authService.ValidateTeamTokenAsync(request.Token);

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return Ok(new { success = true });
            }
            catch (Exception)
            {
                return Unauthorized(CreateError("INVALID_TOKEN", "invalidToken"));
            }
        }
    }
}