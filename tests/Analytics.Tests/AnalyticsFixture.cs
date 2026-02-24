namespace Analytics.Tests;
public class AnalyticsFixture : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public AnalyticsFixture()
    {
        CancellationToken = _cancellationTokenSource.Token;
    }

    public CancellationToken CancellationToken { get; private set; }

    public void Dispose() => _cancellationTokenSource.Cancel();
}
