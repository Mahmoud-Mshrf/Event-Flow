namespace EventFlow.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}

