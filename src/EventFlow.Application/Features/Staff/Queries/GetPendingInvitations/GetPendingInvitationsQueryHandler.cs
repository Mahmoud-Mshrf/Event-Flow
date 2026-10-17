using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Staff.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.StaffInvitations.Enums;
using EventFlow.Domain.Tenants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Staff.Queries.GetPendingInvitations;

internal sealed class GetPendingInvitationsQueryHandler(
    IAppDbContext db,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetPendingInvitationsQuery, Result<PaginatedList<PendingInvitationDto>>>
{
    public async Task<Result<PaginatedList<PendingInvitationDto>>> Handle(
        GetPendingInvitationsQuery request,
        CancellationToken ct)
    {
        if (request.TenantId is not { } tenantId)
            return TenantErrors.NoTenantContext;

        var now = dateTimeProvider.UtcNow;

        // StaffInvitation has a global filter, but we scope explicitly too,
        // so the cache key, the tag, and the query always agree on the tenant.
        var query = db.StaffInvitations
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId
                     && i.Status == StaffInvitationStatus.Pending
                     && i.ExpiresAtUtc > now);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(i => i.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new PendingInvitationDto(
                i.Id,
                i.InviteeEmail,
                i.Role,
                i.ExpiresAtUtc,
                i.CreatedAtUtc))
            .ToListAsync(ct);

        return new PaginatedList<PendingInvitationDto>
        {
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = items
        };
    }
}