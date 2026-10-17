using EventFlow.Infrastructure.Messaging.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Infrastructure.Messaging;

// EventFlow.Infrastructure/Messaging/MassTransitConfiguration.cs

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMessaging(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Register all consumers
            x.AddConsumer<TicketIssuanceConsumer>();
            // x.AddConsumer<EmailNotificationConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                // Retry failed messages before sending to dead letter queue
                cfg.UseMessageRetry(r => r.Incremental(
                    retryLimit: 3,
                    initialInterval: TimeSpan.FromSeconds(1),
                    intervalIncrement: TimeSpan.FromSeconds(2)));

                // MassTransit auto-creates exchanges and queues
                // based on consumer and message type names
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}