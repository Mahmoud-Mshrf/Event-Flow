using EventFlow.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.QrCode)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.CheckedInAt);

        builder.HasOne(x => x.Event)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // builder.HasOne(x => x.Attendee)
        //     .WithMany(x => x.Tickets)
        //     .HasForeignKey(x => x.AttendeeId)
        //     .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TicketType)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // builder.HasOne(x => x.Order)
        //     .WithMany(x => x.Tickets)
        //     .HasForeignKey(x => x.OrderId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}


