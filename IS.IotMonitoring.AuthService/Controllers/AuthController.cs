using IS.IotMonitoring.AuthService.DTOs;
using IS.IotMonitoring.AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace IS.IotMonitoring.AuthService.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService accountService;

        public AuthController(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var user = await accountService.RegisterAsync(request, cancellationToken);
            if (user is null)
                return Conflict($"Username '{request.Username}' is already taken.");

            return StatusCode(StatusCodes.Status201Created, user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var token = await accountService.LoginAsync(request, cancellationToken);
            if (token is null)
                return Unauthorized("Invalid username or password.");

            return Ok(token);
        }
    }
}
