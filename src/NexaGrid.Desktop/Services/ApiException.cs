namespace NexaGrid.Desktop.Services;

public class ApiException : Exception
{
    public ApiException(
        string message,
        int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}