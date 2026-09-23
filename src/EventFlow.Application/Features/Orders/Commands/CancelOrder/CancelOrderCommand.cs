using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(
    Guid OrderId) : IRequest<Result<Success>>;
