using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventFlow.Domain.Events;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.EventStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.Visibility)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.TenantId)
            .IsRequired();

        // FK to Tenant
        builder.HasOne(e => e.Tenant)
            .WithMany(t => t.Events)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to TicketTypes — owned by Event
        builder.HasMany(e => e.TicketTypes)
            .WithOne(tt => tt.Event)
            .HasForeignKey(tt => tt.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for public discovery queries
        // Filters by Visibility + Status + StartDate — all three in every public query
        builder.HasIndex(e => new { e.Visibility, e.EventStatus, e.StartDate });

        // Organizer dashboard — filter by TenantId, order by StartDate
        builder.HasIndex(e => new { e.TenantId, e.StartDate });

        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.LastModifiedUtc).IsRequired();
    }
}