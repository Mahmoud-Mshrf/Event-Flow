using EventFlow.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class VerificationTokenConfiguration
    : IEntityTypeConfiguration<VerificationToken>
{
    public void Configure(EntityTypeBuilder<VerificationToken> builder)
    {
        builder.HasKey(vt => vt.Id);

        builder.Property(vt => vt.CodeHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(vt => vt.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(vt => vt.ExpiresAtUtc)
            .IsRequired();

        builder.Property(vt => vt.Used)
            .IsRequired()
            .HasDefaultValue(false);

        // FK to User
        builder.HasOne(vt => vt.User)
            .WithMany()
            .HasForeignKey(vt => vt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Fast lookup — find active token by user + type
        // ConfirmEmailCommandHandler, ResetPasswordCommandHandler use this
        builder.HasIndex(vt => new { vt.UserId, vt.Type });

        // Rate-limit check — find recent tokens by user + type + CreatedAt
        builder.HasIndex(vt => new { vt.UserId, vt.Type, vt.CreatedAtUtc });

        builder.Property(vt => vt.CreatedAtUtc).IsRequired();
        builder.Property(vt => vt.LastModifiedUtc).IsRequired();
    }
}