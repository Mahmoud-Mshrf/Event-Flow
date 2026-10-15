using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Orders.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.BackgroundJobs;

public sealed class OrderExpirationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<OrderExpirationJob> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan ExpirationThreshold = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OrderExpirationJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireOrdersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "OrderExpirationJob failed. Will retry next cycle.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ExpireOrdersAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var cutoff = DateTimeOffset.UtcNow - ExpirationThreshold;

        // IgnoreQueryFilters — no tenant context in background jobs
        var pendingOrders = await dbContext.Orders
            .IgnoreQueryFilters()
            .Where(o => o.OrderStatus == OrderStatus.Pending
                     && o.CreatedAtUtc <= cutoff)
            .ToListAsync(ct);

        if (pendingOrders.Count == 0)
            return;

        foreach (var order in pendingOrders)
            order.MarkAsExpired(); // raises OrderExpiredDomainEvent

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Expired {Count} orders.", pendingOrders.Count);
    }
}
