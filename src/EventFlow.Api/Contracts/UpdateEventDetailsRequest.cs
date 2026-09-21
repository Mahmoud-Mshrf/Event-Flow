using EventFlow.Domain.Events.Enums;

namespace EventFlow.Api.Contracts;

public sealed record UpdateEventDetailsRequest(string EventName, string? Description);
public sealed record UpdateEventLocationRequest(string Location);
public sealed record UpdateEventScheduleRequest(DateTime StartDate, DateTime EndDate);
public sealed record UpdateEventRegistrationPeriodRequest(DateTime RegistrationStart, DateTime RegistrationEnd);
public sealed record UpdateEventVisibilityRequest(EventVisibility Visibility);