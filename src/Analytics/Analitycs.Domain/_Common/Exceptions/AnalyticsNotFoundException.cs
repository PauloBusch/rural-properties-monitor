namespace Analitycs.Domain._Common.Exceptions;
public class AnalyticsNotFoundException(
    Guid id,
    string entity,
    string message
) : AnalyticsException(message)
{
    public Guid Id { get; } = id;
    public string Entity { get; } = entity;
}
