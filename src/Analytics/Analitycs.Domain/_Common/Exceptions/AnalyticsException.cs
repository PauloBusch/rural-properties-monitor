namespace Analitycs.Domain._Common.Exceptions;
public abstract class AnalyticsException(string? message = default)
    : Exception(message) { }
