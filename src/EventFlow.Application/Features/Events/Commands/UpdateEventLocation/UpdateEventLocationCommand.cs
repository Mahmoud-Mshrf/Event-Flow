using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventLocation;

public sealed record UpdateEventLocationCommand(
    Guid EventId,
    string Location) : IRequest<Result<EventDto>>;
