using EventFlow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.BackgroundJobs;

public sealed class TokenCleanupJob(
    IServiceScopeFactory scopeFactory,
    ILogger<TokenCleanupJob> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);
    private static readonly TimeSpan RetentionBuffer = TimeSpan.FromDays(7);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TokenCleanupJob started.");

        // Run once immediately on startup, then every 24 hours
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "TokenCleanupJob failed. Will retry next cycle.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var cutoff = DateTimeOffset.UtcNow - RetentionBuffer;

        // ── 1. VerificationTokens ─────────────────────────────────────
        // Delete tokens that are either:
        //   - Already consumed (Used = true) and older than retention buffer
        //   - Expired without being used and older than retention buffer
        var deletedVerification = await db.VerificationTokens
            .IgnoreQueryFilters()
            .Where(t => t.ExpiresAtUtc < cutoff || t.Used)
            .ExecuteDeleteAsync(ct);

        // ── 2. RefreshTokens ──────────────────────────────────────────
        // Delete tokens that are either:
        //   - Naturally expired and older than retention buffer
        //   - Revoked (logged out / rotated) and older than retention buffer
        var deletedRefresh = await db.RefreshTokens
            .IgnoreQueryFilters()
            .Where(t => t.ExpiresAtUtc < cutoff
                     || (t.RevokedAtUtc != null && t.RevokedAtUtc < cutoff))
            .ExecuteDeleteAsync(ct);

        logger.LogInformation(
            "TokenCleanupJob completed — VerificationTokens deleted: {V}, RefreshTokens deleted: {R}.",
            deletedVerification,
            deletedRefresh);
    }
}