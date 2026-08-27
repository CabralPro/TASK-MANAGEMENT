using System.Collections.Generic;

namespace TaskManagement.WebAPI.Models;

/// <summary>
/// Standard API response envelope used by all endpoints.
/// </summary>
/// <typeparam name="T">Payload type returned in <see cref="Data"/>.</typeparam>
public class ApiResponse<T>
{
    /// <summary>Indicates whether the operation succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Response payload when <see cref="Success"/> is true.</summary>
    public T Data { get; init; }

    /// <summary>Human-readable summary message.</summary>
    public string Message { get; init; }

    /// <summary>Validation or business error details when the operation failed.</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>Creates a successful response.</summary>
    public static ApiResponse<T> Ok(T data, string message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    /// <summary>Creates a failed response.</summary>
    public static ApiResponse<T> Fail(string message, params string[] errors) => new()
    {
        Success = false,
        Message = message,
        Errors = errors ?? []
    };
}
