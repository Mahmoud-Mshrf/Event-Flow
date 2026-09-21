using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events.Enums;
using EventFlow.Domain.Tenants;
using MediatR;

namespace EventFlow.Application.Features.Events.Commands.CreateEvent;

public sealed record CreateEventCommand(
    string EventName,
    string? Description,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    DateTime RegistrationStart,
    DateTime RegistrationEnd,
    EventVisibility Visibility) : IRequest<Result<EventDto>>;
