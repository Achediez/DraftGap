using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DraftGapBackend.API.Middleware;

/// <summary>
/// Global exception handler middleware
/// </summary>
public static class GlobalExceptionHandler
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var exception = contextFeature.Error;

                    logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

                    var statusCode = exception switch
                    {
                        UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                        InvalidOperationException => (int)HttpStatusCode.BadRequest,
                        ArgumentException => (int)HttpStatusCode.BadRequest,
                        KeyNotFoundException => (int)HttpStatusCode.NotFound,
                        _ => (int)HttpStatusCode.InternalServerError
                    };

                    context.Response.StatusCode = statusCode;

                    var problemDetails = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = GetTitleForStatusCode(statusCode),
                        Detail = exception.Message,
                        Instance = context.Request.Path
                    };

                    await context.Response.WriteAsJsonAsync(problemDetails);
                }
            });
        });
    }

    private static string GetTitleForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            404 => "Not Found",
            500 => "Internal Server Error",
            _ => "Error"
        };
    }
}
