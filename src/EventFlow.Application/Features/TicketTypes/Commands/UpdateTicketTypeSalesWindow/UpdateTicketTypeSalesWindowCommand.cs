using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeSalesWindow;

public sealed record UpdateTicketTypeSalesWindowCommand(
    Guid TicketTypeId,
    DateTime SalesStart,
    DateTime SalesEnd) : IRequest<Result<TicketTypeDto>>;
