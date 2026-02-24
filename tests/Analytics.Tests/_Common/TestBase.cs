using Analytics.Tests.Factories;

namespace Analytics.Tests._Common;
public class TestBase(AnalyticsFixture fixture) : IClassFixture<AnalyticsFixture>
{
    protected CancellationToken CancellationToken { get; } = fixture.CancellationToken;
    protected static ModelFactory ModelFactory { get; } = new();
}
