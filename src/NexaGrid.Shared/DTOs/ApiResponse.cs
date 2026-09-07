namespace NexaGrid.Shared.DTOs;
/* OWASP Foundation (2026) */
public class ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public static ApiResponse<T> Ok(
        T data,
        string message = "Request completed successfully.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }
/* OWASP Foundation (2026) */
    public static ApiResponse<T> Failure(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message
        };
    }
}