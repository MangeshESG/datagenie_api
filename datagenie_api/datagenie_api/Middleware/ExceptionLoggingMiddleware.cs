using System.Text;

namespace datagenie_api.Middleware
{
    public class ExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionLoggingMiddleware> _logger;

        public ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Request Logging
                context.Request.EnableBuffering();
                var reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                _logger.LogInformation("Request Path: {Path}, Body: {Body}", context.Request.Path, requestBody);

                // Proceed to next middleware
                await _next(context);

                // Response Logging
                _logger.LogInformation("Response Status Code: {StatusCode}", context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Global Exception Caught: {Message}", ex.Message);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";

                await context.Response.WriteAsync($"Something went wrong. Error: {ex.Message}");
            }

        }
    }
}
