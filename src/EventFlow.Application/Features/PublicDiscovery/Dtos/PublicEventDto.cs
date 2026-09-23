using EventFlow.Domain.Events.Enums;

namespace EventFlow.Application.Features.PublicDiscovery.Dtos;

public sealed record PublicEventDto(
    Guid Id,
    string Name,
    string? Description,
    string Location,
    string OrganizerName,
    DateTime StartDate,
    DateTime EndDate,
    DateTime RegistrationStart,
    DateTime RegistrationEnd,
    EventVisibility Visibility,
    bool IsRegistrationOpen);
