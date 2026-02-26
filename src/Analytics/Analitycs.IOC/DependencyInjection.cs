using Analitycs.Data.Clients;
using Analitycs.Domain.Interfaces;
using Analitycs.Domain.Settings;
using Analytics.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;

namespace Analitycs.IOC;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KeycloakSettings>(
            configuration.GetSection("Keycloak"));

        services.AddHttpClient<IKeycloakTokenService, KeycloakTokenClient>();

        var ingressBaseUrl = configuration["Services:IngressApi"]
            ?? throw new InvalidOperationException("IngressApi URL not configured");

        services.AddHttpClient<IIngressApiClient, IngressApiClient>(client =>
            {
                client.BaseAddress = new Uri(ingressBaseUrl);
            })
            .AddResilienceHandler("ingress-pipeline", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });

                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(30)
                });
            });

        services.AddScoped<IAnalyticsService, AnalyticsService>();

        return services;
    }
   
}
