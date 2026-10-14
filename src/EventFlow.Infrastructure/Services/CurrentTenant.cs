using System.Security.Claims;
using EventFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventFlow.Infrastructure.Services;

public sealed class CurrentTenant : ICurrentTenant
{
    // Capture the HttpContext object while still inside the request flow.
    // Reading IHttpContextAccessor lazily returns null after HybridCache
    // moves execution to a different context.
    private readonly HttpContext? _httpContext;

    public CurrentTenant(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor.HttpContext;
    }

    public string? TenantId =>
        _httpContext?.User.FindFirstValue("tenant_id");

    public Guid? TenantGuid =>
        Guid.TryParse(TenantId, out var id) ? id : null;
}