using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Application.Features.Events.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using MediatR;

namespace EventFlow.Application.Features.Events.CreateEvent;

public sealed class CreateEventCommandHandler(IAppDbContext context,ICurrentTenant tenant) : IRequestHandler<CreateEventCommand, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var tenantEntity = await context.Tenants.FindAsync(tenant.TenantId, cancellationToken);
        if (tenantEntity is null)
        {
            return EventErrors.InvalidTenant;
        }
        
        var @event = Event.Create(new Guid(),Guid.TryParse(tenant.TenantId, out var tenantId) ? tenantId : Guid.Empty,
            request.EventName,
            request.Description,
            request.Location,
            request.StartDate,
            request.EndDate,
            request.RegistrationStart,
            request.RegistrationEnd,
            request.Visibility,DateTime.UtcNow);

        if (@event.IsError)
        {
            return @event.Errors;
        }
        await context.Events.AddAsync(@event.Value, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return @event.Value.ToDto();
    }
}