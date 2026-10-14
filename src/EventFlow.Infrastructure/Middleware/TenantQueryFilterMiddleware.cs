using EventFlow.Application.Common.Interfaces;
using EventFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace EventFlow.Infrastructure.Middleware;

public sealed class TenantQueryFilterMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ICurrentTenant currentTenant,
        AppDbContext db)
    {
        db.CurrentTenantId = currentTenant.TenantGuid;
        await next(context);
    }
}
