using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DigitalWars.Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace DigitalWars.Server.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("2.0")]
    public abstract class BaseApiController : ControllerBase
    {
        // Sprawdza czy to Admin (brak claima Teams_Id)
        protected bool IsAdmin => User.Identity?.IsAuthenticated == true && !User.HasClaim(c => c.Type == "Teams_Id");

        protected int? CurrentUserId
        {
            get
            {
                if (!IsAdmin) return null;

                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out var userId)) return userId;
                return null;
            }
        }
        private int? TokenTeamId
        {
            get
            {
                var teamIdStr = User.FindFirstValue("Teams_Id");
                if (int.TryParse(teamIdStr, out var teamId)) return teamId;
                return null;
            }
        }
        protected int ResolveTeamId(int? requestTeamId)
        {
            if (IsAdmin)
            {
                if (requestTeamId.HasValue)
                {
                    return requestTeamId.Value;
                }
                throw new ArgumentException("Jako Admin musisz podać 'teamId' w parametrach zapytania lub body.");
            }
            else
            {
                return TokenTeamId ?? throw new UnauthorizedAccessException("Błąd tokena drużyny.");
            }
        }

        protected ErrorResponseDto CreateError(string code, string message)
        {
            return new ErrorResponseDto { ErrorCode = code, Message = message };
        }
    }
}