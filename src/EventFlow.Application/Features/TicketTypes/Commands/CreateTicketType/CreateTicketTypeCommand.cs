using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.TicketTypes.commands.CreateTicketType;

public sealed record CreateTicketTypeCommand(
    Guid EventId,
    string Name,
    decimal Price,
    int Capacity,
    DateTime SalesStart,
    DateTime SalesEnd) : IRequest<Result<TicketTypeDto>>;
