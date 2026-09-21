using System.Security.Claims;
using EventFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventFlow.Infrastructure.Services;

public class CurrentTenant(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    Guid? ICurrentTenant.TenantId => httpContextAccessor.HttpContext?.User.FindFirstValue("tenantId") is { } tenantId
        && Guid.TryParse(tenantId, out var parsedTenantId)
        ? parsedTenantId
        : null;
}