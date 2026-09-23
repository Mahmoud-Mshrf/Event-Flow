namespace EventFlow.Application.Features.PublicDiscovery.Dtos;

public sealed record PublicTicketTypeDto(
    Guid Id,
    string Name,
    decimal Price,
    int AvailableQuantity,
    bool IsSalesOpen);