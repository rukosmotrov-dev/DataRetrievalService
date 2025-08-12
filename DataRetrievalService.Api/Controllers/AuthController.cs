using DataRetrievalService.Api.Contracts.Auth;
using DataRetrievalService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataRetrievalService.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto, CancellationToken ct)
        {
            var result = await _auth.AuthenticateAsync(dto.Email, dto.Password, ct);
            if (result is null) return Unauthorized();

            var (token, roles) = result.Value;
            return Ok(new LoginResponse { Token = token, Roles = roles });
        }
    }
}
