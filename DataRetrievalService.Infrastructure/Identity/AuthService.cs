using DataRetrievalService.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DataRetrievalService.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _users;
    private readonly JwtOptions _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> users,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthService> logger)
    {
        _users = users;
        _jwt = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<(string token, IEnumerable<string> roles)?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var user = await _users.FindByEmailAsync(email);
        if (user is null) return null;

        var valid = await _users.CheckPasswordAsync(user, password);
        if (!valid) return null;

        var roles = await _users.GetRolesAsync(user);
        var claims = BuildClaims(user, roles);

        // Validate key once
        if (string.IsNullOrWhiteSpace(_jwt.Key))
        {
            _logger.LogError("JWT Key is not configured. Set Jwt:Key via user-secrets or environment variables.");
            throw new InvalidOperationException("JWT is not configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = _jwt.TokenExpiryMinutes > 0 ? _jwt.TokenExpiryMinutes : 60;

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenStr, roles);
    }

    private static IEnumerable<Claim> BuildClaims(IdentityUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        return claims;
    }
}
