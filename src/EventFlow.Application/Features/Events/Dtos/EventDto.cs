using EventFlow.Domain.Events.Enums;

namespace EventFlow.Application.Features.Events.Dtos;

public class EventDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = null!;
    public DateTime RegistrationStart { get; set; }
    public DateTime RegistrationEnd { get; set; }
    public EventVisibility Visibility { get; set; }
}
