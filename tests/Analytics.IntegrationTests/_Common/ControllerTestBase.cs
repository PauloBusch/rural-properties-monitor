using Analytics.IntegrationTests.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace Analytics.IntegrationTests._Common;
public class ControllerTestBase(AnalyticsFixture fixture, string url) : IClassFixture<AnalyticsFixture>
{
    protected Uri Uri { get; } = new($"{fixture.Client.BaseAddress}{url}");
    protected Requester Requester { get; } = new(fixture.Client);
    protected CancellationToken CancellationToken { get; } = fixture.CancellationToken;
    protected static EntityFactory EntityFactory { get; } = new();
    //protected static ModelFactory ModelFactory { get; } = new();
    protected TService GetService<TService>() => fixture.Factory.Services
        .GetService<TService>() ?? throw new InvalidOperationException($"Service of type {typeof(TService).FullName} is not registered.");
}
