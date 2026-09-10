using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventFlow.Domain.TicketTypes;

namespace EventFlow.Infrastructure.Data.Configurations;

public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.ToTable("TicketTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Capacity)
            .IsRequired();

        builder.Property(x => x.ReservedQuantity)
            .IsRequired();

        builder.Property(x => x.SalesStart)
            .IsRequired();

        builder.Property(x => x.SalesEnd)
            .IsRequired();

        // Optimistic concurrency
        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Event)
            .WithMany(x => x.TicketTypes)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
