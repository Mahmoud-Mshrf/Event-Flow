using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventFlow.Domain.Events;

namespace EventFlow.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.EventStatus)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Visibility)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.RegistrationStart)
            .IsRequired();

        builder.Property(x => x.RegistrationEnd)
            .IsRequired();

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}