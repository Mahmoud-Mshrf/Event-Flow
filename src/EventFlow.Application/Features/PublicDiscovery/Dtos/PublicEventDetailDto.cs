namespace EventFlow.Application.Features.PublicDiscovery.Dtos;

public sealed record PublicEventDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Location,
    string OrganizerName,
    DateTime StartDate,
    DateTime EndDate,
    DateTime RegistrationStart,
    DateTime RegistrationEnd,
    bool IsRegistrationOpen,
    IReadOnlyCollection<PublicTicketTypeDto> TicketTypes);
