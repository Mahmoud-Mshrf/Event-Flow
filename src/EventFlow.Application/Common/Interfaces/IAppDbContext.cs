using EventFlow.Domain.Events;
using EventFlow.Domain.Identity;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Orders.OrderItems;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.TicketTypes;
using EventFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Event> Events { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<Ticket> Tickets {get;}
    DbSet<TicketType> TicketTypes {get;}
    DbSet<Order> Orders {get;}
    DbSet<OrderItem> OrderItems {get;}
    DbSet<Payment> Payments {get;}
    DbSet<VerificationToken> VerificationTokens {get;}
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
