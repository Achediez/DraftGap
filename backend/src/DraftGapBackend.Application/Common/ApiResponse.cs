using System.Collections.Generic;

namespace DraftGapBackend.Application.Common;

/// <summary>
/// Wrapper genérico para respuestas de API con manejo de errores consistente.
/// Uso:
/// - Success: true si la operación fue exitosa
/// - Data: Payload de la respuesta
/// - Error: Mensaje de error único
/// - Errors: Lista de errores (validación múltiple)
/// Factory methods:
/// - SuccessResponse(data): Para operaciones exitosas
/// - ErrorResponse(error): Para errores simples
/// - ValidationErrorResponse(errors): Para errores de validación múltiples
/// </summary>
/// <typeparam name="T">Tipo del payload de datos</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> ErrorResponse(string error) => new()
    {
        Success = false,
        Error = error
    };

    public static ApiResponse<T> ValidationErrorResponse(List<string> errors) => new()
    {
        Success = false,
        Errors = errors
    };
}
