using System.Collections.Generic;

namespace DraftGapBackend.Application.Common;

/// <summary>
/// Standard API response wrapper for consistent error handling
/// </summary>
/// <typeparam name="T">Type of the response data</typeparam>
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
