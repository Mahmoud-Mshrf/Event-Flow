using EventFlow.Application.Features.PublicDiscovery.Dtos;
using EventFlow.Domain.Events;

namespace EventFlow.Application.Features.PublicDiscovery.Mappers;

public static class PublicEventMapper
{
    public static PublicEventDto ToDto(this Event entity) => new(
        entity.Id,
        entity.EventName,
        entity.Description,
        entity.Location,
        entity.Tenant.Name,
        entity.StartDate,
        entity.EndDate,
        entity.RegistrationStart,
        entity.RegistrationEnd,
        entity.Visibility,
        entity.EventStatus == Domain.Events.Enums.EventStatus.RegistrationOpen);

    public static PublicEventDetailDto ToDetailDto(this Event entity, DateTime now) => new(
        entity.Id,
        entity.EventName,
        entity.Description,
        entity.Location,
        entity.Tenant.Name,
        entity.StartDate,
        entity.EndDate,
        entity.RegistrationStart,
        entity.RegistrationEnd,
        entity.EventStatus == Domain.Events.Enums.EventStatus.RegistrationOpen,
        entity.TicketTypes
            .Where(tt => tt.SalesStart <= now && tt.SalesEnd >= now)
            .OrderBy(tt => tt.Price)
            .Select(tt => new PublicTicketTypeDto(
                tt.Id,
                tt.Name,
                tt.Price,
                tt.Capacity - tt.ReservedQuantity,
                tt.SalesStart <= now && tt.SalesEnd >= now))
            .ToList());
}