using HospitalTransport.Application.DTOs.Auth;
using HospitalTransport.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTransport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            var result = await _authService.ValidateTokenAsync(token);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
    }
}
