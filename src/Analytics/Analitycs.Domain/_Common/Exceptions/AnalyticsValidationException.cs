
namespace Analitycs.Domain._Common.Exceptions;
public class AnalyticsValidationException(
    string field,
    string message
) : AnalyticsException(message)
{
    public string Field { get; } = field;
}
