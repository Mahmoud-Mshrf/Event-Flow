namespace EventFlow.Application.Common.Contracts;

public record PageRequest
{
    private int _pageSize = 10;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 50 ? 50 : value; // cap at 50
    }
}