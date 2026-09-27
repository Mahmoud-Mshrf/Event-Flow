using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Orders;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(o => o.Total)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.OrderStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.TenantId)
            .IsRequired();

        builder.Property(o => o.EventId)
            .IsRequired();

        builder.Property(o => o.AttendeeId)
            .IsRequired();

        builder.Property(o => o.PaidAt)
            .IsRequired(false);

        // FK to Event
        builder.HasOne(o => o.Event)
            .WithMany()
            .HasForeignKey(o => o.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to User (Attendee)
        builder.HasOne(o => o.Attendee)
            .WithMany()
            .HasForeignKey(o => o.AttendeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to OrderItems — owned by Order
        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique: order number must be unique across all orders
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        // Attendee self-service queries — "show me my orders"
        builder.HasIndex(o => o.AttendeeId);

        // Organizer order management — "show me orders for this event"
        builder.HasIndex(o => new { o.TenantId, o.EventId });

        // Background job — find pending orders older than 15 minutes
        builder.HasIndex(o => new { o.OrderStatus, o.CreatedAtUtc });

        builder.Property(o => o.CreatedAtUtc).IsRequired();
        builder.Property(o => o.LastModifiedUtc).IsRequired();
    }
}
