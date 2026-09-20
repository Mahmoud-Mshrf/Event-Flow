using System.Security.Claims;
using EventFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventFlow.Infrastructure.Services;

public class CurrentTenant(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public string? TenantId => httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId");
}