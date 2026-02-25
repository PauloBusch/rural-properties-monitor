using System.Collections;

namespace Analitycs.Domain._Common.Exceptions;
public class AnalyticsExceptionCollection : AnalyticsException, IEnumerable<AnalyticsException>
{
    public IReadOnlyCollection<AnalyticsException> Exceptions { get; }

    public AnalyticsExceptionCollection(IReadOnlyCollection<AnalyticsException> exceptions)
        : base("One or more errors ocurred.")
    {
        Exceptions = exceptions;
    }

    public IEnumerator<AnalyticsException> GetEnumerator()
        => Exceptions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => Exceptions.GetEnumerator();
}
