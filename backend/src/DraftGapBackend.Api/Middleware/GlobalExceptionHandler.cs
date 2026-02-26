using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DraftGapBackend.API.Middleware;

/// <summary>
/// Middleware global para manejo centralizado de excepciones.
/// Funcionalidades:
/// - Captura todas las excepciones no manejadas
/// - Formatea respuestas de error de forma consistente  
/// - Registra errores con niveles apropiados
/// - Distingue entre errores de negocio (400) y del servidor (500)
/// Evita que se filtren detalles internos al cliente en producción.
/// </summary>
public static class GlobalExceptionHandler
{
    /// <summary>
    /// Configura el manejo global de excepciones en la aplicación.
    /// Proceso:
    /// 1. Captura la excepción del contexto
    /// 2. Determina el código de estado HTTP apropiado
    /// 3. Registra el error
    /// 4. Retorna respuesta JSON formateada
    /// </summary>
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
