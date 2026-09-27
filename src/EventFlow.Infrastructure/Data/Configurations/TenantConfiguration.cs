using EventFlow.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        // Unique: no two tenants can have the same name
        // Enforced here and also checked in RegisterOrganizerCommandHandler
        builder.HasIndex(t => t.Name)
            .IsUnique();

        // Audit fields
        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.LastModifiedUtc).IsRequired();
    }
}
