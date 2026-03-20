using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

[ApiController]
[Route("api/test")]
[ApiVersion("1.0")]
[AllowAnonymous]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult GetMessage()
    {
        return Ok(new { message = "Hello from .NET API" });
    }
}