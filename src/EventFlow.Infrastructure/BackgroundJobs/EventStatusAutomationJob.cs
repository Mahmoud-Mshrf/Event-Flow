using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Events.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.BackgroundJobs;

public sealed class EventStatusAutomationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<EventStatusAutomationJob> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EventStatusAutomationJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TransitionEventStatusesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "EventStatusAutomationJob failed. Will retry next cycle.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task TransitionEventStatusesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var now = DateTime.UtcNow;
        var transitioned = 0;

        // Published → RegistrationOpen (when RegistrationStart has arrived)
        var toOpen = await dbContext.Events
            .IgnoreQueryFilters()
            .Where(e => e.EventStatus == EventStatus.Published
                     && e.RegistrationStart <= now)
            .ToListAsync(ct);

        foreach (var ev in toOpen)
            ev.OpenRegistration(now);

        // RegistrationOpen → RegistrationClosed (when RegistrationEnd has passed)
        var toClosed = await dbContext.Events
            .IgnoreQueryFilters()
            .Where(e => e.EventStatus == EventStatus.RegistrationOpen
                     && e.RegistrationEnd <= now)
            .ToListAsync(ct);

        foreach (var ev in toClosed)
            ev.CloseRegistration();

        // RegistrationClosed → Completed (when EndDate has passed)
        var toCompleted = await dbContext.Events
            .IgnoreQueryFilters()
            .Where(e => e.EventStatus == EventStatus.RegistrationClosed
                     && e.EndDate <= now)
            .ToListAsync(ct);

        foreach (var ev in toCompleted)
            ev.Complete(now);

        transitioned = toOpen.Count + toClosed.Count + toCompleted.Count;

        if (transitioned == 0)
            return;

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "Event transitions — Opened: {Open}, Closed: {Closed}, Completed: {Completed}.",
            toOpen.Count, toClosed.Count, toCompleted.Count);
    }
}