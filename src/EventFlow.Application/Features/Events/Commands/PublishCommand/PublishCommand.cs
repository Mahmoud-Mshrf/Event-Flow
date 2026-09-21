using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Events.Commands.PublishCommand;

public sealed record PublishEventCommand(
    Guid EventId) : IRequest<Result<EventDto>>;
