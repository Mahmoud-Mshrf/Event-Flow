using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common;
using EventFlow.Domain.Events;
using EventFlow.Domain.Identity;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Orders.OrderItems;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.TicketTypes;
using EventFlow.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventFlow.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options,ICurrentTenant tenant,IPublisher _publisher) : DbContext(options), IAppDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Event>().HasQueryFilter(x=>x.TenantId == tenant.TenantGuid);
        modelBuilder.Entity<Order>().HasQueryFilter(x=>x.TenantId == tenant.TenantGuid);
        modelBuilder.Entity<User>().HasQueryFilter(x=>x.TenantId == tenant.TenantGuid);
        modelBuilder.Entity<Ticket>().HasQueryFilter(x=>x.TenantId == tenant.TenantGuid);
        modelBuilder.Entity<TicketType>().HasQueryFilter(x=>x.TenantId == tenant.TenantGuid);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(x=>x.TenantId == tenant.TenantGuid);
        modelBuilder.Entity<Payment>().HasQueryFilter(x=>x.TenantId == tenant.TenantGuid);
        
    }
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payment => Set<Payment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents => Set<ProcessedWebhookEvent>();
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    // Collect all domain events before saving
    // (entities are cleared after dispatch so events don't fire twice)
    var domainEvents = ChangeTracker
        .Entries<Entity>()
        .Select(e => e.Entity)
        .Where(e => e.DomainEvents.Count != 0)
        .SelectMany(e =>
        {
            var events = e.DomainEvents.ToList();
            e.ClearDomainEvents();
            return events;
        })
        .ToList();

    // Commit first — events fire only if the transaction succeeded
    var result = await base.SaveChangesAsync(cancellationToken);

    // Publish after commit — handlers run after HTTP response is already on its way
    foreach (var domainEvent in domainEvents)
        await _publisher.Publish(domainEvent, cancellationToken);

    return result;
}
}