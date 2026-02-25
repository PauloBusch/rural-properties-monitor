using Analitycs.Domain._Common.Exceptions;
using System.Net;
using System.Text.Json.Serialization;

namespace Analytics.API._Common;
public class ErrorResponse
{
    public ErrorResponse(HttpStatusCode statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }

    public ErrorResponse(AnalyticsDuplicateException duplicateException)
    {
        StatusCode = HttpStatusCode.Conflict;
        Message = duplicateException.Message;
        Entity = duplicateException.Entity;
        Key = duplicateException.Id;
    }

    public ErrorResponse(AnalyticsNotFoundException notFoundException)
    {
        StatusCode = HttpStatusCode.NotFound;
        Message = notFoundException.Message;
        Key = notFoundException.Id;
        Entity = notFoundException.Entity;
    }

    public ErrorResponse(AnalyticsValidationException validationException)
    {
        StatusCode = HttpStatusCode.BadRequest;
        Message = validationException.Message;
        Field = validationException.Field;
    }

    public ErrorResponse(AnalyticsExceptionCollection exceptionCollection)
    {
        var exceptions = exceptionCollection.Exceptions;

        Errors = [
            ..exceptions
                .OfType<AnalyticsDuplicateException>()
                .Select(e => new ErrorResponse(e)),
            ..exceptions
                .OfType<AnalyticsNotFoundException>()
                .Select(e => new ErrorResponse(e)),
            ..exceptions
                .OfType<AnalyticsValidationException>()
                .Select(e => new ErrorResponse(e)),
            ..exceptions
                .OfType<AnalyticsExceptionCollection>()
                .Select(e => new ErrorResponse(e)),
        ];

        Message = exceptionCollection.Message;

        StatusCode = Errors.Max(e => e.StatusCode);
    }

    public ErrorResponse() { }

    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }

    public string Message { get; set; }

    public Guid? Key { get; set; }

    public string? Field { get; set; }

    public string? Entity { get; set; }

    public IReadOnlyCollection<ErrorResponse>? Errors { get; set; }
}
