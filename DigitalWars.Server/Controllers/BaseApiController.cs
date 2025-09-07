using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
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
    }
}