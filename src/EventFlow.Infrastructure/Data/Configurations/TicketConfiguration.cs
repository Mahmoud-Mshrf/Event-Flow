using EventFlow.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TicketNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(t => t.QrCode)
            .IsRequired()
            .HasMaxLength(500);     // HMAC-signed payload — needs room

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.TenantId)
            .IsRequired();

        builder.Property(t => t.EventId)
            .IsRequired();

        builder.Property(t => t.AttendeeId)
            .IsRequired();

        builder.Property(t => t.TicketTypeId)
            .IsRequired();

        builder.Property(t => t.OrderId)
            .IsRequired();

        builder.Property(t => t.CheckedInAt)
            .IsRequired(false);

        // IsValid is computed ([NotMapped]) — not stored
        builder.Ignore(t => t.IsValid);

        // FK to Event
        builder.HasOne(t => t.Event)
            .WithMany()
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to User (Attendee)
        builder.HasOne(t => t.Attendee)
            .WithMany()
            .HasForeignKey(t => t.AttendeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to TicketType
        builder.HasOne(t => t.TicketType)
            .WithMany(tt => tt.Tickets)
            .HasForeignKey(t => t.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to Order
        builder.HasOne(t => t.Order)
            .WithMany()
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique: each ticket number is unique across the system
        builder.HasIndex(t => t.TicketNumber)
            .IsUnique();

        // QR check-in lookup — most frequent read at the venue
        // This is the hottest index in the system on event day
        builder.HasIndex(t => t.QrCode)
            .IsUnique();

        // Attendee's own tickets
        builder.HasIndex(t => t.AttendeeId);

        // Organizer's view — all tickets for an event
        builder.HasIndex(t => new { t.TenantId, t.EventId });

        // SignalR dashboard — count checked-in per event
        builder.HasIndex(t => new { t.EventId, t.Status });

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.LastModifiedUtc).IsRequired();
    }
}
