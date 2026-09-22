using System.Security.Claims;
using EventFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventFlow.Infrastructure.Services;

public sealed class CurrentTenant(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public string? TenantId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");

    public Guid? TenantGuid =>
        Guid.TryParse(TenantId, out var id) ? id : null;
}