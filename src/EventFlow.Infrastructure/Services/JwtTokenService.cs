using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventFlow.Infrastructure.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> settings) : ITokenService
{
    private readonly JwtSettings _settings = settings.Value;

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("email", user.Email),
        };

        if (user.TenantId is { } tenantId)
            claims.Add(new Claim("tenant_id", tenantId.ToString()));

        if (user.Role is { } role)
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}

public sealed class JwtSettings:ITokenSettings
{
    public string SigningKey { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;
}