using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Staff.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Tenants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Staff.Queries.GetStaff;

internal sealed class GetStaffQueryHandler(IAppDbContext db)
    : IRequestHandler<GetStaffQuery, Result<PaginatedList<StaffMemberDto>>>
{
    public async Task<Result<PaginatedList<StaffMemberDto>>> Handle(
        GetStaffQuery request,
        CancellationToken ct)
    {
        if (request.TenantId is not { } tenantId)
            return TenantErrors.NoTenantContext;

        // User has NO global query filter, so tenant scoping is explicit.
        // Without this Where, attendees and other tenants' staff would leak.
        var query = db.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new StaffMemberDto(
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.Disabled))
            .ToListAsync(ct);

        return new PaginatedList<StaffMemberDto>
        {
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = items
        };
    }
}