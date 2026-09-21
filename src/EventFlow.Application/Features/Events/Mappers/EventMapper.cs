using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Events;

namespace EventFlow.Application.Features.Events.Mappers;

public static class EventMapper
{
    public static EventDto ToDto(this Event entity)
    {
        return new EventDto
        {
            Id = entity.Id,
            Name = entity.EventName,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Location = entity.Location,
            RegistrationStart = entity.RegistrationStart,
            RegistrationEnd = entity.RegistrationEnd,
            Visibility = entity.Visibility
        };
    }
}