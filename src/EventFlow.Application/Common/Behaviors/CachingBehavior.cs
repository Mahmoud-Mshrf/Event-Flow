using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace EventFlow.Application.Common.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    // Injected on purpose: forces the scoped services to be constructed here,
    // inside the request flow, before HybridCache moves execution elsewhere.
    ICurrentUser currentUser,
    ICurrentTenant currentTenant,
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

        // Never cache for unauthenticated callers.
        if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty)
            return await next(ct);

        var requestName = typeof(TRequest).Name;

        logger.LogInformation(
            "Checking cache for {RequestName} with key {CacheKey}",
            requestName,
            cachedRequest.CacheKey);

        var options = new HybridCacheEntryOptions
        {
            Expiration = cachedRequest.Expiration,
            LocalCacheExpiration = cachedRequest.Expiration < TimeSpan.FromSeconds(30)
                ? cachedRequest.Expiration
                : TimeSpan.FromSeconds(30)
        };
        _ = currentTenant;

        try
        {
            return await cache.GetOrCreateAsync<TResponse>(
                cachedRequest.CacheKey,
                async factoryCt =>
                {
                    logger.LogInformation(
                        "Cache miss for {RequestName} — executing handler",
                        requestName);

                    var response = await next(factoryCt);

                    // HybridCache never stores exceptions, so this keeps failures out.
                    if (response is IResult { IsSuccess: false })
                        throw new UncacheableResultException(response);

                    return response;
                },
                options,
                cachedRequest.Tags,
                ct);
        }
        catch (UncacheableResultException ex)
        {
            logger.LogInformation(
                "Result for {RequestName} was a failure — not cached",
                requestName);

            return ex.Response;
        }
    }

    private sealed class UncacheableResultException(TResponse response) : Exception
    {
        public TResponse Response { get; } = response;
    }
}