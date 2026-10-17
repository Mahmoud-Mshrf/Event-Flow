using EventFlow.Domain.StaffInvitations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class StaffInvitationConfiguration
    : IEntityTypeConfiguration<StaffInvitation>
{
    public void Configure(EntityTypeBuilder<StaffInvitation> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TenantId)
            .IsRequired();

        builder.Property(i => i.InvitedByUserId)
            .IsRequired();

        builder.Property(i => i.InviteeEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.TokenHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(i => i.ExpiresAtUtc)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.AcceptedAtUtc)
            .IsRequired(false);

        builder.Property(i => i.CreatedAtUtc)
            .IsRequired();

        builder.Property(i => i.LastModifiedUtc)
            .IsRequired();

        // Fast lookup by email when accepting an invitation
        builder.HasIndex(i => i.InviteeEmail);

        // Prevent duplicate pending invitations for same email in same tenant
        // Partial unique index — only one Pending per email+tenant at a time
        builder.HasIndex(i => new { i.TenantId, i.InviteeEmail, i.Status })
            .IsUnique()
            .HasFilter("[Status] = 'Pending'");
    }
}