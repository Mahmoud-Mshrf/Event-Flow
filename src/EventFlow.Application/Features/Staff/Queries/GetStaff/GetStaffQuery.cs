using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Staff.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.Staff.Queries.GetStaff;

public sealed record GetStaffQuery : PageRequest,
    ICachedQuery<Result<PaginatedList<StaffMemberDto>>>
{
    public Guid? TenantId { get; init; }

    public string CacheKey =>
        $"tenant-staff:{TenantId}:{Page}:{PageSize}";

    public string[] Tags =>
    [
        $"tenant-{TenantId}-staff"
    ];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
