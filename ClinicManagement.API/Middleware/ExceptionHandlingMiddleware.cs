using ClinicManagement.Application.Exceptions;

namespace ClinicManagement.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
                if (context.Response.HasStarted)
                {
                    throw;
                }

                var (statusCode, message) = exception switch
                {
                    KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                    BusinessRuleException => (StatusCodes.Status400BadRequest, exception.Message),
                    _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
                };

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    _logger.LogError(
                        exception,
                        "Unhandled exception while processing {RequestMethod} {RequestPath}. RequestId: {RequestId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);
                }

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(
                    new ExceptionResponse(statusCode, message),
                    context.RequestAborted);
            }
        }

        private sealed record ExceptionResponse(int StatusCode, string Message);
    }
}
