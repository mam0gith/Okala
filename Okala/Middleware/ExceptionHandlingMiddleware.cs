using Okala.Shared;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Unhandled exception");

            context.Response.ContentType = "application/json";

            Result<object> result;
            int statusCode;

            switch (ex)
            {
                case ArgumentException argEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    result = Result.Failure<object>("Invalid input", new List<string> { argEx.Message });
                    break;

                case KeyNotFoundException keyEx:
                    statusCode = StatusCodes.Status404NotFound;
                    result = Result.Failure<object>("Resource not found", new List<string> { keyEx.Message });
                    break;

                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    result = Result.Failure<object>("Unauthorized access");
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    result = Result.Failure<object>("An unexpected error occurred.", new List<string> { ex.Message });
                    break;
            }

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(result);
        }
    }
}