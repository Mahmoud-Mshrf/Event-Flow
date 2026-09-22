using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypePrice;

public sealed record UpdateTicketTypePriceCommand(
    Guid TicketTypeId,
    decimal Price) : IRequest<Result<TicketTypeDto>>;
