using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeCapacity;

public sealed record UpdateTicketTypeCapacityCommand(
    Guid TicketTypeId,
    int Capacity) : IRequest<Result<TicketTypeDto>>;
