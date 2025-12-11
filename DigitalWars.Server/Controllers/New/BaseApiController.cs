using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DigitalWars.Server.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace DigitalWars.Server.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("2.0")]
    public abstract class BaseApiController : ControllerBase
    {
        protected int? CurrentUserId
        {
            get
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out var userId))
                {
                    return userId;
                }
                return null;
            }
        }

        protected ErrorResponseDto CreateError(string code, string message)
        {
            return new ErrorResponseDto
            {
                ErrorCode = code,
                Message = message
            };
        }
    }
}