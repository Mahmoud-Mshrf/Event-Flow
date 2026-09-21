using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventDetails;

public sealed record UpdateEventDetailsCommand(
    Guid EventId,
    string EventName,
    string? Description) : IRequest<Result<EventDto>>;
