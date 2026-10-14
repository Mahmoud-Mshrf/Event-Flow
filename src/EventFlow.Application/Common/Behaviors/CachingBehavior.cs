using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace EventFlow.Application.Common.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not ICachedQuery cachedRequest)
            return await next(ct);

        logger.LogInformation(
            "Checking cache for {RequestName} with key {CacheKey}",
            typeof(TRequest).Name,
            cachedRequest.CacheKey);

        var response = await cache.GetOrCreateAsync<TResponse>(
            cachedRequest.CacheKey,
            async _ =>
            {
                logger.LogInformation(
                    "Cache miss for {RequestName} — executing handler",
                    typeof(TRequest).Name);

                return await next(ct);
            },
            new HybridCacheEntryOptions
            {
                Expiration = cachedRequest.Expiration,
                LocalCacheExpiration = TimeSpan.FromSeconds(30)
            },
            cachedRequest.Tags,
            ct);

        if (response is IResult { IsSuccess: false })
        {
            foreach (var tag in cachedRequest.Tags)
                await cache.RemoveByTagAsync(tag, ct);
        }

        return response;
    }
}