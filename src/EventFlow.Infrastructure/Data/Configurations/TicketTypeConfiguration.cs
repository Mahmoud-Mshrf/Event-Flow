using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventFlow.Domain.TicketTypes;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.HasKey(tt => tt.Id);

        builder.Property(tt => tt.Name)
            .IsRequired()
            .HasMaxLength(30);

        // Explicit precision for money — never use float/double for currency
        builder.Property(tt => tt.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(tt => tt.Capacity)
            .IsRequired();

        builder.Property(tt => tt.ReservedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(tt => tt.TenantId)
            .IsRequired();

        builder.Property(tt => tt.EventId)
            .IsRequired();

        // ── RowVersion — this is what makes concurrency-safe capacity work ──
        // EF Core uses this to detect concurrent modifications
        // If two threads modify the same TicketType simultaneously,
        // the second SaveChangesAsync throws DbUpdateConcurrencyException
        builder.Property(tt => tt.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Index for fast ticket type lookup by event
        builder.HasIndex(tt => tt.EventId);

        // Composite index for tenant-scoped event lookups
        builder.HasIndex(tt => new { tt.TenantId, tt.EventId });

        builder.Property(tt => tt.CreatedAtUtc).IsRequired();
        builder.Property(tt => tt.LastModifiedUtc).IsRequired();
    }
}
