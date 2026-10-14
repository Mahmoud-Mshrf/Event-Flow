using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventFlow.Infrastructure.Services;

public sealed class CurrentUser : ICurrentUser
{
    private readonly HttpContext? _httpContext;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor.HttpContext;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContext?.User;

            var sub = user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        _httpContext?.User.Identities.Any(identity => identity.IsAuthenticated) ?? false;
}