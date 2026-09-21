using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Events.CreateEvent;

public sealed record CreateEventCommand(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate) : IRequest<Result<EventDto>>;