using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.TicketTypes.Commands.DeleteTicketType;

public sealed record DeleteTicketTypeCommand(
    Guid TicketTypeId) : IRequest<Result<Deleted>>;