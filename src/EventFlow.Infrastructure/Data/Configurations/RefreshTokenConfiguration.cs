using EventFlow.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Infrastructure.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.RevokedAtUtc)
            .IsRequired(false);

        builder.Property(rt => rt.ReplacedByTokenId)
            .IsRequired(false);

        // FK to User
        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Fast lookup — find active tokens by user
        // RefreshTokenCommandHandler loads active tokens by UserId
        builder.HasIndex(rt => rt.UserId);

        // Composite — find active (non-revoked) tokens for a user quickly
        builder.HasIndex(rt => new { rt.UserId, rt.RevokedAtUtc });

        builder.Property(rt => rt.CreatedAtUtc).IsRequired();
        builder.Property(rt => rt.LastModifiedUtc).IsRequired();
    }
}