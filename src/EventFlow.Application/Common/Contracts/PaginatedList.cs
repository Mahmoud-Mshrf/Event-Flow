namespace EventFlow.Application.Common.Contracts;

public sealed record PaginatedList<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyCollection<T> Items { get; init; } = [];
}
