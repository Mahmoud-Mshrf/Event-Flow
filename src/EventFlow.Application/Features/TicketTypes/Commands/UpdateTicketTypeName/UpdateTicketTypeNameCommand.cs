using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeName;

public sealed record UpdateTicketTypeNameCommand(
    Guid TicketTypeId,
    string Name) : IRequest<Result<TicketTypeDto>>;
