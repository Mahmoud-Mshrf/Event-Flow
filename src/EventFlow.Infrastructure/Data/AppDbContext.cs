using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Events;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Orders.OrderItems;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.TicketTypes;
using EventFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventFlow.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options,ICurrentTenant tenant) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppContext).Assembly);

        modelBuilder.Entity<Event>().HasQueryFilter(x=>x.TenantId.ToString()==tenant.TenantId);
        modelBuilder.Entity<Order>().HasQueryFilter(x=>x.TenantId.ToString()==tenant.TenantId);
        modelBuilder.Entity<User>().HasQueryFilter(x=>x.TenantId.ToString()==tenant.TenantId);
        modelBuilder.Entity<Ticket>().HasQueryFilter(x=>x.TenantId.ToString()==tenant.TenantId);
        modelBuilder.Entity<TicketType>().HasQueryFilter(x=>x.TenantId.ToString()==tenant.TenantId);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(x=>x.TenantId.ToString()==tenant.TenantId);
    }
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payment => Set<Payment>();
}