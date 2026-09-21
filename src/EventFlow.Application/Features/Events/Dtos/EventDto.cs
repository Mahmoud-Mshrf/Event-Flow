using EventFlow.Domain.Events.Enums;

namespace EventFlow.Application.Features.Events.Dtos;

public sealed record EventDto(
    Guid Id,
    string Name,
    string? Description,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    DateTime RegistrationStart,
    DateTime RegistrationEnd,
    EventVisibility Visibility,
    EventStatus Status);