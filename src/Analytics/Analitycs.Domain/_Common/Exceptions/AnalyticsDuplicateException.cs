namespace Analitycs.Domain._Common.Exceptions;
public class AnalyticsDuplicateException(
    Guid? id,
    string entity,
    string message
) : AnalyticsException(message)
{
    public AnalyticsDuplicateException(
        string entity,
        string message
    ) : this(null, entity, message) { }

    public Guid? Id { get; } = id;
    public string Entity { get; } = entity;
}
