using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventFlow.Infrastructure.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?
                .User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}