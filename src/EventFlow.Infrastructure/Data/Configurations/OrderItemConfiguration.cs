using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventFlow.Domain.Orders.OrderItems;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        builder.Property(oi => oi.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        // Subtotal is computed ([NotMapped]) — not stored
        builder.Ignore(oi => oi.Subtotal);

        builder.Property(oi => oi.TenantId)
            .IsRequired();

        builder.Property(oi => oi.OrderId)
            .IsRequired();

        builder.Property(oi => oi.TicketTypeId)
            .IsRequired();

        // FK to TicketType
        builder.HasOne(oi => oi.TicketType)
            .WithMany(tt => tt.OrderItems)
            .HasForeignKey(oi => oi.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for capacity release on order expiration/cancellation
        builder.HasIndex(oi => oi.TicketTypeId);

        builder.Property(oi => oi.CreatedAtUtc).IsRequired();
        builder.Property(oi => oi.LastModifiedUtc).IsRequired();
    }
}
