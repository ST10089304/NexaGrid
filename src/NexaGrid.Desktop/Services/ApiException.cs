namespace NexaGrid.Desktop.Services;
/*Stack Overflow Community (2016) Proper place for business logic in WinForms applications*/
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