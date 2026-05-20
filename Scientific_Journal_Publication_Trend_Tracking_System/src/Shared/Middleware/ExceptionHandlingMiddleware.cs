using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;
using AppValidEx = Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions.ValidationException;
using AppAppEx = Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions.ApplicationException;
using AppUnauthorizedEx = Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions.UnauthorizedException;
using AppConflictEx = Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions.ConflictException;
using AppNotFoundEx = Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions.NotFoundException;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Shared.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception has occurred: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var traceId = context.TraceIdentifier;

        int statusCode;
        string message;
        List<string>? validationErrors = null;

        if (exception is AppValidEx validEx)
        {
            statusCode = StatusCodes.Status400BadRequest;
            message = "One or more validation failures have occurred.";
            validationErrors = validEx.Errors.Values.SelectMany(x => x).ToList();
        }
        else if (exception is AppUnauthorizedEx)
        {
            statusCode = StatusCodes.Status401Unauthorized;
            message = exception.Message;
        }
        else if (exception is AppConflictEx)
        {
            statusCode = StatusCodes.Status409Conflict;
            message = exception.Message;
        }
        else if (exception is AppNotFoundEx)
        {
            statusCode = StatusCodes.Status404NotFound;
            message = exception.Message;
        }
        else if (exception is AppAppEx)
        {
            statusCode = StatusCodes.Status400BadRequest;
            message = exception.Message;
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            message = "An internal server error occurred.";
        }

        context.Response.StatusCode = statusCode;

        var apiResponse = new ApiResponse
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails
            {
                StatusCode = statusCode,
                Message = message,
                ValidationErrors = validationErrors,
                ExceptionType = exception.GetType().Name
            },
            TraceId = traceId
        };

        return context.Response.WriteAsJsonAsync(apiResponse);
    }
}
