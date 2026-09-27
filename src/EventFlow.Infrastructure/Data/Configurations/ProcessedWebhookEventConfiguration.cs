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

        builder.Property(e => e.ProviderEventId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        // ── THIS IS THE MOST CRITICAL INDEX IN THE WHOLE PROJECT ──
        // Without this unique index, idempotency doesn't exist
        // Two concurrent webhooks could both pass the "already processed?" check
        // before either writes, resulting in double-processing
        // The unique constraint makes the database enforce atomicity
        builder.HasIndex(e => e.ProviderEventId)
            .IsUnique();

        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.LastModifiedUtc).IsRequired();
    }
}