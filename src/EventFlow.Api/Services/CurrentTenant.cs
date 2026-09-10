using System.Security.Claims;
using EventFlow.Application.Common.Interfaces;

namespace EventFlow.Api.Services;

public class CurrentTenant(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public string? TenantId => httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId");
}