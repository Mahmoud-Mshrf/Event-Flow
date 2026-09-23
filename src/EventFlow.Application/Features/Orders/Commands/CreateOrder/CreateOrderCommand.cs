using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderItemRequest(
    Guid TicketTypeId,
    int Quantity);

public sealed record CreateOrderCommand(
    Guid EventId,
    IReadOnlyCollection<CreateOrderItemRequest> Items) : IRequest<Result<OrderDto>>;
