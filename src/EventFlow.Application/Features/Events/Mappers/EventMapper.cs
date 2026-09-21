using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Events;

namespace EventFlow.Application.Features.Events.Mappers;

public static class EventMapper
{
    public static EventDto ToDto(this Event entity) => new(
        entity.Id,
        entity.EventName,
        entity.Description,
        entity.Location,
        entity.StartDate,
        entity.EndDate,
        entity.RegistrationStart,
        entity.RegistrationEnd,
        entity.Visibility,
        entity.EventStatus);
}