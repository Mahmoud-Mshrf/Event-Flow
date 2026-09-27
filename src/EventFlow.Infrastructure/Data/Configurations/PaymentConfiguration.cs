using EventFlow.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.ProviderReferenceId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(p => p.SucceededAt)
            .IsRequired(false);

        builder.Property(p => p.FailedAt)
            .IsRequired(false);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.OrderId)
            .IsRequired();

        // FK to Order
        builder.HasOne(p => p.Order)
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Webhook handler lookup — find payment by ProviderReferenceId
        // This is the index that makes webhook processing fast
        builder.HasIndex(p => p.ProviderReferenceId);

        // Prevent duplicate payment initiation — check for pending payments by order
        builder.HasIndex(p => new { p.OrderId, p.PaymentStatus });

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.LastModifiedUtc).IsRequired();
    }
}