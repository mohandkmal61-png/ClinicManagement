using ClinicManagement.Application.DTOs.Authentication;
using ClinicManagement.Application.Exceptions;
using ClinicManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(
            [FromBody] RegisterDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _authService.RegisterAsync(dto, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (AuthValidationException exception)
            {
                return BadRequest(new { errors = exception.Errors });
            }
        }

        [HttpPost("login")]
        [EnableRateLimiting("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login(
            [FromBody] LoginDto dto,
            CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(dto, cancellationToken);
            return response is null ? Unauthorized() : Ok(response);
        }
    }
}
