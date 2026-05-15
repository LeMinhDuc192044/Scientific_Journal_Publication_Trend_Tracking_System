namespace Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

/// <summary>
/// Standardized API response wrapper for all endpoints
/// </summary>
public class ApiResponse<T> where T : class
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public ErrorDetails? Error { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T? data = null, string message = "Request processed successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Created(T data, string message = "Resource created successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(string message, int statusCode = 400, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails { StatusCode = statusCode, Message = message },
            TraceId = traceId
        };
    }

    public static ApiResponse<T> Failure(ErrorDetails error, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = error.Message,
            Error = error,
            TraceId = traceId
        };
    }
}

public class ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public ErrorDetails? Error { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? TraceId { get; init; }

    public static ApiResponse Ok(string message = "Request processed successfully")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public static ApiResponse Failure(string message, int statusCode = 400, string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails { StatusCode = statusCode, Message = message },
            TraceId = traceId
        };
    }
}

public class ErrorDetails
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string>? ValidationErrors { get; init; }
    public string? ExceptionType { get; init; }
}

public class PagedResult<T> where T : class
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
