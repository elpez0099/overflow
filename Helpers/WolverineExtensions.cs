using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Helpers;

public static class WolverineExtensions
{
    public static async Task UseWolverineWithRabbitMqAsync(this IHostApplicationBuilder builder,
         Action<WolverineOptions> configureMessaging)
    {
        var retryPolicy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                retryCount:5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount) =>
                {
                    Console.WriteLine($"Retry attempt {retryCount} failed. Retrying in {timeSpan.Seconds} seconds...");
                }
            );

        await retryPolicy.ExecuteAsync(async () =>
        {
            var endpoint = builder.Configuration.GetConnectionString("messaging") ?? throw new InvalidOperationException("Missing configuration connection string");
            var factory = new ConnectionFactory
            {
                Uri = new Uri(endpoint)
            };
            await using var connection  = await  factory.CreateConnectionAsync();
        });
        
        // Adds Open Telemetry for RabbitMQ
        builder.Services.AddOpenTelemetry().WithTracing(traceProviderBuilder =>
        {
            traceProviderBuilder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName))
                .AddSource("Wolverine");
        });
        
        builder.UseWolverine(opts =>
        {
            opts.UseRuntimeCompilation();
            opts.UseRabbitMqUsingNamedConnection("messaging")
                .AutoProvision()
                .DeclareExchange("questions");
            configureMessaging(opts);
        });
    }
}