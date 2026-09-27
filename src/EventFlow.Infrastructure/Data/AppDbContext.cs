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
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace EventFlow.Infrastructure.Data;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentTenant currentTenant,
    IPublisher publisher) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();
    public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents => Set<ProcessedWebhookEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Picks up all IEntityTypeConfiguration<T> classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── Global Query Filters ──────────────────────────────
        // Automatically appends WHERE TenantId = @currentTenantId
        // to every query against these entities
        // Use .IgnoreQueryFilters() to bypass for cross-tenant reads
        //
        // Nullable TenantId on User: attendees have null TenantId
        // The filter returns users where TenantId matches OR where
        // TenantId is null (attendees) — handled by making the filter
        // null-aware so attendees can query their own orders
        //
        // For strict tenant staff isolation we use non-null filter:
        modelBuilder.Entity<Event>()
            .HasQueryFilter(e => e.TenantId == currentTenant.TenantGuid);

        modelBuilder.Entity<TicketType>()
            .HasQueryFilter(tt => tt.TenantId == currentTenant.TenantGuid);

        modelBuilder.Entity<Order>()
            .HasQueryFilter(o => o.TenantId == currentTenant.TenantGuid);

        modelBuilder.Entity<OrderItem>()
            .HasQueryFilter(oi => oi.TenantId == currentTenant.TenantGuid);

        modelBuilder.Entity<Payment>()
            .HasQueryFilter(p => p.TenantId == currentTenant.TenantGuid);

        modelBuilder.Entity<Ticket>()
            .HasQueryFilter(t => t.TenantId == currentTenant.TenantGuid);

        // User: staff only (non-null TenantId matching current tenant)
        // Attendees (null TenantId) are accessed via IgnoreQueryFilters
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.TenantId == currentTenant.TenantGuid);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // Set audit fields on all tracked AuditableEntity instances
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(AuditableEntity.CreatedAtUtc)).CurrentValue = now;

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Property(nameof(AuditableEntity.LastModifiedUtc)).CurrentValue = now;
        }

        // Collect domain events BEFORE saving
        // Entities clear their own events after we snapshot them
        // so they don't fire twice on a second SaveChangesAsync call
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

        // Commit to database first
        // Domain events only fire if the transaction succeeded
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful commit
        // MediatR calls each INotificationHandler<TEvent>
        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        return result;
    }
}

// Stub implementations for design-time only — never registered in DI
internal sealed class NullCurrentTenant : ICurrentTenant
{
    public string? TenantId => null;
    public Guid? TenantGuid => null;
}

internal sealed class NullPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : INotification
        => Task.CompletedTask;
}