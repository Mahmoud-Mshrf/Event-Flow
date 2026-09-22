using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Domain.TicketTypes;

namespace EventFlow.Application.Features.TicketTypes.Mappers;

public static class TicketTypeMapper
{
    public static TicketTypeDto ToDto(this TicketType entity, DateTime now) => new(
        entity.Id,
        entity.EventId,
        entity.Name,
        entity.Price,
        entity.Capacity,
        entity.ReservedQuantity,
        entity.AvailableQuantity,
        entity.SalesStart,
        entity.SalesEnd,
        entity.IsSalesOpen(now));
}

