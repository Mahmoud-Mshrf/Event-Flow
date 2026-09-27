using EventFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion<string>()    // store as "Owner", "Employee" not 0, 1
            .HasMaxLength(20);

        builder.Property(u => u.Disabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.EmailConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        // Unique: one account per email address across the whole system
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // TenantId nullable — staff have one, attendees don't
        builder.Property(u => u.TenantId)
            .IsRequired(false);

        // FK to Tenant (nullable — attendees have no tenant)
        builder.HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Index for fast staff lookup by tenant
        builder.HasIndex(u => u.TenantId)
            .HasFilter("[TenantId] IS NOT NULL");   // partial index — only index non-null rows

        builder.Property(u => u.CreatedAtUtc).IsRequired();
        builder.Property(u => u.LastModifiedUtc).IsRequired();
    }
}
