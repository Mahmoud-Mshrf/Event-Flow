using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Orders;

namespace EventFlow.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.OrderNumber)
            .IsUnique();

        builder.Property(x => x.OrderStatus)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Event)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Attendee)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.AttendeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // builder.HasMany("OrderItems")
        //     .WithOne("Order")
        //     .HasForeignKey("OrderId")
        //     .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x =>x.OrderItems)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Payment)
            .WithOne(x => x.Order)
            .HasForeignKey<Payment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
