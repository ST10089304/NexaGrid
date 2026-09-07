using System.Text;

using NexaGrid.Desktop.Forms;

namespace NexaGrid.Desktop;

internal static class Program
{
    private static readonly string ErrorLogPath =
        Path.Combine(
            AppContext.BaseDirectory,
            "nexagrid-startup-error.log");

    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(
            UnhandledExceptionMode.CatchException);

        Application.ThreadException +=
            (_, eventArgs) =>
            {
                HandleFatalException(
                    eventArgs.Exception,
                    "Windows Forms thread error");
            };

        AppDomain.CurrentDomain.UnhandledException +=
            (_, eventArgs) =>
            {
                Exception exception =
                    eventArgs.ExceptionObject
                    as Exception
                    ?? new Exception(
                        eventArgs.ExceptionObject
                            ?.ToString()
                        ?? "An unknown application error occurred.");

                HandleFatalException(
                    exception,
                    "Unhandled application error");
            };

        TaskScheduler.UnobservedTaskException +=
            (_, eventArgs) =>
            {
                eventArgs.SetObserved();

                HandleFatalException(
                    eventArgs.Exception,
                    "Asynchronous task error");
            };

        try
        {
            ApplicationConfiguration.Initialize();

            using var mainForm =
                new MainForm();

            Application.Run(
                mainForm);
        }
        catch (Exception exception)
        {
            HandleFatalException(
                exception,
                "NexaGrid startup error");
        }
    }

    private static void HandleFatalException(
        Exception exception,
        string title)
    {
        string errorDetails =
            BuildErrorDetails(
                exception,
                title);

        try
        {
            File.WriteAllText(
                ErrorLogPath,
                errorDetails,
                Encoding.UTF8);
        }
        catch
        {
            // The message box will still display the error
            // if the log file cannot be created.
        }

        try
        {
            MessageBox.Show(
                errorDetails,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch
        {
            // Avoid another exception while reporting
            // the original startup failure.
        }
    }

    private static string BuildErrorDetails(
        Exception exception,
        string title)
    {
        var builder =
            new StringBuilder();

        builder.AppendLine(title);
        builder.AppendLine();

        builder.AppendLine(
            $"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        builder.AppendLine(
            $"Exception: {exception.GetType().FullName}");

        builder.AppendLine(
            $"Message: {exception.Message}");

        builder.AppendLine();

        builder.AppendLine(
            "Full details:");

        builder.AppendLine(
            exception.ToString());

        return builder.ToString();
    }
}