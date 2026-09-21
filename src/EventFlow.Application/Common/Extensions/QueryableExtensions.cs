using Microsoft.EntityFrameworkCore;
using EventFlow.Application.Common.Contracts;

namespace EventFlow.Application.Common.Extensions;

// EventFlow.Application/Common/Extensions/QueryableExtensions.cs
public static class QueryableExtensions
{
    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedList<T>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }
}