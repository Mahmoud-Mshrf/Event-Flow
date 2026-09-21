using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Application.Features.Events.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.CreateEvent;

public sealed class CreateEventCommandHandler(
    IAppDbContext context,
    ICurrentTenant currentTenant,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateEventCommand, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(
        CreateEventCommand request,
        CancellationToken cancellationToken)
    {
        // TenantId is Guid? — null means the caller is not a tenant user
        if (currentTenant.TenantId is not { } tenantId)
            return EventErrors.InvalidTenant;

        var result = Event.Create(
            Guid.NewGuid(),
            tenantId,
            request.EventName,
            request.Description,
            request.Location,
            request.StartDate,
            request.EndDate,
            request.RegistrationStart,
            request.RegistrationEnd,
            request.Visibility,
            dateTimeProvider.UtcNow);

        if (result.IsError)
            return result.Errors!;

        await context.Events.AddAsync(result.Value, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return result.Value.ToDto();
    }
}