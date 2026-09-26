using EventFlow.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class ProcessedWebhookEventConfiguration
    : IEntityTypeConfiguration<ProcessedWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedWebhookEvent> builder)
    {
        builder.HasKey(e => e.Id);

        // THIS IS THE KEY LINE — without this unique index, idempotency doesn't work
        // When two threads try to insert the same ProviderEventId at the same time,
        // SQL Server rejects the second one at the database level — no race condition possible
        builder.HasIndex(e => e.ProviderEventId)
            .IsUnique();

        builder.Property(e => e.ProviderEventId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);
    }
}