using DataRetrievalService.Api.Contracts.Auth;
using DataRetrievalService.Api.Contracts.Data;
using DataRetrievalService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataRetrievalService.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest dto, CancellationToken ct)
    {
        var result = await _authService.AuthenticateAsync(dto.Email, dto.Password, ct);
        if (result is null) return Unauthorized();

        var (token, roles) = result.Value;
        return Ok(new LoginResponse { Token = token, Roles = roles });
    }
}
